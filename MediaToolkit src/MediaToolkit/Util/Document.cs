using System;
using System.IO;

namespace MediaToolkit.Util
{
    public class Document
    {
        [Obsolete("Replaced by the method `MediaToolkit.Util.Document.IsLocked`")]
        internal static bool IsFileLocked(FileInfo file)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                fileStream?.Close();
            }

            return false;
        }


        internal static bool IsLocked(string filePath)
        {
            if (filePath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(filePath));

            var file = new FileInfo(filePath);
            FileStream fileStream = null;

            try
            {
                fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                fileStream?.Close();
            }

            return false;
        }
    }
}