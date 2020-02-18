using System;

namespace MediaToolkit.Core.Events
{
    public class WarningEventArgs : EventArgs
    {
        public WarningEventArgs(string warning)
        {
            this.Warning = warning;
        }

        public string Warning { get; }
    }
}