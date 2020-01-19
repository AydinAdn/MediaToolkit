using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute
    {
    }
}

namespace MediaToolkit.Util
{
    public static class Extensions
    {
        internal static string Remove(this Enum enumerable, string text)
        {
            return enumerable.ToString()
                .Replace(text, "");
        }

        internal static string ToLower(this Enum enumerable)
        {
            return enumerable.ToString()
                .ToLowerInvariant();
        }
    }
}