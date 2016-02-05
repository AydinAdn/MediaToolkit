using System;
using System.Collections.Generic;
using System.Diagnostics;
using MediaToolkit.Util;

namespace MediaToolkit.Features
{
    public class MetadataProvider : IMetadataProvider
    {
        const string DefaultExiftoolPath = "/MediaToolkit/exiftool.exe";

        public MetadataProvider()
        {
            Document.Decompress("MediaToolkit.Resources.Exiftool.exe.gz", DefaultExiftoolPath);

            this.isDisposed = false;
            this.exiftoolProcess = new Process();
        }

        private bool isDisposed;

        private readonly Process exiftoolProcess;

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing) return;
            if (this.isDisposed) return;

            this.isDisposed = true;
            this.exiftoolProcess.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public IMetadata GetMetadata(string filename)
        {
            exiftoolProcess.StartInfo = new ProcessStartInfo
            {
                Arguments = "\"{0}\"".FormatInvariant(filename),
                FileName = DefaultExiftoolPath,
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Dictionary<string, string> metadataIndex = new Dictionary<string, string>();

            exiftoolProcess.OutputDataReceived += (sender, args) =>
            {
                if (args.Data.Contains("ExifTool Version Number")) return;

                var keyValue = args.Data.Split(':');
                metadataIndex.Add(keyValue[0], keyValue[1]);
            };

            exiftoolProcess.Start();
            exiftoolProcess.BeginOutputReadLine();
            exiftoolProcess.WaitForExit();

            return new Metadata {MetadataIndex = metadataIndex};
        }
    }
}