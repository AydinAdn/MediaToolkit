using MediaToolkit.Model;
using MediaToolkit.Options;
using MediaToolkit.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace MediaToolkit
{
    public sealed class Engine : IDisposable
    {
        /// <summary>
        ///     Used for locking the FFmpeg process to one thread.
        /// </summary>
        private static readonly string LockName = "MediaToolkit.Engine.LockName";
        private static readonly string FFmpegFilePath = Path.GetTempPath() + "/MediaToolkit/ffmpeg.exe";
        private Process _ffmpegProcess;
        private Mutex mutex;

        public void Dispose()
        {
            _ffmpegProcess = null;
        }

        public event EventHandler<ConversionCompleteEventArgs> ConversionCompleteEvent;
        public event EventHandler<ConvertProgressEventArgs> ConvertProgressEvent;

        private void OnProgressChanged(ConvertProgressEventArgs e)
        {
            EventHandler<ConvertProgressEventArgs> handler = ConvertProgressEvent;
            if (handler != null) handler(this, e);
        }

        private void OnConversionComplete(ConversionCompleteEventArgs e)
        {
            EventHandler<ConversionCompleteEventArgs> handler = ConversionCompleteEvent;
            if (handler != null) handler(this, e);
        }


        /// <summary>
        ///     <para> --- </para>
        ///     <para> Initializes FFmpeg.exe; Ensuring that there is a copy</para>
        ///     <para> in the clients temp folder & isn't in use by another process.</para>
        /// </summary>
        public Engine()
        {
            mutex = new Mutex(false, LockName);

            string ffmpegDirectory = "" + Path.GetDirectoryName(FFmpegFilePath);

            if (!Directory.Exists(ffmpegDirectory)) Directory.CreateDirectory(ffmpegDirectory);

            if (File.Exists(FFmpegFilePath))
            {
                if (!Document.IsFileLocked(new FileInfo(FFmpegFilePath))) return;

                try
                {
                    mutex.WaitOne();
                    KillFFmpegProcesses();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                UnpackFFmpegExecutable();
            }
        }

        /// <summary>
        ///     Retrieve a thumbnail image from a video file.
        /// </summary>
        /// <param name="inputFile">Video file</param>
        /// <param name="outputFile">Image file</param>
        /// <param name="options">Conversion options</param>
        public void GetThumbnail(MediaFile inputFile, MediaFile outputFile, ConversionOptions options)
        {
            var engineParams = new EngineParameters
            {
                InputFile = inputFile,
                OutputFile = outputFile,
                ConversionOptions = options,
                Task = FFmpegTask.GetThumbnail
            };

            FFmpegEngine(engineParams);
        }

        /// <summary>
        ///     <para> ---</para>
        ///     <para> Retrieve media metadata</para>
        /// </summary>
        /// <param name="inputFile">Retrieves the metadata for the input file</param>
        public void GetMetadata(MediaFile inputFile)
        {
            var engineParams = new EngineParameters
            {
                InputFile = inputFile,
                Task = FFmpegTask.GetMetaData
            };

            FFmpegEngine(engineParams);
        }

        /// <summary>
        ///     <para> ---</para>
        ///     <para> Converts media with conversion options</para>
        /// </summary>
        /// <param name="inputFile">Input file</param>
        /// <param name="outputFile">Output file</param>
        /// <param name="options">Conversion options</param>
        public void Convert(MediaFile inputFile, MediaFile outputFile, ConversionOptions options)
        {
            var engineParams = new EngineParameters
            {
                InputFile = inputFile,
                OutputFile = outputFile,
                ConversionOptions = options,
                Task = FFmpegTask.Convert
            };

            FFmpegEngine(engineParams);
        }

        /// <summary>
        ///     <para> ---</para>
        ///     <para> Converts media with default options</para>
        /// </summary>
        public void Convert(MediaFile inputFile, MediaFile outputFile)
        {
            var engineParams = new EngineParameters
            {
                InputFile = inputFile,
                OutputFile = outputFile,
                Task = FFmpegTask.Convert
            };

            FFmpegEngine(engineParams);
        }

        private static void KillFFmpegProcesses()
        {
            Process[] ffmpegProcesses = Process.GetProcessesByName("ffmpeg");
            if (ffmpegProcesses.Length > 0)
                foreach (Process process in ffmpegProcesses)
                {
                    // pew pew pew...
                    process.Kill();
                    // let it die...
                    Thread.Sleep(200);
                }
        }

        private static void UnpackFFmpegExecutable()
        {
            Stream ffmpegStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("MediaToolkit.Resources.FFmpeg.exe.gz");

            if (ffmpegStream == null) throw new Exception("FFMpeg GZip stream is null");

            using (var tempFileStream = new FileStream(FFmpegFilePath, FileMode.Create))
            using (var gZipStream = new GZipStream(ffmpegStream, CompressionMode.Decompress))
            {
                gZipStream.CopyTo(tempFileStream);
            }
        }

        /// <summary>
        ///     Where the magic happens
        /// </summary>
        /// <param name="engineParameters">The engine parameters</param>
        private void FFmpegEngine(EngineParameters engineParameters)
        {
            if (!File.Exists(engineParameters.InputFile.Filename))
                throw new FileNotFoundException("Input file not found", engineParameters.InputFile.Filename);

            try
            {
                mutex.WaitOne();
                StartFFmpegProcess(engineParameters);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private void StartFFmpegProcess(EngineParameters engineParameters)
        {
            var receivedMessagesLog = new List<string>();
            var totalMediaDuration = new TimeSpan();

            ProcessStartInfo processStartInfo = GenerateStartInfo(engineParameters);

            using (_ffmpegProcess = Process.Start(processStartInfo))
            {
                Exception caughtException = null;
                if (_ffmpegProcess == null) throw new InvalidOperationException("FFmpeg process is not running.");

                _ffmpegProcess.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs received)
                {
                    if (received.Data == null) return;

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
                                engineParameters.InputFile.Metadata = new Metadata();

                            TimeSpan.TryParse(matchDuration.Groups[1].Value, out totalMediaDuration);
                            engineParameters.InputFile.Metadata.Duration = totalMediaDuration;
                        }

                        ConversionCompleteEventArgs convertCompleteEvent;
                        ConvertProgressEventArgs progressEvent;

                        if (RegexEngine.IsProgressData(received.Data, out progressEvent))
                        {
                            progressEvent.TotalDuration = totalMediaDuration;
                            OnProgressChanged(progressEvent);
                        }
                        else if (RegexEngine.IsConvertCompleteData(received.Data, out convertCompleteEvent))
                        {
                            convertCompleteEvent.TotalDuration = totalMediaDuration;
                            OnConversionComplete(convertCompleteEvent);
                        }
                    }
                    catch (Exception ex)
                    {
                        // catch the exception and kill the process since we're in a faulted state
                        caughtException = ex;

                        try
                        {
                            _ffmpegProcess.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                            // swallow exceptions that are thrown when killing the process, 
                            //one possible candidate is the application ending naturally before we get a chance to kill it
                        }
                    }
                };

                _ffmpegProcess.BeginErrorReadLine();
                _ffmpegProcess.WaitForExit();

                if ((_ffmpegProcess.ExitCode != 0 && _ffmpegProcess.ExitCode != 1) || caughtException != null)
                    throw new Exception(_ffmpegProcess.ExitCode + ": " + receivedMessagesLog[1] +
                                        receivedMessagesLog[0], caughtException);
            }
        }


        private ProcessStartInfo GenerateStartInfo(EngineParameters engineParameters)
        {
            string ffmpegCommand = CommandBuilder.Serialize(engineParameters);

            return new ProcessStartInfo
            {
                Arguments = "-nostdin -y -loglevel info " + ffmpegCommand,
                FileName = FFmpegFilePath,
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetTempPath()
            };
        }

        /// <summary>
        ///     Configures the engine to perform the correct task
        /// </summary>
        internal class EngineParameters
        {
            internal MediaFile InputFile { get; set; }
            internal MediaFile OutputFile { get; set; }
            internal ConversionOptions ConversionOptions { get; set; }
            internal FFmpegTask Task { get; set; }
        }

        internal enum FFmpegTask
        {
            Convert,
            GetMetaData,
            GetThumbnail
        }
    }
}