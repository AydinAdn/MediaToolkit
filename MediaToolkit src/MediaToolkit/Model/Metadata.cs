using System;

namespace MediaToolkit.Model
{
    public partial class Metadata
    {
        internal Metadata() { }
        public TimeSpan Duration { get; internal set; }
        public Video VideoData { get; internal set ; }
        public Audio AudioData { get; internal set; }
    }
}
