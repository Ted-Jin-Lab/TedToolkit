using System.Text.RegularExpressions;
using MathNet.Symbolics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Units.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TedToolkit.Units.Analyzer;

internal static class Helpers
{
    public static ExpressionSyntax ToExpression(this string expression, string parameterName, ITypeSymbol dataType)
    {
        try
        {
            expression = expression.SetExpressionValue("value");
            expression = SymbolicExpression.Parse(expression).ToString();
            expression = Unit.RemoveSci(expression);
            expression = expression
                .AddPostfix(dataType)
                .Replace("PI", "global::System.Math.PI")
                .Replace("value", parameterName);
            return ParseExpression(expression);
        }
        catch (Exception e)
        {
            return LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(expression + ": " + e.Message));
        }
    }

    private static string AddPostfix(this string formula, ITypeSymbol dataType)
    {
        return Regex.Replace(formula, @"\d+(?:\.\d+)?", m =>
        {
            var num = m.Value;

            if (!IsFloatingPoint(dataType) && !num.Contains('.'))
            {
                if (int.TryParse(num, out _))
                {
                    return num;
                }

                if (long.TryParse(num, out _))
                {
                    return num + "l";
                }
            }

            if (dataType.SpecialType is SpecialType.System_Decimal && decimal.TryParse(num, out _))
            {
                return num + "m";
            }

            return num + "d";
        });

        static bool IsFloatingPoint(ITypeSymbol type)
        {
            return type.SpecialType is SpecialType.System_Single
                or SpecialType.System_Double
                or SpecialType.System_Decimal;
        }
    }

    public static string GetUnitToSystem(this UnitInfo info, UnitSystem system, BaseDimensions dimensions)
    {
        var expression = info.UnitToBase;
        ModifyOne(dimensions.N, system.AmountOfSubstance);
        ModifyOne(dimensions.I, system.ElectricCurrent);
        ModifyOne(dimensions.L, system.Length);
        ModifyOne(dimensions.J, system.LuminousIntensity);
        ModifyOne(dimensions.M, system.Mass);
        ModifyOne(dimensions.Θ, system.Temperature);
        ModifyOne(dimensions.T, system.Time);
        return expression;

        void ModifyOne(int index, UnitInfo unitInfo)
        {
            if (index is 0) return;
            var str = index > 0 ? unitInfo.UnitToBase : unitInfo.BaseToUnit;
            for (var i = 0; i < Math.Abs(index); i++)
            {
                expression = str.SetExpressionValue(expression);
            }
        }
    }

    public static string GetSystemToUnit(this UnitInfo info, UnitSystem system, BaseDimensions dimensions)
    {
        var expression = "{x}";
        ModifyOne(dimensions.N, system.AmountOfSubstance);
        ModifyOne(dimensions.I, system.ElectricCurrent);
        ModifyOne(dimensions.L, system.Length);
        ModifyOne(dimensions.J, system.LuminousIntensity);
        ModifyOne(dimensions.M, system.Mass);
        ModifyOne(dimensions.Θ, system.Temperature);
        ModifyOne(dimensions.T, system.Time);
        expression = info.UnitToBase.SetExpressionValue(expression);

        return expression;

        void ModifyOne(int index, UnitInfo unitInfo)
        {
            if (index is 0) return;
            var str = index > 0 ? unitInfo.BaseToUnit : unitInfo.UnitToBase;
            for (var i = 0; i < Math.Abs(index); i++)
            {
                expression = str.SetExpressionValue(expression);
            }
        }
    }

    public static List<string> GetAllCommonSubstrings(string a, string b)
    {
        var dp = new int[a.Length + 1, b.Length + 1];
        var candidates = new List<(int len, int end)>();

        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                if (a[i - 1] != b[j - 1]) continue;
                dp[i, j] = dp[i - 1, j - 1] + 1;
                candidates.Add((dp[i, j], i));
            }
        }

        candidates = candidates
            .Where(c => c.len > 0)
            .OrderByDescending(c => c.len)
            .ToList();

        var result = new List<(int start, int end)>();
        var used = new List<(int start, int end)>();

        foreach (var (len, end) in candidates)
        {
            var start = end - len;

            if (used.Any(u => !(end <= u.start || start >= u.end)))
                continue;

            used.Add((start, end: end));
        }

        used.Sort((x, y) => x.start.CompareTo(y.start));

        return used.Select(u => a.Substring(u.start, u.end - u.start)).ToList();
    }

    public static int GetCommonCharactersCount(string a, string b)
    {
        var list = GetAllCommonSubstrings(a, b);
        return list.Sum(s => s.Length);
    }
}