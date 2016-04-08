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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     <para> Initializes FFmpeg.exe; Ensuring that there is a copy</para>
        ///     <para> in the clients temp folder &amp; isn't in use by another process.</para>
        /// </summary>
        protected EngineBase(string ffMpegPath)
        {
            this.Mutex = new Mutex(false, LockName);
            this.isDisposed = false;

            if (ffMpegPath.IsNullOrWhiteSpace())
            {
                ffMpegPath = DefaultFFmpegFilePath;
            }

            this.FFmpegFilePath = ffMpegPath;

            this.EnsureDirectoryExists ();
            this.EnsureFFmpegFileExists();
            this.EnsureFFmpegIsNotUsed ();
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
                UnpackFFmpegExecutable(this.FFmpegFilePath);
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

            if(FFmpegProcess != null)
            {
                this.FFmpegProcess.Dispose();
            }            
            this.FFmpegProcess = null;
            this.isDisposed = true;
        }
    }
}
