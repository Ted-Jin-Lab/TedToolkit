using System.Text.RegularExpressions;

namespace TedToolkit.RoslynHelper.Extensions;

/// <summary>
///     For the string extensions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Make a string to pascal case.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToPascalCase(this string input)
    {
        return string.Join(".", input.Split('.').Select(ConvertToPascalCase));

        static string ConvertToPascalCase(string input)
        {
            Regex invalidCharsRgx = new(@"[^_a-zA-Z0-9]");
            Regex whiteSpace = new(@"(?<=\s)");
            Regex startsWithLowerCaseChar = new("^[a-z]");
            Regex firstCharFollowedByUpperCasesOnly = new("(?<=[A-Z])[A-Z0-9]+$");
            Regex lowerCaseNextToNumber = new("(?<=[0-9])[a-z]");
            Regex upperCaseInside = new("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

            var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(input, "_"), string.Empty)
                .Split(['_'], StringSplitOptions.RemoveEmptyEntries)
                .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
                .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
                .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
                .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));

            return string.Concat(pascalCase);
        }
    }

    /// <summary>
    ///     Add leading string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="leading"></param>
    /// <returns></returns>
    public static string Leading(this string input, string leading)
    {
        return leading + input.Replace("\n", "\n" + leading);
    }
}