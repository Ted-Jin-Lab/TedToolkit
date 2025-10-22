using System.Collections;
using System.Text.RegularExpressions;

namespace TedToolkit.Assertions.Utils;

internal static class GeneralHelper
{
    internal static Match PlaceHolderMatch(this string input, string placeholder)
    {
        var pattern = $@"\{{({placeholder})(:.*?)?\}}";
        return Regex.Match(input, pattern);
    }

    internal static string ReplacePlaceHolder(this string input, string placeholder, string replacementValue)
    {
        var pattern = $@"\{{({placeholder})(:.*?)?\}}";
        var replacement = $"{{{replacementValue}$2}}";
        return Regex.Replace(input, pattern, replacement);
    }

    internal static string GetFullName(this Type type)
    {
        if (!type.IsGenericType)
            return GetTypeName(type);

        var genericArgs = string.Join(", ", type.GetGenericArguments()
            .Select(GetFullName));

        var typeName = GetTypeName(type.GetGenericTypeDefinition());
        typeName = typeName[..typeName.IndexOf('`')];

        return $"{typeName}<{genericArgs}>";

        static string GetTypeName(Type type)
        {
            return type.FullName ?? type.Name;
        }
    }

    internal static string GetObjectString(this object? obj)
    {
        return obj switch
        {
            string str => str,
            IEnumerable enumerable => GetItemsString(enumerable),
            _ => obj?.ToString() ?? "<null>"
        };
    }

    private static string GetItemsString(IEnumerable items)
    {
        const int maxItems = 10;

        object?[] objects = [..items];
        return objects.Length > maxItems
            ? $"({objects.Length})[{string.Join(", ", objects.Take(maxItems).Select(GetObjectString))}, ...]"
            : $"[{string.Join(", ", objects.Select(GetObjectString))}]";
    }

    internal static int Count(this IEnumerable enumerable)
    {
        return Enumerable.Count(enumerable.Cast<object?>());
    }
}