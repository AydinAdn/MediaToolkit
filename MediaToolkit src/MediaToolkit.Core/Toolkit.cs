using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaToolkit.Core.Infrastructure;
using MediaToolkit.Core.Utilities;

namespace MediaToolkit.Core
{
    public class Toolkit : IDisposable
    {
        readonly ILogger logger;
        private readonly string ffmpegExePath;

        public Toolkit(ILogger logger) : this(logger, Directory.GetCurrentDirectory() + @"/MediaToolkit/ffmpeg.exe")
        {
        }

        /// <param name="logger">Logger</param>
        /// <param name="ffmpegPath">Custom path of ffmpegFile</param>
        public Toolkit(ILogger logger, string ffmpegPath)
        {
            this.logger = logger;
            this.ffmpegExePath = ffmpegPath;
        }

        public async Task ExecuteInstruction(IInstruction instruction, CancellationToken token)
        {
            this.CreateDirectoryIfMissing(this.ffmpegExePath);
            await this.RestoreFFmpegFileIfMissing(this.ffmpegExePath);

            // We're creating a temporary copy of the ffmpeg.exe to enable the client the option of processing multiple files concurrently, each process having their own exe.
            // The file is deleted once processing has completed or the application has faulted.
            string ffmpegExeCopyPath = this.ChangeFileName(this.ffmpegExePath, Path.GetRandomFileName());
            await this.CopyFileAsync(this.ffmpegExePath, ffmpegExeCopyPath);

            ProcessStartInfo startInfo = new ProcessStartInfo
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
            
            using (Process ffmpegProcess = new Process { StartInfo = startInfo })
            {
                bool started = ffmpegProcess.Start();

                this.logger.Log(LogLevel.Information, "FFmpeg process started: {0}", started);


                ffmpegProcess.ErrorDataReceived += (sender, received) =>
                {
                    if (received.Data == null) return;

                    try
                    {
                        this.logger.LogTrace(received.Data);

                        if (!token.IsCancellationRequested) return;

                        this.logger.LogInformation("Token has been cancelled, killing FFmpeg process");

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
                    catch (Exception ex)
                    {
                        // catch the exception and kill the process since we're in a faulted state
                        //caughtException = ex;
                        
                        try
                        {
                            this.logger.LogError(ex, "FFmpeg faulted, killing FFmpeg process.");

                            ffmpegProcess.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                            // swallow exceptions that are thrown when killing the process, 
                            // one possible candidate is the application ending naturally before we get a chance to kill it
                        }
                    }
                };

                this.logger.LogInformation("Begin reading from ffmpeg console");

                ffmpegProcess.BeginErrorReadLine();
                ffmpegProcess.WaitForExit();
                this.logger.LogInformation("FFmpeg process has completed");

            }

            this.logger.LogInformation("Deleting {0}", ffmpegExeCopyPath);
            File.Delete(ffmpegExeCopyPath);
            this.logger.LogInformation("Deleted", ffmpegExeCopyPath);
        }

        #region Utilities

        private void CreateDirectoryIfMissing(string directory)
        {
            if (directory.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(directory));

            this.logger.LogInformation("Checking that FFmpeg directory exists at {0}", directory);

            string directoryPath = Path.GetDirectoryName(directory);

            if (Directory.Exists(directoryPath)) return;

            this.logger.LogInformation("Directory not found. Creating directory for FFmpeg at {0}", directory);
            Directory.CreateDirectory(directoryPath);
        }

        private async Task RestoreFFmpegFileIfMissing(string ffmpegFilePath)
        {
            this.logger.LogInformation("Checking that the specified FFmpeg file exists at {0}", ffmpegFilePath);
            if (!File.Exists(ffmpegFilePath))
            {
                this.logger.LogInformation("FFmpeg file not found. Unpacking embedded FFmpeg.exe to {0}", ffmpegFilePath);
                await this.UnpackFFmpegExecutableAsync(ffmpegFilePath);
            }
        }

        private async Task UnpackFFmpegExecutableAsync(string path)
        {
            const string resourceId = "MediaToolkit.Core.Resources.FFmpeg.exe.gz";

            Assembly currentAssembly= Assembly.GetExecutingAssembly();

            this.logger.LogInformation("Locating compressed FFmpeg.exe in embedded resources");
            using (Stream zippedFFmpeg = currentAssembly.GetManifestResourceStream(resourceId))
            {
                if (zippedFFmpeg == null)
                {
                    this.logger.LogError("Compressed FFmpeg.exe resource stream is null");

                    throw new Exception("FFmpeg GZip stream is null");
                }

                this.logger.LogInformation("Zipped FFmpeg.exe found in resources");


                this.logger.LogInformation("Begin decompressing FFmpeg.exe.gz");

                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                using (GZipStream compressedStream = new GZipStream(zippedFFmpeg, CompressionMode.Decompress))
                {
                    await compressedStream.CopyToAsync(fileStream);
                }
            }

            this.logger.LogInformation("FFmpeg.exe unpacked to {0}", path);
        }

        private async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            this.logger.LogInformation("Copying FFmpeg.exe to {0}", destinationFile);

            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (FileStream destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream);

            this.logger.LogInformation("Successfully copied to {0}", destinationFile);
        }

        private string ChangeFileName(string from, string to)
        {
            string dir = Path.GetDirectoryName(from);
            return Path.Combine(dir, to + Path.GetExtension(from));
        }

        #endregion

        #region IDisposable Support
        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.isDisposed = true;
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
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
