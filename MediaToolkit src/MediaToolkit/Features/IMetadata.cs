using System.Collections.Generic;

namespace MediaToolkit.Features
{
    public interface IMetadata
    {
        Dictionary<string, string> MetadataIndex { get; set; } 
    }
}