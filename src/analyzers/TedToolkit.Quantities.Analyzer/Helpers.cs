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

    public static string CreateSummary(string description, IEnumerable<Link> links)
    {
        return $"""
                /// <summary>
                /// {description.Replace("\n", "\n///")}
                /// </summary>
                /// <remarks>{string.Join("\t", links.Select(l => l.Remarks))}</remarks>
                """;
    }

    public static DataCollection GetData(params IEnumerable<string> jsons)
    {
        JObject? jObject = null;
        var asm = typeof(QuantitiesGenerator).Assembly;
        foreach (var se in asm.GetManifestResourceNames()
                     .Where(n => n.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
        {
            using var stream = asm.GetManifestResourceStream(se)!;
            using var reader = new StreamReader(stream);
            var obj = JObject.Parse(reader.ReadToEnd());
            AppendObject(obj);
        }

        foreach (var json in jsons)
        {
            AppendObject(JObject.Parse(json));
        }

        return jObject?.ToObject<DataCollection>() ?? throw new NullReferenceException();

        void AppendObject(JObject obj)
        {
            if (jObject is null)
            {
                jObject = obj;
            }
            else
            {
                jObject.Merge(obj, new JsonMergeSettings
                {
                    MergeNullValueHandling = MergeNullValueHandling.Merge,
                    MergeArrayHandling = MergeArrayHandling.Replace
                });
            }
        }
    }

    public static ExpressionSyntax GetSystemToUnit(this Unit unit, UnitSystem system, Dimension dimension,
        ITypeSymbol dataType)
    {
        var conversion = ToSystemConversion(system, dimension)?.TransformTo(unit.Conversion);
        return ToExpression(conversion, dataType);
    }

    public static ExpressionSyntax GetUnitToSystem(this Unit unit, UnitSystem system, Dimension dimension,
        ITypeSymbol dataType)
    {
        var conversion = unit.Conversion.TransformTo(ToSystemConversion(system, dimension));
        return ToExpression(conversion, dataType);
    }

    private static ExpressionSyntax ToExpression(Conversion? conversion, ITypeSymbol dataType)
    {
        if (conversion is null || !conversion.Value.IsValid)
        {
            return ThrowExpression(ObjectCreationExpression(IdentifierName("global::System.NotImplementedException"))
                .WithArgumentList(
                    ArgumentList()));
        }

        ExpressionSyntax multiple = conversion.Value.Multiplier.ToEDecimal().Equals(EDecimal.One)
            ? IdentifierName("Value")
            : BinaryExpression(
                SyntaxKind.MultiplyExpression,
                CreateNumber(conversion.Value.Multiplier, dataType),
                IdentifierName("Value"));

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
                Literal(num + "m",m));
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

    static bool IsFloatingPoint(ITypeSymbol type)
    {
        return type.SpecialType is SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal;
    }

    // public static ExpressionSyntax ToExpression(this string expression, string parameterName, ITypeSymbol dataType)
    // {
    //     try
    //     {
    //         expression = expression.SetExpressionValue("value");
    //         expression = SymbolicExpression.Parse(expression).ToString();
    //         expression = Unit.RemoveSci(expression);
    //         expression = expression
    //             .AddPostfix(dataType)
    //             .Replace("PI", "global::System.Math.PI")
    //             .Replace("value", parameterName);
    //         return ParseExpression(expression);
    //     }
    //     catch (Exception e)
    //     {
    //         return LiteralExpression(
    //             SyntaxKind.StringLiteralExpression,
    //             Literal(expression + ": " + e.Message));
    //     }
    // }
    //
    //

    // }
    //
    // public static string GetUnitToSystem(this UnitInfo info, UnitSystem system, BaseDimensions dimensions)
    // {
    //     var expression = info.UnitToBase;
    //     ModifyOne(dimensions.N, system.AmountOfSubstance);
    //     ModifyOne(dimensions.I, system.ElectricCurrent);
    //     ModifyOne(dimensions.L, system.Length);
    //     ModifyOne(dimensions.J, system.LuminousIntensity);
    //     ModifyOne(dimensions.M, system.Mass);
    //     ModifyOne(dimensions.Θ, system.Temperature);
    //     ModifyOne(dimensions.T, system.Time);
    //     return expression;
    //
    //     void ModifyOne(int index, UnitInfo unitInfo)
    //     {
    //         if (index is 0) return;
    //         var str = index > 0 ? unitInfo.UnitToBase : unitInfo.BaseToUnit;
    //         for (var i = 0; i < Math.Abs(index); i++)
    //         {
    //             expression = str.SetExpressionValue(expression);
    //         }
    //     }
    // }
    //
    // public static string GetSystemToUnit(this UnitInfo info, UnitSystem system, BaseDimensions dimensions)
    // {
    //     var expression = "{x}";
    //     ModifyOne(dimensions.N, system.AmountOfSubstance);
    //     ModifyOne(dimensions.I, system.ElectricCurrent);
    //     ModifyOne(dimensions.L, system.Length);
    //     ModifyOne(dimensions.J, system.LuminousIntensity);
    //     ModifyOne(dimensions.M, system.Mass);
    //     ModifyOne(dimensions.Θ, system.Temperature);
    //     ModifyOne(dimensions.T, system.Time);
    //     expression = info.UnitToBase.SetExpressionValue(expression);
    //
    //     return expression;
    //
    //     void ModifyOne(int index, UnitInfo unitInfo)
    //     {
    //         if (index is 0) return;
    //         var str = index > 0 ? unitInfo.BaseToUnit : unitInfo.UnitToBase;
    //         for (var i = 0; i < Math.Abs(index); i++)
    //         {
    //             expression = str.SetExpressionValue(expression);
    //         }
    //     }
    // }
    //
    // public static List<string> GetAllCommonSubstrings(string a, string b)
    // {
    //     var dp = new int[a.Length + 1, b.Length + 1];
    //     var candidates = new List<(int len, int end)>();
    //
    //     for (var i = 1; i <= a.Length; i++)
    //     {
    //         for (var j = 1; j <= b.Length; j++)
    //         {
    //             if (a[i - 1] != b[j - 1]) continue;
    //             dp[i, j] = dp[i - 1, j - 1] + 1;
    //             candidates.Add((dp[i, j], i));
    //         }
    //     }
    //
    //     candidates = candidates
    //         .Where(c => c.len > 0)
    //         .OrderByDescending(c => c.len)
    //         .ToList();
    //
    //     var result = new List<(int start, int end)>();
    //     var used = new List<(int start, int end)>();
    //
    //     foreach (var (len, end) in candidates)
    //     {
    //         var start = end - len;
    //
    //         if (used.Any(u => !(end <= u.start || start >= u.end)))
    //             continue;
    //
    //         used.Add((start, end: end));
    //     }
    //
    //     used.Sort((x, y) => x.start.CompareTo(y.start));
    //
    //     return used.Select(u => a.Substring(u.start, u.end - u.start)).ToList();
    // }
    //
    // public static int GetCommonCharactersCount(string a, string b)
    // {
    //     var list = GetAllCommonSubstrings(a, b);
    //     return list.Sum(s => s.Length);
    // }
}