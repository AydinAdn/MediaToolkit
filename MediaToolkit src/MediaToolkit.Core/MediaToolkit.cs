using MediaToolkit.Core.CommandHandler;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.Core
{
    public class MediaToolkit : IDisposable
    {
        ILogger logger;
        private string ffmpegExePath;

        public MediaToolkit(ILogger logger) : this(logger, @"/MediaToolkit/ffmpeg.exe")
        {
        }

        /// <param name="ffmpegPath">Custom path of ffmpegFile</param>
        public MediaToolkit(ILogger logger, string ffmpegPath)
        {
            this.logger = logger;
            this.ffmpegExePath = ffmpegPath;
        }

        public async Task ExecuteInstruction(IInstruction instruction, CancellationToken token)
        {
            EnsureDirectoryExists(this.ffmpegExePath);
            await EnsureFFmpegFileExistsAsync(this.ffmpegExePath);

            string ffmpegExeCopyPath = this.ChangeFileName(this.ffmpegExePath, Path.GetRandomFileName());
            await CopyFileAsync(ffmpegExePath, ffmpegExeCopyPath);

            var startInfo = new ProcessStartInfo
            {
                Arguments = "-nostdin -y -loglevel info " + instruction.Instruction,
                FileName = ffmpegExeCopyPath,
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (var ffmpegProcess = Process.Start(startInfo))
            {
                ffmpegProcess.ErrorDataReceived += (sender, received) =>
                {
                    if (received.Data == null) return;

                    try
                    {
                        logger.LogInformation(received.Data);
                        if (token.IsCancellationRequested)
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception ex)
                    {
                        // catch the exception and kill the process since we're in a faulted state
                        //caughtException = ex;
                        
                        try
                        {
                            ffmpegProcess.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                            // swallow exceptions that are thrown when killing the process, 
                            // one possible candidate is the application ending naturally before we get a chance to kill it
                        }
                    }
                };

                ffmpegProcess.BeginErrorReadLine();
                ffmpegProcess.WaitForExit();
            }
        }

        private void EnsureDirectoryExists(string directory)
        {
            //string directory = Path.GetDirectoryName(this.FFmpegFilePath) ?? Directory.GetCurrentDirectory(); ;
            logger.LogInformation("Checking that FFmpeg directory exists at {0}", directory);
            if (!Directory.Exists(directory))
            {
                logger.LogInformation("Directory not found. Creating directory for FFmpeg at {0}", directory);
                Directory.CreateDirectory(directory);
            }
        }

        private async Task EnsureFFmpegFileExistsAsync(string ffmpegFilePath)
        {
            logger.LogInformation("Checking that the specified FFmpeg file exists at {0}", ffmpegFilePath);
            if (!File.Exists(ffmpegFilePath))
            {
                logger.LogInformation("FFmpeg file not found. Unpacking embedded FFmpeg.exe to {0}", ffmpegFilePath);
                await UnpackFFmpegExecutableAsync(ffmpegFilePath);
            }
        }

        private async Task UnpackFFmpegExecutableAsync(string path)
        {
            logger.LogInformation("Locating compressed FFmpeg.exe");

            Stream compressedFFmpegStream = Assembly.GetExecutingAssembly()
                                                    .GetManifestResourceStream("MediaToolkit.Core.Resources.FFmpeg.exe.gz");

            if (compressedFFmpegStream == null)
            {
                logger.LogError("Compressed FFmpeg.exe resource stream is null");

                throw new Exception("FFmpeg GZip stream is null");
            }

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            using (GZipStream compressedStream = new GZipStream(compressedFFmpegStream, CompressionMode.Decompress))
            {
                await compressedStream.CopyToAsync(fileStream);
            }
            
            logger.LogInformation("FFmpeg.exe unpacked to {0}", path);
        }

        private async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream);
        }

        private string ChangeFileName(string from, string to)
        {
            var dir = Path.GetDirectoryName(from);
            return Path.Combine(dir, to + Path.GetExtension(from));
        }

        #region IDisposable Support
        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MediaToolkit()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
