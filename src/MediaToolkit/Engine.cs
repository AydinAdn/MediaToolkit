using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using MediaToolkit.Model;
using MediaToolkit.Options;
using MediaToolkit.Util;

namespace MediaToolkit
{
    public sealed class Engine : IDisposable
    {
        private const string FfmpegDirectory = "/MediaToolkit/";
        internal static readonly object Lock = new object();
        internal Process FFMpegProcess;

        private string _ffMpegFilePath;

        public void Dispose()
        {
            FFMpegProcess = null;
            _ffMpegFilePath = null;
        }

        /// <summary>
        ///     Initializes ffmpeg tool by ensuring that
        ///     <para>there is a copy in the temp folder</para>
        ///     <para>and is not being used by another process.</para>
        /// </summary>
        private void Init()
        {
            string mediaToolkitFolder = Path.GetTempPath() + FfmpegDirectory;

            if (!Directory.Exists(mediaToolkitFolder)) Directory.CreateDirectory(mediaToolkitFolder);

            _ffMpegFilePath = mediaToolkitFolder + "ffmpeg.exe";

            if (File.Exists(_ffMpegFilePath))
            {
                if (!Document.IsFileLocked(new FileInfo(_ffMpegFilePath))) return;

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
            else
            {
                Stream ffmpegStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("MediaToolkit.Resources.ffmpeg.exe.gz");

                if (ffmpegStream == null) throw new Exception("FFMpeg GZip stream is null");

                using (var tempFileStream = new FileStream(_ffMpegFilePath, FileMode.Create))
                using (var gZipStream = new GZipStream(ffmpegStream, CompressionMode.Decompress))
                {
                    gZipStream.CopyTo(tempFileStream);
                }
            }
        }

        public static event EventHandler<ConvertProgressChangedEventArgs> ConvertProgressEvent;

        /// <summary>
        ///     Converts media with conversion options
        /// </summary>
        /// <param name="iFile">Input file</param>
        /// <param name="oFile">Output file</param>
        /// <param name="options">Conversion options</param>
        public void Convert(MediaFile iFile, MediaFile oFile, ConversionOptions options)
        {
            if (!File.Exists(iFile.Filename)) throw new FileNotFoundException("Input file couldn't be found!");
            ConvertEngine(iFile, oFile, options);
        }

        /// <summary>
        ///     Converts media
        /// </summary>
        /// <param name="iFile">Input file</param>
        /// <param name="oFile">Output file</param>
        public void Convert(MediaFile iFile, MediaFile oFile)
        {
            ConvertEngine(iFile, oFile, null);
        }


        private void ConvertEngine(MediaFile iFile, MediaFile oFile, ConversionOptions options)
        {
            lock (Lock)
            {
                Init();
                string conversionArgs = CommandBuilder.Convert(iFile, oFile, options);

                var totalMediaDuration = new TimeSpan();

                ProcessStartInfo processStartInfo = CreateProcessStartInfo(conversionArgs);

                string lastReceivedMessage = "";
                using (FFMpegProcess = Process.Start(processStartInfo))
                {
                    if (FFMpegProcess == null) throw new InvalidOperationException("FFMpeg process is not running");

                    FFMpegProcess.ErrorDataReceived += delegate(object o, DataReceivedEventArgs e)
                    {
                        if (e.Data == null) return;

                        // Logging received messages; if error occurs last message explains why.
                        lastReceivedMessage = e.Data;

                        Dictionary<Find, Regex> regexIndex = RegexLibrary.Index;
                        Match matchDuration = regexIndex[Find.Duration].Match(e.Data);
                        Match matchFrame = regexIndex[Find.ConvertProgressFrame].Match(e.Data);
                        Match matchFps = regexIndex[Find.ConvertProgressFps].Match(e.Data);
                        Match matchSize = regexIndex[Find.ConvertProgressSize].Match(e.Data);
                        Match matchTime = regexIndex[Find.ConvertProgressTime].Match(e.Data);
                        Match matchBitrate = regexIndex[Find.ConvertProgressBitrate].Match(e.Data);

                        // Log the length of the loaded media
                        if (matchDuration.Success)
                            TimeSpan.TryParse(matchDuration.Groups[1].Value, out totalMediaDuration);

                        if (!matchFrame.Success || !matchFps.Success || !matchSize.Success || !matchTime.Success ||
                            !matchBitrate.Success) return;

                        // If each Regex is successful, raise the ConvertProgressChangedEvent.
                        TimeSpan processedDuration;
                        TimeSpan.TryParse(matchTime.Groups[1].Value, out processedDuration);

                        long frame = System.Convert.ToInt64(matchFrame.Groups[1].Value);
                        double fps = System.Convert.ToDouble(matchFps.Groups[1].Value);
                        int sizeKb = System.Convert.ToInt32(matchSize.Groups[1].Value);
                        double bitrate = System.Convert.ToDouble(matchBitrate.Groups[1].Value);
                        TimeSpan totalDuration = totalMediaDuration;

                        ConvertProgressEvent(this,
                            new ConvertProgressChangedEventArgs(processedDuration, totalDuration, frame, fps, sizeKb,
                                bitrate));
                    };

                    FFMpegProcess.BeginErrorReadLine();
                    FFMpegProcess.WaitForExit();

                    if (FFMpegProcess.ExitCode != 0 && FFMpegProcess.ExitCode != 1)
                        throw new Exception(lastReceivedMessage);
                }
            }
        }

        private ProcessStartInfo CreateProcessStartInfo(string arguments)
        {
            return new ProcessStartInfo
            {
                Arguments = "-nostdin -y -loglevel info " + arguments,
                FileName = _ffMpegFilePath,
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetTempPath()
            };
        }
    }
}