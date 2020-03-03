namespace MediaToolkit
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using System.Threading;

    using MediaToolkit.Properties;
    using MediaToolkit.Util;

    public class EngineBase : IDisposable
    {
        private bool isDisposed;

        /// <summary>   Used for locking the FFmpeg process to one thread. </summary>
        private const string LockName = "MediaToolkit.Engine.LockName";

        /// <summary>
        /// Path to the ffmpeg executable
        /// </summary>
        private string DefaultFFmpegFilePath => Path.Combine(Path.GetTempPath(), "MediaToolkit/" + Guid.NewGuid().ToString() + "/ffmpeg.exe");

        private bool DeleteExeOnExit;

        /// <summary>   Full pathname of the FFmpeg file. </summary>
        protected readonly string FFmpegFilePath;

        /// <summary>   The Mutex. </summary>
        /// <remarks>Null if concurrently running FFmpeg instances are allowed.</remarks>
        protected readonly Mutex Mutex;

        private object _fileExistLock = new object();


        protected EngineBase()
           : this(ConfigurationManager.AppSettings["mediaToolkit.ffmpeg.path"])
        {
        }

        protected EngineBase(bool enableMultipleRunningProcesses)
            : this(ConfigurationManager.AppSettings["mediaToolkit.ffmpeg.path"], enableMultipleRunningProcesses)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     <para> Initializes FFmpeg.exe; Ensuring that there is a copy</para>
        ///     <para> in the clients temp folder &amp; isn't in use by another process.</para>
        /// </summary>
        protected EngineBase(string ffMpegPath) : this(ffMpegPath, false)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     <para> Initializes FFmpeg.exe; Ensuring that there is a copy</para>
        ///     <para> in the clients temp folder &amp; isn't in use by another process.</para>
        /// </summary>
        /// <param name="enableMultipleRunningProcesses">Whether or not to allow multiple instances of FFmpeg to run concurrently.</param>
        protected EngineBase(string ffMpegPath, bool enableMultipleRunningProcesses)
        {
            if (!enableMultipleRunningProcesses)
            {
                this.Mutex = new Mutex(false, LockName);
            }

            this.isDisposed = false;

            if (ffMpegPath.IsNullOrWhiteSpace())
            {
                ffMpegPath = DefaultFFmpegFilePath;
                DeleteExeOnExit = true;
            }

            this.FFmpegFilePath = ffMpegPath;

            this.EnsureDirectoryExists();
            this.EnsureFFmpegFileExists();

            if (!enableMultipleRunningProcesses)
            {
                this.EnsureFFmpegIsNotUsed();
            }
        }

        private void EnsureFFmpegIsNotUsed()
        {
            try
            {
                this.Mutex.WaitOne();
                Process.GetProcessesByName(Resources.FFmpegProcessName)
                       .ForEach(process =>
                       {
                           process.Kill();
                           process.WaitForExit();
                       });
            }
            finally
            {
                this.Mutex.ReleaseMutex();
            }
        }

        private void EnsureDirectoryExists()
        {
            string directory = Path.GetDirectoryName(this.FFmpegFilePath) ?? Directory.GetCurrentDirectory(); ;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void EnsureFFmpegFileExists()
        {
            if (!File.Exists(this.FFmpegFilePath))
            {
                lock (_fileExistLock)
                {
                    if (!File.Exists(this.FFmpegFilePath)) // Check again in case another thread got this far and created the file
                    {
                        UnpackFFmpegExecutable(this.FFmpegFilePath);
                    }
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

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            using (GZipStream compressedStream = new GZipStream(compressedFFmpegStream, CompressionMode.Decompress))
            {
                compressedStream.CopyTo(fileStream);
            }
        }



        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        /// <remarks>   Aydin Aydin, 30/03/2015. </remarks>
        public virtual void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || this.isDisposed)
            {
                return;
            }

            // Clean up temporary file
            if (DeleteExeOnExit)
            {
                File.Delete(FFmpegFilePath);
            }

            this.isDisposed = true;
        }
    }
}
