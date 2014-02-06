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
        internal Process FFmpegProcess;

        private string _ffMpegFilePath;

        public void Dispose()
        {
            FFmpegProcess = null;
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
                    .GetManifestResourceStream("MediaToolkit.Resources.FFmpeg.exe.gz");

                if (ffmpegStream == null) throw new Exception("FFMpeg GZip stream is null");

                using (var tempFileStream = new FileStream(_ffMpegFilePath, FileMode.Create))
                using (var gZipStream = new GZipStream(ffmpegStream, CompressionMode.Decompress))
                {
                    gZipStream.CopyTo(tempFileStream);
                }
            }
        }

        public static event EventHandler<ConvertProgressEventArgs> ConvertProgressEvent;


        public void GetMetaData(MediaFile inputFile)
        {
            var engineParams = new EngineParams
            {
                InputFile = inputFile,
                Task = FFmpegTask.GetMetaData
            };

            FFmpegEngine(engineParams);
        }

        /// <summary>
        ///     Converts media with conversion options
        /// </summary>
        /// <param name="inputFile">Input file</param>
        /// <param name="outputFile">Output file</param>
        /// <param name="options">Conversion options</param>
        public void Convert(MediaFile inputFile, MediaFile outputFile, ConversionOptions options)
        {
            var engineParams = new EngineParams
            {
                InputFile = inputFile,
                OutputFile = outputFile,
                ConversionOptions = options,
                Task = FFmpegTask.Convert
            };

            FFmpegEngine(engineParams);
        }

        /// <summary>
        ///     Converts media
        /// </summary>
        /// <param name="inputFile">Input file</param>
        /// <param name="outputFile">Output file</param>
        public void Convert(MediaFile inputFile, MediaFile outputFile)
        {
            var engineParams = new EngineParams
            {
                InputFile = inputFile,
                OutputFile = outputFile,
                Task = FFmpegTask.Convert
            };

            FFmpegEngine(engineParams);
        }


        private void FFmpegEngine(EngineParams engineParams)
        {
            if (!File.Exists(engineParams.InputFile.Filename)) 
                throw new FileNotFoundException("Input file not found", engineParams.InputFile.Filename);

            lock (Lock)
            {
                Init();

                string conversionArgs = string.Empty;

                switch (engineParams.Task)
                {
                    case FFmpegTask.Convert:
                        conversionArgs = CommandBuilder.Convert(engineParams.InputFile,
                            engineParams.OutputFile,
                            engineParams.ConversionOptions);
                        break;

                    case FFmpegTask.GetMetaData:
                        conversionArgs = CommandBuilder.GetMetaData(engineParams.InputFile);
                        break;
                }

                var totalMediaDuration = new TimeSpan();

                ProcessStartInfo processStartInfo = GenerateProcessStartInfo(conversionArgs);

                var receivedMessages = new List<string>();

                using (FFmpegProcess = Process.Start(processStartInfo))
                {
                    if (FFmpegProcess == null) throw new InvalidOperationException("FFmpeg process is not running.");

                    FFmpegProcess.ErrorDataReceived += delegate(object o, DataReceivedEventArgs e)
                    {
                        if (e.Data == null) return;

#if (DebugToConsole)
                        Console.WriteLine(e.Data);
#endif

                        // Logging received messages; if error occurs last message  explains why.
                        receivedMessages.Insert(0, e.Data);

                        Dictionary<Find, Regex> regexIndex = RegexLibrary.Index;
                        Match matchDuration = regexIndex[Find.Duration].Match(e.Data);
                        Match matchFrame = regexIndex[Find.ConvertProgressFrame].Match(e.Data);
                        Match matchFps = regexIndex[Find.ConvertProgressFps].Match(e.Data);
                        Match matchSize = regexIndex[Find.ConvertProgressSize].Match(e.Data);
                        Match matchTime = regexIndex[Find.ConvertProgressTime].Match(e.Data);
                        Match matchBitrate = regexIndex[Find.ConvertProgressBitrate].Match(e.Data);
                        
                        Match matchMetaVideo = regexIndex[Find.MetaVideo].Match(e.Data);
                        if (matchMetaVideo.Success)
                        {
                            string fullMetadata = matchMetaVideo.Groups[1].ToString();

                            GroupCollection matchVideoFormatColorSize = regexIndex[Find.VideoFormatColorSize].Match(fullMetadata).Groups;
                            GroupCollection matchVideoFps             = regexIndex[Find.VideoFps].Match(fullMetadata).Groups;
                            GroupCollection matchVideoBitRate         = regexIndex[Find.BitRate].Match(fullMetadata).Groups;

                            if (engineParams.InputFile.Metadata == null)
                                engineParams.InputFile.Metadata = new Metadata();

                            if (engineParams.InputFile.Metadata.VideoData == null)
                                engineParams.InputFile.Metadata.VideoData = new Metadata.Video
                                {
                                    Format = matchVideoFormatColorSize[1].ToString(),
                                    ColorModel = matchVideoFormatColorSize[2].ToString(),
                                    FrameSize = matchVideoFormatColorSize[3].ToString(),
                                    Fps = System.Convert.ToInt32(matchVideoFps[1].ToString()),
                                    BitRateKbs = System.Convert.ToInt32(matchVideoBitRate[1].ToString())
                                };
                        }

                        Match matchMetaAudio = regexIndex[Find.MetaAudio].Match(e.Data);
                        if (matchMetaAudio.Success)
                        {
                            string fullMetadata = matchMetaAudio.Groups[1].ToString();
                            GroupCollection matchAudioFormatHzChannel = regexIndex[Find.AudioFormatHzChannel].Match(fullMetadata).Groups;
                            GroupCollection matchAudioBitRate = regexIndex[Find.BitRate].Match(fullMetadata).Groups;

                            if (engineParams.InputFile.Metadata == null)
                                engineParams.InputFile.Metadata = new Metadata();

                            if (engineParams.InputFile.Metadata.AudioData == null)
                                engineParams.InputFile.Metadata.AudioData = new Metadata.Audio
                                {
                                    Format = matchAudioFormatHzChannel[1].ToString(),
                                    SampleRate = matchAudioFormatHzChannel[2].ToString(),
                                    ChannelOutput = matchAudioFormatHzChannel[3].ToString(),
                                    BitRateKbs = System.Convert.ToInt32(matchAudioBitRate[1].ToString())
                                };
                        }

                        // Log the length of the loaded media
                        if (matchDuration.Success)
                        {
                            if (engineParams.InputFile.Metadata == null)
                                engineParams.InputFile.Metadata = new Metadata();
                            
                            TimeSpan.TryParse(matchDuration.Groups[1].Value, out totalMediaDuration);
                            engineParams.InputFile.Metadata.Duration = totalMediaDuration;
                        }

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
                            new ConvertProgressEventArgs(processedDuration, totalDuration, frame, fps, sizeKb,
                                bitrate));
                    };

                    FFmpegProcess.BeginErrorReadLine();
                    FFmpegProcess.WaitForExit();

                    if (FFmpegProcess.ExitCode != 0 && FFmpegProcess.ExitCode != 1)
                        throw new Exception(receivedMessages[1] + receivedMessages[0]);
                }
            }
        }

        private ProcessStartInfo GenerateProcessStartInfo(string arguments)
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

        internal class EngineParams
        {
            internal MediaFile InputFile { get; set; }
            internal MediaFile OutputFile { get; set; }
            public ConversionOptions ConversionOptions { get; set; }
            internal FFmpegTask Task { get; set; }
        }

        internal enum FFmpegTask
        {
            Convert,
            GetMetaData
        }
    }
}