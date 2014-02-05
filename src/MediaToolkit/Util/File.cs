using System.IO;

namespace MediaToolkit.Util
{
    public class Document
    {
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
    }
}