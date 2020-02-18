using Newtonsoft.Json;

namespace MediaToolkit.Core.Meta
{
    public class MediaStream
    {

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("codec_name")]
        public string CodecName { get; set; }

        [JsonProperty("codec_long_name")]
        public string CodecLongName { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; }

        [JsonProperty("codec_type")]
        public string CodecType { get; set; }

        [JsonProperty("codec_time_base")]
        public string CodecTimeBase { get; set; }

        [JsonProperty("codec_tag_string")]
        public string CodecTagString { get; set; }

        [JsonProperty("codec_tag")]
        public string CodecTag { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("coded_width")]
        public int CodedWidth { get; set; }

        [JsonProperty("coded_height")]
        public int CodedHeight { get; set; }

        [JsonProperty("has_b_frames")]
        public int HasBFrames { get; set; }

        [JsonProperty("pix_fmt")]
        public string PixFmt { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("chroma_location")]
        public string ChromaLocation { get; set; }

        [JsonProperty("refs")]
        public int Refs { get; set; }

        [JsonProperty("is_avc")]
        public string IsAvc { get; set; }

        [JsonProperty("nal_length_size")]
        public string NalLengthSize { get; set; }

        [JsonProperty("r_frame_rate")]
        public string RFrameRate { get; set; }

        [JsonProperty("avg_frame_rate")]
        public string AvgFrameRate { get; set; }

        [JsonProperty("time_base")]
        public string TimeBase { get; set; }

        [JsonProperty("start_pts")]
        public int StartPts { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("duration_ts")]
        public int DurationTs { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("bit_rate")]
        public string BitRate { get; set; }

        [JsonProperty("bits_per_raw_sample")]
        public string BitsPerRawSample { get; set; }

        [JsonProperty("nb_frames")]
        public string NbFrames { get; set; }

        [JsonProperty("disposition")]
        public Disposition Disposition { get; set; }

        [JsonProperty("tags")]
        public StreamTags Tags { get; set; }

        [JsonProperty("sample_fmt")]
        public string SampleFmt { get; set; }

        [JsonProperty("sample_rate")]
        public string SampleRate { get; set; }

        [JsonProperty("channels")]
        public int? Channels { get; set; }

        [JsonProperty("channel_layout")]
        public string ChannelLayout { get; set; }

        [JsonProperty("bits_per_sample")]
        public int? BitsPerSample { get; set; }

        [JsonProperty("max_bit_rate")]
        public string MaxBitRate { get; set; }

        [JsonProperty("color_range")]
        public string ColorRange { get; set; }

        [JsonProperty("color_space")]
        public string ColorSpace { get; set; }

        [JsonProperty("color_transfer")]
        public string ColorTransfer { get; set; }

        [JsonProperty("color_primaries")]
        public string ColorPrimaries { get; set; }
    }
}