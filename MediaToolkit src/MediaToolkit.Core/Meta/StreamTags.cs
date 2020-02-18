using Newtonsoft.Json;

namespace MediaToolkit.Core.Meta
{
    public class StreamTags
    {

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("handler_name")]
        public string HandlerName { get; set; }

        [JsonProperty("creation_time")]
        public string CreationTime { get; set; }
    }
}