using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaToolkit.Core.Events;
using MediaToolkit.Core.Infrastructure;
using MediaToolkit.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace MediaToolkit.Core.Services
{
    public interface IMediaConverterService
    {
        event EventHandler<ProgressUpdateEventArgs> OnProgressUpdateEventHandler;
        event EventHandler<WarningEventArgs> OnWarningEventHandler;
        event EventHandler OnCompleteEventHandler;

        Task ExecuteInstructionAsync(string instruction, CancellationToken token = default);
        Task ExecuteInstructionAsync(IInstructionBuilder instruction, CancellationToken token = default);
    }

    public class MediaConverterService : ProcessService, IMediaConverterService
    {
        public MediaConverterService(IProcessServiceConfiguration configuration = null, ILogger logger = null)
            : base(configuration ?? new FFmpegServiceConfiguration(), logger)
        {
        }

        public MediaConverterService(ILogger logger = null)
            : base(new FFmpegServiceConfiguration(), logger)
        {
        }

        public event EventHandler<ProgressUpdateEventArgs> OnProgressUpdateEventHandler;
        public event EventHandler<WarningEventArgs> OnWarningEventHandler;
        public event EventHandler OnCompleteEventHandler;

        public override event EventHandler<RawDataReceivedEventArgs> OnRawDataReceivedEventHandler;

        public override async Task ExecuteInstructionAsync(string instruction, CancellationToken token = default)
        {
            if (instruction == null)              throw new ArgumentNullException(nameof(instruction));
            if (instruction.IsNullOrWhiteSpace()) throw new ArgumentException("Instructions are empty", nameof(instruction));

            // We're creating a temporary copy of the the process to enable the client the option of processing 
            // multiple files concurrently, each thread owning their own exe process.
            // The copy is deleted once processing has completed or the application has faulted.
            string exeTempPath = await this.GetTempExe();

            Dictionary<string, string> progressValues = new Dictionary<string, string>();

            ProcessStartInfo startInfo = this.GetProcessStartInfo(instruction);
            using (Process process = new Process { StartInfo = startInfo })
            {
                bool started = process.Start();

                this.Logger?.LogInformation("FFmpeg process started? {0}", started);
                process.ErrorDataReceived += (sender, received) =>
                {
                    if (received.Data.IsNullOrWhiteSpace()) return;


                    try
                    {
                        this.OnRawDataReceivedEventHandler?.Invoke(this, new RawDataReceivedEventArgs(received.Data));
                        this.Logger?.LogTrace(received.Data);

                        if (!received.Data.Contains("="))
                        {
                            this.OnWarningEventHandler?.Invoke(this, new WarningEventArgs(received.Data));

                            return;
                        }

                        string[] progressValue = received.Data.Trim()
                                                              .Split('=')
                                                              .Select(x => x.Trim())
                                                              .ToArray();

                        if (progressValue[0] == "progress")
                        {
                            ProgressUpdateEventArgs updateEventArgs = new ProgressUpdateEventArgs(progressValues);
                            this.OnProgressUpdateEventHandler?.Invoke(this, updateEventArgs);

                            if (progressValue[1] == "continue")
                            {
                                progressValues.Clear();
                                return;
                            }

                            if (progressValue[1] == "end")
                            {
                                this.OnCompleteEventHandler?.Invoke(this, EventArgs.Empty);
                                return;
                            }

                            throw new Exception(received.Data);
                        }

                        progressValues.Add(progressValue[0], progressValue[1]);

                        if (!token.IsCancellationRequested) return;

                        this.Logger?.LogInformation("Token has been cancelled, killing FFmpeg process");

                        try
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            process.Kill();
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
                            this.Logger?.LogError(ex, "FFmpeg faulted, killing FFmpeg process.");

                            // ReSharper disable once AccessToDisposedClosure
                            process.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                            // swallow exceptions that are thrown when killing the process, 
                            // one possible candidate is the application ending naturally before we get a chance to kill it
                        }
                    }
                };
                this.Logger?.LogInformation("Begin reading stderr from ffmpeg console");

                process.BeginErrorReadLine();
                process.WaitForExit();

            }

            File.Delete(exeTempPath);
        }

        public override Task ExecuteInstructionAsync(IInstructionBuilder instruction, CancellationToken token = default)
        {
            return this.ExecuteInstructionAsync(instruction.BuildInstructions(), token);
        }
    }
}