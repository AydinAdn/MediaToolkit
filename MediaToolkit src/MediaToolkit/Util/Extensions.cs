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
        private const int BuffSize = 16 * 1024;

        internal static void CopyTo(this Stream input, Stream output)
        {
            var buffer = new byte[BuffSize];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) output.Write(buffer, 0, bytesRead);
        }

        // ReSharper disable once UnusedMember.Global
        public static string FormatInvariant(this string value, params object[] args)
        {
            try
            {
                return value == null
                    ? string.Empty
                    : string.Format(CultureInfo.InvariantCulture, value, args);
            }
            catch (FormatException)
            {
                return value;
            }
        }

        internal static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim()
                       .Length == 0;
        }

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

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var t in collection) action(t);
        }
    }
}