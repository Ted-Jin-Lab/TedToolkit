namespace TedToolkit.Units.Json;

public readonly record struct UnitInfo(Unit Unit, Prefix Prefix)
{
    public PrefixInfo PrefixInfo => PrefixInfo.Entries[Prefix];

    public string Name => Prefix is Prefix.None ? Unit.SingularName : Prefix + Unit.SingularName;

    public string UnitToBase
    {
        get
        {
            var expr = Unit.UnitToBase;
            if (Prefix is not Prefix.None)
            {
                expr = PrefixInfo.UnitToBase.SetExpressionValue(expr);
            }

            return expr;
        }
    }

    public string BaseToUnit
    {
        get
        {
            var expr = Unit.BaseToUnit;
            if (Prefix is not Prefix.None)
            {
                expr = PrefixInfo.BaseToUnit.SetExpressionValue(expr);
            }

            return expr;
        }
    }
}