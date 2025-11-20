using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using PeterO.Numbers;
using TedToolkit.Quantities.Data;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Conversion = TedToolkit.Quantities.Data.Conversion;

namespace TedToolkit.Quantities.Analyzer;

internal static class Helpers
{
    private static readonly Dictionary<char, char> Superscripts = new()
    {
        ['0'] = '⁰',
        ['1'] = '¹',
        ['2'] = '²',
        ['3'] = '³',
        ['4'] = '⁴',
        ['5'] = '⁵',
        ['6'] = '⁶',
        ['7'] = '⁷',
        ['8'] = '⁸',
        ['9'] = '⁹',
        ['-'] = '⁻'
    };

    public static string ToSuperscript(this int value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        var sb = new StringBuilder(s.Length);
        foreach (var c in s)
        {
            sb.Append(Superscripts.TryGetValue(c, out var i) ? i : c);
        }

        return sb.ToString();
    }

    public static string CreateSummary(string description, IEnumerable<Link> links, string dimension)
    {
        dimension = string.IsNullOrEmpty(dimension) ? dimension : $"<b>{dimension}</b>";
        return $"""
                /// <summary>
                /// {description.Replace("\n", "\n///")}
                /// </summary>
                /// <remarks>{dimension}{string.Join("\t", links.Select(l => l.Remarks))}</remarks>
                """;
    }

    public static DataCollection GetData(string? fileName, IEnumerable<string> jsons, string[] quantities)
    {
        var asm = typeof(QuantitiesGenerator).Assembly;

        string? resourceName = null;
        if (fileName is not null)
        {
            resourceName = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName + ".json", StringComparison.OrdinalIgnoreCase));
        }

        resourceName ??= asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("ISQ.json", StringComparison.OrdinalIgnoreCase));

        JObject jObject;
        {
            using var stream = asm.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            jObject = JObject.Parse(reader.ReadToEnd());
        }

        foreach (var json in jsons)
        {
            AppendObject(JObject.Parse(json));
        }

        var result = jObject?.ToObject<DataCollection>() ?? throw new NullReferenceException();

        return result with
        {
            Quantities = result.Quantities
                .Where(QuantityPredict)
                .ToDictionary(i => i.Key, i => i.Value)
        };

        bool QuantityPredict(KeyValuePair<string, Quantity> pair)
        {
            if (fileName is null) return pair.Value.IsBasic;
            if (quantities.Length is not 0) return pair.Value.IsBasic || quantities.Contains(pair.Key);
            return true;
        }

        void AppendObject(JObject obj)
        {
            jObject.Merge(obj, new JsonMergeSettings
            {
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                MergeArrayHandling = MergeArrayHandling.Union,
            });
        }
    }

    public static ExpressionSyntax GetSystemToUnit(this Unit unit, UnitSystem system, Dimension dimension,
        ITypeSymbol dataType)
    {
        var conversion = ToSystemConversion(system, dimension)?.TransformTo(unit.Conversion);
        return ToExpression(conversion, dataType, "Value");
    }

    public static ExpressionSyntax GetUnitToSystem(this Unit unit, UnitSystem system, Dimension dimension,
        ITypeSymbol dataType)
    {
        var conversion = unit.Conversion.TransformTo(ToSystemConversion(system, dimension));
        return ToExpression(conversion, dataType, "value");
    }

    private static ExpressionSyntax ToExpression(Conversion? conversion, ITypeSymbol dataType, string argument)
    {
        if (conversion is null || !conversion.Value.IsValid)
        {
            return ThrowExpression(ObjectCreationExpression(IdentifierName("global::System.NotImplementedException"))
                .WithArgumentList(
                    ArgumentList()));
        }

        ExpressionSyntax multiple = conversion.Value.Multiplier.ToEDecimal().Equals(EDecimal.One)
            ? IdentifierName(argument)
            : BinaryExpression(
                SyntaxKind.MultiplyExpression,
                CreateNumber(conversion.Value.Multiplier, dataType),
                IdentifierName(argument));

        if (conversion.Value.Offset.IsZero) return multiple;
        return BinaryExpression(
            SyntaxKind.AddExpression,
            multiple,
            CreateNumber(conversion.Value.Offset, dataType));
    }

    public static Conversion? ToSystemConversion(UnitSystem system, Dimension dimension)
    {
        Conversion? result = Conversion.Unit;
        foreach (var systemKey in system.Keys)
        {
            var unitConversion = system.GetUnit(systemKey).Conversion;
            var conversion = unitConversion.Pow(dimension.GetExponent(systemKey));
            result = result?.Merge(conversion);
        }

        return result;
    }

    public static decimal ToDecimal(ERational data)
    {
        try
        {
            var dec = data.ToEDecimal();
            if (!dec.IsNaN())
            {
                return dec.ToDecimal();
            }
        }
        catch (OverflowException)
        {
        }

        return (decimal)data.Numerator.ToInt64Unchecked() / data.Denominator.ToInt64Unchecked();
    }

    private static ExpressionSyntax CreateNumber(ERational data, ITypeSymbol dataType)
    {
        var dec = data.ToEDecimal();
        if (!dec.IsNaN())
        {
            return CreateNumber(dec, dataType);
        }

        return BinaryExpression(
            SyntaxKind.DivideExpression,
            CreateNumber(data.Numerator, dataType),
            CreateNumber(data.Denominator, dataType));
    }

    private static LiteralExpressionSyntax CreateNumber(EDecimal data, ITypeSymbol dataType)
    {
        var num = data.ToString();

        if (!IsFloatingPoint(dataType))
        {
            if (int.TryParse(num, out var i))
            {
                return LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(i));
            }

            if (uint.TryParse(num, out var u))
            {
                return LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(u));
            }

            if (long.TryParse(num, out var l))
            {
                return LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(l));
            }

            if (ulong.TryParse(num, out var ul))
            {
                return LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(ul));
            }
        }

        if (dataType.SpecialType is SpecialType.System_Decimal && decimal.TryParse(num, out var m))
        {
            return LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(num + "m", m));
        }

        if (double.TryParse(num, out var d))
        {
            return LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(num + "d", d));
        }

        return LiteralExpression(
            SyntaxKind.NumericLiteralExpression,
            Literal(num));
    }

    public static bool IsFloatingPoint(this ITypeSymbol type)
    {
        return type.SpecialType is SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal;
    }
}