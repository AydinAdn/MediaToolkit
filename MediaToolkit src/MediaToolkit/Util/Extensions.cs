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
        private const int BUFF_SIZE = 16*1024;

        internal static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[Extensions.BUFF_SIZE];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) { output.Write(buffer, 0, bytesRead); }
        }

        public static string FormatInvariant(this string value, params object[] args)
        {
            try
            {
                return value == null
                    ? string.Empty
                    : string.Format(CultureInfo.InvariantCulture, value, args);
            }
            catch (FormatException ex) {
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
            if (action == null) throw new ArgumentNullException("action");

            foreach (T t in collection) action(t);
        }
    }
}