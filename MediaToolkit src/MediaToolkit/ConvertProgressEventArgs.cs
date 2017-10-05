using System;

namespace MediaToolkit
{
    public class ConvertProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Raises notifications on the conversion process
        /// </summary>
        /// <param name="processed">Duration of the media which has been processed</param>
        /// <param name="totalDuration">The total duration of the original media</param>
        /// <param name="frame">The specific frame the conversion process is on</param>
        /// <param name="fps">The frames converted per second</param>
        /// <param name="sizeKb">The current size in Kb of the converted media</param>
        /// <param name="bitrate">The bit rate of the converted media</param>
        public ConvertProgressEventArgs(TimeSpan processed, TimeSpan totalDuration, long? frame, double? fps, int? sizeKb,
            double? bitrate)
        {
            TotalDuration = totalDuration;
            ProcessedDuration = processed;
            Frame = frame;
            Fps = fps;
            SizeKb = sizeKb;
            Bitrate = bitrate;
        }

        public long? Frame { get; }
        public double? Fps { get; }
        public int? SizeKb { get; }
        public TimeSpan ProcessedDuration { get; }
        public double? Bitrate { get; }
        public TimeSpan TotalDuration { get; internal set; }
    }
}