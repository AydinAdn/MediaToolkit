using System;
using System.Collections.Generic;

namespace MediaToolkit.Core.Events
{
    public class ProgressUpdateEventArgs : EventArgs
    {
        public ProgressUpdateEventArgs(Dictionary<string, string> updateData)
        {
            this.UpdateData = updateData;
        }

        public Dictionary<string, string> UpdateData { get; }
    }
}
