//using MathNet.Symbolics;
using TedToolkit.Units.Json;

namespace TedToolkit.Units.Analyzer;

internal static class Helpers
{
    public static string GetUnitToSystem(this UnitInfo info, UnitSystem system, BaseDimensions dimensions)
    {
        var expr = WithUnitSystem(info.UnitToBase, dimensions, system, i => i.UnitToBase, i => i.BaseToUnit);
        return expr;
        //return SymbolicExpression.Parse(expr).ToString();
    }

    public static string GetSystemToUnit(this UnitInfo info, UnitSystem system, BaseDimensions dimensions)
    {
        var expr = WithUnitSystem(info.BaseToUnit, dimensions, system, i => i.BaseToUnit, i => i.UnitToBase);
        return expr;
        //return SymbolicExpression.Parse(expr).ToString();
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