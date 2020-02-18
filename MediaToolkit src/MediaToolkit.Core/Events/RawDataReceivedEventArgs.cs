using System;

namespace MediaToolkit.Core.Events
{
    public class RawDataReceivedEventArgs : EventArgs
    {
        public RawDataReceivedEventArgs(string data)
        {
            this.Data = data;
        }

        public string Data { get; }
    }
}