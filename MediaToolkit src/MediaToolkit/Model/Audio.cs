namespace MediaToolkit.Model
{
    public partial class Metadata
    {
        public class Audio 
        {
            internal Audio() { }

            public string Format { get; internal set; }
            public string SampleRate { get; internal set; }
            public string ChannelOutput { get; internal set; }
            public int BitRateKbs { get; internal set; }
        }
    }
}
