using System.Collections.Generic;

namespace MediaToolkit.Features
{
    public class Metadata : IMetadata
    {
        public Dictionary<string, string> MetadataIndex { get; set; }
    }
}