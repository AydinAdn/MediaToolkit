namespace MediaToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    using MediaToolkit.Model;
    using MediaToolkit.Options;
    using MediaToolkit.Properties;
    using MediaToolkit.Util;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>   An engine. This class cannot be inherited. </summary>
    public class Engine : EngineBase
    {
        /// <summary>
        ///     Event queue for all listeners interested in conversionComplete events.
        /// </summary>
        public event EventHandler<ConversionCompleteEventArgs> ConversionCompleteEvent;

        public Engine()
        {
            
        }

        public Engine(string ffMpegPath) : base(ffMpegPath)
        {
            
        }

        /// -------------------------------------------------------------------------------------------------
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

        /// -------------------------------------------------------------------------------------------------
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

        public void CustomCommand(string ffmpegCommand)
        {
            if (ffmpegCommand.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("ffmpegCommand");
            }

            EngineParameters engineParameters = new EngineParameters { CustomArguments = ffmpegCommand };

            this.StartFFmpegProcess(engineParameters);
        }

        /// -------------------------------------------------------------------------------------------------
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

        /// -------------------------------------------------------------------------------------------------
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
        
        #region Private method - Helpers

        private void FFmpegEngine(EngineParameters engineParameters)
        {
            if (!engineParameters.InputFile.Filename.StartsWith("http://") && !File.Exists(engineParameters.InputFile.Filename))
            {
                throw new FileNotFoundException(Resources.Exception_Media_Input_File_Not_Found, engineParameters.InputFile.Filename);
            }

            try
            {
                this.Mutex.WaitOne();
                this.StartFFmpegProcess(engineParameters);
            }
            finally
            {
                this.Mutex.ReleaseMutex();
            }
        }

        private ProcessStartInfo GenerateStartInfo(EngineParameters engineParameters)
        {
            string arguments = CommandBuilder.Serialize(engineParameters);

            return this.GenerateStartInfo(arguments);
        }

        private ProcessStartInfo GenerateStartInfo(string arguments)
        {
            //windows case
            if (Path.DirectorySeparatorChar == '\\')
            {
                return new ProcessStartInfo
                {
                    Arguments = "-nostdin -y -loglevel info " + arguments,
                    FileName = this.FFmpegFilePath,
                    CreateNoWindow = true,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            }
            else //linux case: -nostdin options doesn't exist at least in debian ffmpeg
            {
                return new ProcessStartInfo
                {
                    Arguments = "-y -loglevel info " + arguments,
                    FileName = this.FFmpegFilePath,
                    CreateNoWindow = true,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            }
        }
        
        #endregion

        /// -------------------------------------------------------------------------------------------------
        /// <summary>   Raises the conversion complete event. </summary>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        private void OnConversionComplete(ConversionCompleteEventArgs e)
        {
            EventHandler<ConversionCompleteEventArgs> handler = this.ConversionCompleteEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>   Raises the convert progress event. </summary>
        /// <param name="e">    Event information to send to registered event handlers. </param>
        private void OnProgressChanged(ConvertProgressEventArgs e)
        {
            EventHandler<ConvertProgressEventArgs> handler = this.ConvertProgressEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>   Starts FFmpeg process. </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is
        ///     invalid.
        /// </exception>
        /// <exception cref="Exception">
        ///     Thrown when an exception error condition
        ///     occurs.
        /// </exception>
        /// <param name="engineParameters"> The engine parameters. </param>
        private void StartFFmpegProcess(EngineParameters engineParameters)
        {
            List<string> receivedMessagesLog = new List<string>();
            TimeSpan totalMediaDuration = new TimeSpan();
         
            ProcessStartInfo processStartInfo = engineParameters.HasCustomArguments 
                                              ? this.GenerateStartInfo(engineParameters.CustomArguments)
                                              : this.GenerateStartInfo(engineParameters);

            using (this.FFmpegProcess = Process.Start(processStartInfo))
            {
                Exception caughtException = null;
                if (this.FFmpegProcess == null)
                {
                    throw new InvalidOperationException(Resources.Exceptions_FFmpeg_Process_Not_Running);
                }

                this.FFmpegProcess.ErrorDataReceived += (sender, received) =>
                {
                    if (received.Data == null) return;
#if (DebugToConsole)
                    Console.WriteLine(received.Data);
#endif
                    try
                    {
                        
                        receivedMessagesLog.Insert(0, received.Data);
                        if (engineParameters.InputFile != null)
                        {
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
                            this.FFmpegProcess.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                            // swallow exceptions that are thrown when killing the process, 
                            // one possible candidate is the application ending naturally before we get a chance to kill it
                        }
                    }
                };

                this.FFmpegProcess.BeginErrorReadLine();
                this.FFmpegProcess.WaitForExit();

                if ((this.FFmpegProcess.ExitCode != 0 && this.FFmpegProcess.ExitCode != 1) || caughtException != null)
                {
                    throw new Exception(
                        this.FFmpegProcess.ExitCode + ": " + receivedMessagesLog[1] + receivedMessagesLog[0],
                        caughtException);
                }
            }
        }
    }
}