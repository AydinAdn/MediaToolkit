using System.IO;

namespace MediaToolkit.Util
{
    using System;

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
                if (fileStream != null)
                    fileStream.Close();
            }

            return false;
        }


        internal static bool IsLocked(string filePath)
        {
            if (filePath.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("filePath");
            }

            FileInfo file = new FileInfo(filePath);
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
                if (fileStream != null)
                    fileStream.Close();
            }

            return false;
        }


    }
}