namespace MediaToolkit.Model
{
    public class MediaFile
    {
        public MediaFile(){}

        public MediaFile(string filename)
        {
            Filename = filename;
        }
        public string Filename { get; set; }

        /* TODO: Implement method which retrieves metadata.
        public bool? HasKeyFrames { get; private set; }
        public bool? HasVideo { get; private set; }
        public bool? HasAudio { get; private set; }
        public bool? HasMetadata { get; private set; }
        public bool? CanSeekToEnd { get; private set; }
        public long? DataSize { get; private set; }
        public long? VideoSize { get; private set; }
        public long? AudioSize { get; private set; }
        public int? LastTimeStamp { get; private set; }
        public int? LastKeyFrameTimeStamp { get; private set; }
        public long? LastKeyFrameLocation { get; private set; }
        public TimeSpan Duration { get; private set; }
        public float Start { get; private set; }
        public int BitRateKbs { get; private set; }
        public string VideoFormat { get; private set; }
        public string VideoFrameSize { get; private set; }
        public int Fps { get; private set; }
        public string ColorModel { get; private set; }
        public string AudioFormat { get; private set; }
         * */

    }

}
