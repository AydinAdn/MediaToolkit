using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using MediaToolkit.Properties;
using MediaToolkit.Util;

namespace MediaToolkit
{
    public class EngineBase : IDisposable
    {
        private bool _isDisposed;

        /// <summary>   Used for locking the FFmpeg process to one thread. </summary>
        private const string LockName = "MediaToolkit.Engine.LockName";

        private const string DefaultFFmpegFilePath = @"/MediaToolkit/ffmpeg.exe";

        /// <summary>   Full pathname of the FFmpeg file. </summary>
        protected readonly string FFmpegFilePath;

        /// <summary>   The Mutex. </summary>
        protected readonly Mutex Mutex;

        /// <summary>   The ffmpeg process. </summary>
        protected Process FFmpegProcess;


        protected EngineBase()
            : this(ConfigurationManager.AppSettings["mediaToolkit.ffmpeg.path"])
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     <para> Initializes FFmpeg.exe; Ensuring that there is a copy</para>
        ///     <para> in the clients temp folder &amp; isn't in use by another process.</para>
        /// </summary>
        protected EngineBase(string ffMpegPath)
        {
            Mutex = new Mutex(false, LockName);
            _isDisposed = false;

            if (ffMpegPath.IsNullOrWhiteSpace())
                ffMpegPath = DefaultFFmpegFilePath;

            FFmpegFilePath = ffMpegPath;

            EnsureDirectoryExists();
            EnsureFFmpegFileExists();
            EnsureFFmpegIsNotUsed();
        }

        private void EnsureFFmpegIsNotUsed()
        {
            try
            {
                Mutex.WaitOne();
                Process.GetProcessesByName(Resources.FFmpegProcessName)
                    .ForEach(process =>
                    {
                        process.Kill();
                        process.WaitForExit();
                    });
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        private void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(FFmpegFilePath) ?? Directory.GetCurrentDirectory();

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        private void EnsureFFmpegFileExists()
        {
            if (!File.Exists(FFmpegFilePath))
                UnpackFFmpegExecutable(FFmpegFilePath);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>   Unpack ffmpeg executable. </summary>
        /// <exception cref="Exception">    Thrown when an exception error condition occurs. </exception>
        private static void UnpackFFmpegExecutable(string path)
        {
            var compressedFFmpegStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(Resources.FFmpegManifestResourceName);

            if (compressedFFmpegStream == null)
                throw new Exception(Resources.Exceptions_Null_FFmpeg_Gzip_Stream);

            using (var fileStream = new FileStream(path, FileMode.Create))
            using (var compressedStream = new GZipStream(compressedFFmpegStream, CompressionMode.Decompress))
            {
                compressedStream.CopyTo(fileStream);
            }
        }


        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        /// <remarks>   Aydin Aydin, 30/03/2015. </remarks>
        public virtual void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed)
                return;

            FFmpegProcess?.Dispose();
            FFmpegProcess = null;
            _isDisposed = true;
        }
    }
}
