using System.IO;
using System.IO.Compression;
using System.Reflection;
using MediaToolkit.Properties;

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

        public static void Decompress(string resource, string toPath)
        {
            if (resource.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("resource");
            }

            if (toPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("toPath");
            }

            Stream compressedResourceStream = Assembly.GetExecutingAssembly()
                                                      .GetManifestResourceStream(resource);

            if (compressedResourceStream == null)
            {
                throw new Exception(Resources.Exceptions_Null_CompressedResourceStream);
            }

            using (FileStream fileStream = new FileStream(toPath, FileMode.Create))
            using (GZipStream compressedStream = new GZipStream(compressedResourceStream, CompressionMode.Decompress))
            {
                compressedStream.CopyTo(fileStream);
            }
        }
    }
}