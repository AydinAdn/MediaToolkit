using System;

namespace MediaToolkit.Core.Utilities
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string text)
        {
            return String.IsNullOrWhiteSpace(text);
        }
    }
}
