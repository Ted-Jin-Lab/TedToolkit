namespace TedToolkit.Units.Json;

public readonly record struct UnitInfo(Unit Unit, Prefix Prefix)
{
    public PrefixInfo PrefixInfo => PrefixInfo.Entries[Prefix];

    public string Name => Prefix is Prefix.None ? Unit.SingularName : Prefix + Unit.SingularName;
    public string Names => Prefix is Prefix.None ? Unit.PluralName : Prefix + Unit.PluralName;

    public Dictionary<string, string[]> LocalNames
    {
        get
        {
            var prefix = Prefix;
            if (prefix is 0)
            {
                return Unit.Localization.ToDictionary(i => i.Culture, i => i.Abbreviations);
            }
            var prefixInfo = PrefixInfo;
            return Unit.Localization.ToDictionary(i => i.Culture, i =>
            {
                List<string> listResult = [];
                if (i.AbbreviationsForPrefixes?.TryGetValue(prefix, out var relay) ?? false)
                {
                    listResult.AddRange(relay);
                }

                if (!prefixInfo.CultureToPrefix.TryGetValue(i.Culture, out var prefixPrefix))
                    prefixPrefix = prefixInfo.SiPrefix;

                listResult.AddRange(i.Abbreviations.Select(a => prefixPrefix + a));
                return listResult.ToArray();
            });
        }
    }

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