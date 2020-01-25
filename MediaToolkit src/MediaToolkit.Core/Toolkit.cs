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
using System.Collections.Generic;
using System.Linq;
using MediaToolkit.Core.Events;

namespace MediaToolkit.Core
{
    public class Toolkit
    {
        readonly ILogger logger;
        private readonly string ffmpegExePath;
        private long isInitialized = 0;

        public EventHandler<ProgressUpdateEventArgs> OnProgressUpdateEventHandler;
        public EventHandler<WarningEventArgs> OnWarningEventHandler;
        public EventHandler OnCompleteEventHandler;


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

        public async Task ExecuteInstruction(IInstructionBuilder instructionBuilder, CancellationToken token)
        {
            if (Interlocked.Read(ref this.isInitialized) == 0)
            {
                this.CreateDirectoryIfMissing(this.ffmpegExePath);
                await this.RestoreFFmpegFileIfMissing(this.ffmpegExePath);
                Interlocked.Increment(ref this.isInitialized);
            }

            // We're creating a temporary copy of the ffmpeg.exe to enable the client the option of processing 
            // multiple files concurrently, each process having their own exe.
            // The copy is deleted once processing has completed or the application has faulted.
            string ffmpegExeCopyPath = this.ChangeFileName(this.ffmpegExePath, Path.GetRandomFileName());
            await this.CopyFileAsync(this.ffmpegExePath, ffmpegExeCopyPath);

            string instructions = instructionBuilder.BuildInstructions();

            this.logger.LogInformation("Executing instructions: {0}", instructions);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = "-nostdin -progress pipe:2 -y -loglevel warning " + instructions,
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

                this.logger.LogInformation("FFmpeg process started? {0}", started);

                Dictionary<string, string> progressValues = new Dictionary<string, string>();

                ffmpegProcess.ErrorDataReceived += (sender, received) =>
                {
                    if (received.Data == null) return;

                    try
                    {
                        this.logger.LogTrace(received.Data);

                        if (!received.Data.Contains("="))
                        {
                            this.OnWarningEventHandler?.Invoke(this, new WarningEventArgs(received.Data));

                            return;
                        }

                        string[] progressValue = received.Data.Trim()
                                                              .Split('=')
                                                              .Select(x=>x.Trim())
                                                              .ToArray();

                        if (progressValue[0] == "progress")
                        {
                            ProgressUpdateEventArgs updateEventArgs = new ProgressUpdateEventArgs(progressValues);
                            this.OnProgressUpdateEventHandler?.Invoke(this, updateEventArgs);

                            switch (progressValue[1])
                            {
                                case "continue":
                                    progressValues.Clear();
                                    return;

                                case "end":
                                    this.OnCompleteEventHandler.Invoke(this, EventArgs.Empty);

                                    return;
                                default:
                                    throw new Exception(received.Data);
                            }
                        }

                        progressValues.Add(progressValue[0], progressValue[1]);



                        if (!token.IsCancellationRequested) return;

                        this.logger.LogInformation("Token has been cancelled, killing FFmpeg process");

                        try
                        {
                            // ReSharper disable once AccessToDisposedClosure
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

                this.logger.LogInformation("Begin reading stderr from ffmpeg console");

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
            //if (!File.Exists(ffmpegFilePath))
            //{
                this.logger.LogInformation("FFmpeg file not found. Unpacking embedded FFmpeg.exe to {0}", ffmpegFilePath);
                await this.UnpackFFmpegExecutableAsync(ffmpegFilePath);
            //}
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
    }
}
