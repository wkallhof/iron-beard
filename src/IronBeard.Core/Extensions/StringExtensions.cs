using System;

namespace IronBeard.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsSet(this string s){
            return !string.IsNullOrWhiteSpace(s);
        }

        public static bool IgnoreCaseEquals(this string s, string stringToCompare){
            return s.Equals(stringToCompare, StringComparison.OrdinalIgnoreCase);
        }
    }
}