using System.Text.Json;

namespace TypescriptModelGenerator;

public static class StringExtensions
{
    public static string ToCamelCase(this string str) => JsonNamingPolicy.CamelCase.ConvertName(str);
}