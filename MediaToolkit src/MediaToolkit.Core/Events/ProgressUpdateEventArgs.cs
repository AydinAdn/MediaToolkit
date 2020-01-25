using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Core.Events
{
    public class ProgressUpdateEventArgs : EventArgs
    {
        public ProgressUpdateEventArgs()
        {
        }

        public ProgressUpdateEventArgs(Dictionary<string, string> updateData)
        {
            this.UpdateData = updateData;
        }

        public Dictionary<string, string> UpdateData { get; set; }
    }

}
