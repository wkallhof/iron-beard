namespace IronBeard.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsSet(this string s){
            return !string.IsNullOrWhiteSpace(s);
        }
    }
}