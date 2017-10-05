namespace MediaToolkit.Model
{
    public partial class Metadata
    {
        public class Video
        {
            internal Video() { }
            public string Format { get; internal set; }
            public string ColorModel { get; internal set; }
            public string FrameSize { get; internal set; }
            public int? BitRateKbs { get; internal set; }
            public double Fps { get; internal set; }
        }
    }
}
