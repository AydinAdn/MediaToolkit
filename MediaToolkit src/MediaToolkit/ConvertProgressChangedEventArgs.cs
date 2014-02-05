using System;

namespace MediaToolkit
{
    public class ConvertProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     Event model to track media conversion.
        /// </summary>
        /// <param name="processed">Duration of the media which has been processed</param>
        /// <param name="totalDuration">The total duration of the media</param>
        /// <param name="frame">The frame the conversion process is on</param>
        /// <param name="fps">The frames per second</param>
        /// <param name="sizeKb">The current size of the converted file</param>
        /// <param name="bitrate">The bit rate of the converted file</param>
        public ConvertProgressChangedEventArgs(TimeSpan processed, TimeSpan totalDuration, long frame, double fps, int sizeKb, double bitrate)
        {
            TotalDuration = totalDuration;
            ProcessedDuration = processed;
            Frame = frame;
            Fps = fps;
            SizeKb = sizeKb;
            Bitrate = bitrate;
        }

        public long Frame { get; private set; }
        public double Fps { get; private set; }
        public int SizeKb { get; private set; }
        public TimeSpan ProcessedDuration { get; private set; }
        public double Bitrate { get; private set; }
        public TimeSpan TotalDuration { get; private set; }
    }
}