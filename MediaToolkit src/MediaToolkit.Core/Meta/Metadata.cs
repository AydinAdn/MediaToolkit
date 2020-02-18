using System.Collections.Generic;
using Newtonsoft.Json;

namespace MediaToolkit.Core.Meta
{
    public class Metadata
    {
        [JsonProperty("streams")]
        public IList<MediaStream> Streams { get; set; }

        [JsonProperty("format")]
        public Format Format { get; set; }

        public string RawMetaData { get; set; }
    }
}
