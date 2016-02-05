using System;
using System.Globalization;
using System.IO;

namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}
namespace MediaToolkit.Util
{
    public static class Extensions
    {
        public static string FormatInvariant(this string value, params object[] args)
        {
            try
            {
                return value == null ? string.Empty : string.Format(CultureInfo.InvariantCulture, value, args);
            }
            catch (FormatException ex)
            {
                return value;
            }
        }

        internal static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        const int BUFF_SIZE = 16 * 1024;
        internal static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[BUFF_SIZE];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }
        internal static string ToLower(this Enum enumerable)
        {
            return enumerable.ToString().ToLowerInvariant();
        }

        internal static string Remove(this Enum enumerable, string text)
        {
            return enumerable.ToString().Replace(text, "");
        }

        //internal static bool IsNullOrWhiteSpace(this string text)
        //{
        //    return string.IsNullOrWhiteSpace(text);
        //}
    }
}
