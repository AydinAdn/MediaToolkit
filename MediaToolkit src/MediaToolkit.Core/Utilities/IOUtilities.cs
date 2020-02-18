using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

namespace MediaToolkit.Core.Utilities
{
    public class IOUtilities
    {
        public async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using (FileStream sourceStream      = new FileStream(sourceFile,      FileMode.Open,      FileAccess.Read,  FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (FileStream destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
        }

        public string ChangeFilePathName(string from, string to)
        {
            if (from.Length < 1 || from.IsNullOrWhiteSpace()) throw new ArgumentException("Path is empty", nameof(from));
            if (to.Length   < 1 || to  .IsNullOrWhiteSpace()) throw new ArgumentException("Path is empty", nameof(to));

            string fileName      = Path.GetFileName (from);
            string fileExtension = Path.GetExtension(fileName);

            if (fileExtension.IsNullOrWhiteSpace()) return from.Replace(fileName, to);

            int index = fileName.LastIndexOf(fileExtension, StringComparison.Ordinal);
            fileName  = fileName.Substring  (0, index);

            return from.Replace(fileName, to);
        }

        public void DecompressResourceStream(string resourceId, string toPath)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            using (Stream resourceStream = currentAssembly.GetManifestResourceStream(resourceId))
            {
                if (resourceStream == null) throw new Exception("GZip stream is null"); 

                using (FileStream fileStream = new FileStream(toPath, FileMode.Create))
                using (GZipStream compressedStream = new GZipStream(resourceStream, CompressionMode.Decompress))
                {
                    compressedStream.CopyTo(fileStream);
                }
            }
        }
    }

}
