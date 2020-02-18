using Newtonsoft.Json;

namespace MediaToolkit.Core.Meta
{
    public class Format
    {

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("nb_streams")]
        public int NbStreams { get; set; }

        [JsonProperty("nb_programs")]
        public int NbPrograms { get; set; }

        [JsonProperty("format_name")]
        public string FormatName { get; set; }

        [JsonProperty("format_long_name")]
        public string FormatLongName { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("bit_rate")]
        public string BitRate { get; set; }

        [JsonProperty("probe_score")]
        public int ProbeScore { get; set; }

        [JsonProperty("tags")]
        public FormatTags Tags { get; set; }
    }
}