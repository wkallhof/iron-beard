namespace IronBeard.Core.Extensions;

/// <summary>
/// String Extensions
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Short, inverted helper for IsNullOrWhitespace
    /// </summary>
    /// <param name="s">String to check value on</param>
    /// <returns>True if value set, False if not</returns>
    public static bool IsSet(this string? s){
        return !string.IsNullOrWhiteSpace(s);
    }

    /// <summary>
    /// Quick way to compare two strings with OrdinalIgnoreCase
    /// </summary>
    /// <param name="s">Target string</param>
    /// <param name="stringToCompare">String to compare to</param>
    /// <returns>True if equals, False if not</returns>
    public static bool IgnoreCaseEquals(this string s, string stringToCompare){
        return s.Equals(stringToCompare, StringComparison.OrdinalIgnoreCase);
    }
}
