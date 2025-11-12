// Licensed under MIT No Attribution, see LICENSE file at the root.
// Copyright 2013 Andreas Gullberg Larsen (andreas.larsen84@gmail.com). Maintained at https://github.com/angularsen/UnitsNet.

namespace TedToolkit.Units.Generator.JsonTypes;

/// <summary>
///     Localization of a unit, such as unit abbreviations in different languages.
/// </summary>
internal struct Localization()
{
    /// <summary>
    ///     The unit abbreviations. Can be empty for dimensionless units like Ratio.DecimalFraction.
    /// </summary>
    public string[] Abbreviations { get; set; } = [];
    
    /// <summary>
    ///     Explicit configuration of unit abbreviations for prefixes.
    ///     This is typically used for languages or special unit abbreviations where you cannot simply prepend SI prefixes like
    ///     "k" for kilo
    ///     to the abbreviations defined in <see cref="Localization.Abbreviations" />.
    /// </summary>
    /// <example>
    ///     Energy.ThermEc unit has "Abbreviations": "th (E.C.)" and "AbbreviationsForPrefixes": { "Deca": "Dth (E.C.)" } since
    ///     the SI prefix for Deca is "Da" and "Dath (E.C.)" is not the conventional form.
    /// </example>
    /// <remarks>
    ///     The unit abbreviation value can either be a string or an array of strings. Typically the number of abbreviations
    ///     for a prefix matches that of "Abbreviations" array, but this is not required.
    /// </remarks>
    //public Dictionary<Prefix, string[]> AbbreviationsForPrefixes { get; set; } = [];

    /// <summary>
    ///     The name of the culture this is a localization for.
    /// </summary>
    public string Culture { get; set; } = string.Empty;
}