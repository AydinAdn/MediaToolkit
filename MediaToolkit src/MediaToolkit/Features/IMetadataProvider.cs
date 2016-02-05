using System;

namespace MediaToolkit.Features
{
    public interface IMetadataProvider : IDisposable
    {
        IMetadata GetMetadata(string filename);
    }
}