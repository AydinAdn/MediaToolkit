namespace MediaToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;

    using MediaToolkit.Model;
    using MediaToolkit.Options;
    using MediaToolkit.Properties;
    using MediaToolkit.Util;

    ///-------------------------------------------------------------------------------------------------
    /// <summary>   An engine. This class cannot be inherited. </summary>
    public sealed class Engine : IDisposable
    {
        private bool isDisposed;

        /// <summary>   Used for locking the FFmpeg process to one thread. </summary>
        private const string LockName = "MediaToolkit.Engine.LockName";

        private const string DefaultFFmpegFilePath = @"/MediaToolkit/ffmpeg.exe";

        /// <summary>   Full pathname of the fmpeg file. </summary>
        private readonly string ffmpegFilePath;

        /// <summary>   The mutex. </summary>
        private readonly Mutex mutex;

        /// <summary>   The ffmpeg process. </summary>
        private Process ffmpegProcess;


        public Engine()
            : this(ConfigurationManager.AppSettings["mediaToolkit.ffmpeg.path"])
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     <para> Initializes FFmpeg.exe; Ensuring that there is a copy</para>
        ///     <para> in the clients temp folder &amp; isn't in use by another process.</para>
        /// </summary>
        public Engine(string ffMpegPath)
        {
            this.mutex = new Mutex(false, LockName);
            this.isDisposed = false;

            if (ffMpegPath.IsNullOrWhiteSpace())
            {
                ffMpegPath = DefaultFFmpegFilePath;
            }

            this.ffmpegFilePath = ffMpegPath;

            this.EnsureDirectoryExists ();
            this.EnsureFFmpegFileExists();
            this.EnsureFFmpegIsNotUsed ();
        }

        private void EnsureFFmpegIsNotUsed()
        {
            if (!Document.IsLocked(this.ffmpegFilePath)) return;

            try
            {
                this.mutex.WaitOne();
                KillFFmpegProcesses();
            }
            finally
            {
                this.mutex.ReleaseMutex();
            }
        }

        private void EnsureDirectoryExists()
        {
            string directory = Path.GetDirectoryName(this.ffmpegFilePath) ?? Directory.GetCurrentDirectory(); ;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void EnsureFFmpegFileExists()
        {
            if (!File.Exists(this.ffmpegFilePath))
            {
                UnpackFFmpegExecutable(this.ffmpegFilePath);  
            }
        }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        /// <remarks>   Aydin Aydin, 30/03/2015. </remarks>
        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || this.isDisposed)
            {
                return;
            }

            KillFFmpegProcesses();
            this.ffmpegProcess = null;
            this.isDisposed = true;
        }

        /// <summary>   Event queue for all listeners interested in conversionComplete events. </summary>
        public event EventHandler<ConversionCompleteEventArgs> ConversionCompleteEvent;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     <para> ---</para>
        ///     <para> Converts media with conversion options</para>
        /// </summary>
        /// <param name="inputFile">    Input file. </param>
        /// <param name="outputFile">   Output file. </param>
        /// <param name="options">      Conversion options. </param>
        public void Convert(MediaFile inputFile, MediaFile outputFile, ConversionOptions options)
        {
            EngineParameters engineParams = new EngineParameters
                                                {
                                                    InputFile = inputFile,
                                                    OutputFile = outputFile,
                                                    ConversionOptions = options,
                                                    Task = FFmpegTask.Convert
                                                };

            this.FFmpegEngine(engineParams);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     <para> ---</para>
        ///     <para> Converts media with default options</para>
        /// </summary>
        /// <param name="inputFile">    Input file. </param>
        /// <param name="outputFile">   Output file. </param>
        public void Convert(MediaFile inputFile, MediaFile outputFile)
        {
            EngineParameters engineParams = new EngineParameters
                                                {
                                                    InputFile = inputFile,
                                                    OutputFile = outputFile,
                                                    Task = FFmpegTask.Convert
                                                };

            this.FFmpegEngine(engineParams);
        }

        /// <summary>   Event queue for all listeners interested in convertProgress events. </summary>
        public event EventHandler<ConvertProgressEventArgs> ConvertProgressEvent;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Where the magic happens. </summary>
        /// <exception cref="FileNotFoundException">    Thrown when the requested file is not present.
        /// </exception>
        /// <param name="engineParameters"> The engine parameters. </param>
        private void FFmpegEngine(EngineParameters engineParameters)
        {
            if (!engineParameters.InputFile.Filename.StartsWith("http://")
                && !File.Exists(engineParameters.InputFile.Filename))
            {
                throw new FileNotFoundException(Resources.Exception_Media_Input_File_Not_Found, engineParameters.InputFile.Filename);
            }

            try
            {
                this.mutex.WaitOne();
                this.StartFFmpegProcess(engineParameters);
            }
            finally
            {
                this.mutex.ReleaseMutex();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Generates a start information. </summary>
        /// <param name="engineParameters"> The engine parameters. </param>
        /// <returns>   The start information. </returns>
        private ProcessStartInfo GenerateStartInfo(EngineParameters engineParameters)
        {
            string ffmpegCommand = CommandBuilder.Serialize(engineParameters);

            return new ProcessStartInfo
            {
                Arguments = "-nostdin -y -loglevel info " + ffmpegCommand,
                FileName = this.ffmpegFilePath,
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     <para> Retrieve media metadata</para>
        /// </summary>
        /// <param name="inputFile">    Retrieves the metadata for the input file. </param>
        public void GetMetadata(MediaFile inputFile)
        {
            EngineParameters engineParams = new EngineParameters
            {
                InputFile = inputFile,
                Task = FFmpegTask.GetMetaData
            };

            this.FFmpegEngine(engineParams);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Retrieve a thumbnail image from a video file. </summary>
        /// <param name="inputFile">    Video file. </param>
        /// <param name="outputFile">   Image file. </param>
        /// <param name="options">      Conversion options. </param>
        public void GetThumbnail(MediaFile inputFile, MediaFile outputFile, ConversionOptions options)
        {
            EngineParameters engineParams = new EngineParameters
            {
                InputFile = inputFile,
                OutputFile = outputFile,
                ConversionOptions = options,
                Task = FFmpegTask.GetThumbnail
            };

            this.FFmpegEngine(engineParams);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Kill f fmpeg processes. </summary>
        private static void KillFFmpegProcesses()
        {
            Process[] ffmpegProcesses = Process.GetProcessesByName(Resources.FFmpegProcessName);
            if (ffmpegProcesses.Length > 0)
            {
                foreach (Process process in ffmpegProcesses)
                {
                    // pew pew pew...
                    process.Kill();
                    // let it die...
                    Thread.Sleep(200);
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Raises the conversion complete event. </summary>
        /// <remarks>   Aydin Aydin, 30/03/2015. </remarks>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        private void OnConversionComplete(ConversionCompleteEventArgs e)
        {
            EventHandler<ConversionCompleteEventArgs> handler = this.ConversionCompleteEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Raises the convert progress event. </summary>
        /// <remarks>   Aydin Aydin, 30/03/2015. </remarks>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        private void OnProgressChanged(ConvertProgressEventArgs e)
        {
            EventHandler<ConvertProgressEventArgs> handler = this.ConvertProgressEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Starts f fmpeg process. </summary>
        /// <remarks>   Aydin Aydin, 30/03/2015. </remarks>
        /// <exception cref="InvalidOperationException">    Thrown when the requested operation is
        ///                                                 invalid.
        /// </exception>
        /// <exception cref="Exception">                    Thrown when an exception error condition
        ///                                                 occurs.
        /// </exception>
        /// <param name="engineParameters"> The engine parameters. </param>
        private void StartFFmpegProcess(EngineParameters engineParameters)
        {
            List<string> receivedMessagesLog = new List<string>();
            TimeSpan totalMediaDuration = new TimeSpan();

            ProcessStartInfo processStartInfo = this.GenerateStartInfo(engineParameters);

            using (this.ffmpegProcess = Process.Start(processStartInfo))
            {
                Exception caughtException = null;
                if (this.ffmpegProcess == null)
                {
                    throw new InvalidOperationException(Resources.Exceptions_FFmpeg_Process_Not_Running);
                }

                this.ffmpegProcess.ErrorDataReceived += (sender, received) =>
                {
                    if (received.Data == null)
                    {
                        return;
                    }

#if (DebugToConsole)
                    Console.WriteLine(received.Data);
#endif
                    try
                    {
                        receivedMessagesLog.Insert(0, received.Data);

                        RegexEngine.TestVideo(received.Data, engineParameters);
                        RegexEngine.TestAudio(received.Data, engineParameters);

                        Match matchDuration = RegexEngine.Index[RegexEngine.Find.Duration].Match(received.Data);
                        if (matchDuration.Success)
                        {
                            if (engineParameters.InputFile.Metadata == null)
                            {
                                engineParameters.InputFile.Metadata = new Metadata();
                            }

                            TimeSpan.TryParse(matchDuration.Groups[1].Value, out totalMediaDuration);
                            engineParameters.InputFile.Metadata.Duration = totalMediaDuration;
                        }

                        ConversionCompleteEventArgs convertCompleteEvent;
                        ConvertProgressEventArgs progressEvent;

                        if (RegexEngine.IsProgressData(received.Data, out progressEvent))
                        {
                            progressEvent.TotalDuration = totalMediaDuration;
                            this.OnProgressChanged(progressEvent);
                        }
                        else if (RegexEngine.IsConvertCompleteData(received.Data, out convertCompleteEvent))
                        {
                            convertCompleteEvent.TotalDuration = totalMediaDuration;
                            this.OnConversionComplete(convertCompleteEvent);
                        }
                    }
                    catch (Exception ex)
                    {
                        // catch the exception and kill the process since we're in a faulted state
                        caughtException = ex;

                        try
                        {
                            this.ffmpegProcess.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                            // swallow exceptions that are thrown when killing the process, 
                            //one possible candidate is the application ending naturally before we get a chance to kill it
                        }
                    }
                };

                this.ffmpegProcess.BeginErrorReadLine();
                this.ffmpegProcess.WaitForExit();

                if ((this.ffmpegProcess.ExitCode != 0 && this.ffmpegProcess.ExitCode != 1) || caughtException != null)
                {
                    throw new Exception(
                        this.ffmpegProcess.ExitCode + ": " + receivedMessagesLog[1] + receivedMessagesLog[0],
                        caughtException);
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Unpack ffmpeg executable. </summary>
        /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
        private static void UnpackFFmpegExecutable(string path)
        {
            Stream compressedFFmpegStream = Assembly.GetExecutingAssembly()
                                                    .GetManifestResourceStream(Resources.FFmpegManifestResourceName);

            if (compressedFFmpegStream == null)
            {
                throw new Exception(Resources.Exceptions_Null_FFmpeg_Gzip_Stream);
            }

            using (FileStream fileStream       = new FileStream(path,                   FileMode.Create))
            using (GZipStream compressedStream = new GZipStream(compressedFFmpegStream, CompressionMode.Decompress))
            {
                compressedStream.CopyTo(fileStream);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Configures the engine to perform the correct task. </summary>
        internal class EngineParameters
        {
            ///-------------------------------------------------------------------------------------------------
            /// <summary>   Gets or sets options for controlling the conversion. </summary>
            /// <value> Options that control the conversion. </value>
            internal ConversionOptions ConversionOptions { get; set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>   Gets or sets the input file. </summary>
            /// <value> The input file. </value>
            internal MediaFile InputFile { get; set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>   Gets or sets the output file. </summary>
            /// <value> The output file. </value>
            internal MediaFile OutputFile { get; set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>   Gets or sets the task. </summary>
            /// <value> The task. </value>
            internal FFmpegTask Task { get; set; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Values that represent fmpeg tasks. </summary>
        internal enum FFmpegTask
        {
            /// <summary>   An enum constant representing the convert option. </summary>
            Convert,

            /// <summary>   An enum constant representing the get meta data option. </summary>
            GetMetaData,

            /// <summary>   An enum constant representing the get thumbnail option. </summary>
            GetThumbnail
        }
    }
}