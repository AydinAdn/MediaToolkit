using System;

namespace MediaToolkit.Util
{
    internal static class Extensions
    {
        internal static string ToLower(this Enum enumerable)
        {
            return enumerable.ToString().ToLowerInvariant();
        }

        internal static string Remove(this Enum enumerable, string text)
        {
            return enumerable.ToString().Replace(text, "");
        }
    }
}