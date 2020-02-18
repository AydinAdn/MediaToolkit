using System;
using MediaToolkit.Core.Meta;

namespace MediaToolkit.Core.Events
{
    public class MetadataEventArgs : EventArgs
    {
        public MetadataEventArgs(Metadata metadata)
        {
            this.Metadata = metadata;
        }

        public Metadata Metadata { get; }
    }
}