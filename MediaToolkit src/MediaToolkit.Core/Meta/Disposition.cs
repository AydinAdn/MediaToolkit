using Newtonsoft.Json;

namespace MediaToolkit.Core.Meta
{
    public class Disposition
    {

        [JsonProperty("default")]
        public int Default { get; set; }

        [JsonProperty("dub")]
        public int Dub { get; set; }

        [JsonProperty("original")]
        public int Original { get; set; }

        [JsonProperty("comment")]
        public int Comment { get; set; }

        [JsonProperty("lyrics")]
        public int Lyrics { get; set; }

        [JsonProperty("karaoke")]
        public int Karaoke { get; set; }

        [JsonProperty("forced")]
        public int Forced { get; set; }

        [JsonProperty("hearing_impaired")]
        public int HearingImpaired { get; set; }

        [JsonProperty("visual_impaired")]
        public int VisualImpaired { get; set; }

        [JsonProperty("clean_effects")]
        public int CleanEffects { get; set; }

        [JsonProperty("attached_pic")]
        public int AttachedPic { get; set; }

        [JsonProperty("timed_thumbnails")]
        public int TimedThumbnails { get; set; }
    }
}