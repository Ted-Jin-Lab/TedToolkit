using MathNet.Symbolics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Units.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;
namespace TedToolkit.Units.Analyzer;

internal static class Helpers
{
    public static ExpressionSyntax ToExpression(this string expression, string parameterName, bool simplify)
    {
        try
        {
            expression = expression.SetExpressionValue("value");
            if (simplify)
            {
                expression = SymbolicExpression.Parse(expression).ToString(); //TODO: Better simplify.
            }
            expression = expression
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
    
    public static string GetUnitToSystem(this UnitInfo info, UnitSystem system, BaseDimensions dimensions)
    {
        return WithUnitSystem(info.UnitToBase, dimensions, system, i => i.UnitToBase, i => i.BaseToUnit);
    }

    public static string GetSystemToUnit(this UnitInfo info, UnitSystem system, BaseDimensions dimensions)
    {
        return WithUnitSystem(info.BaseToUnit, dimensions, system, i => i.BaseToUnit, i => i.UnitToBase);
    }

    private static string WithUnitSystem(string expression, BaseDimensions dimensions, UnitSystem system,
        Func<UnitInfo, string> bigger, Func<UnitInfo, string> smaller)
    {
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
            var modifier = index > 0 ? bigger : smaller;
            var str = modifier(unitInfo);
            for (var i = 0; i < Math.Abs(index); i++)
            {
                expression = str.SetExpressionValue(expression);
            }
        }
    }
}