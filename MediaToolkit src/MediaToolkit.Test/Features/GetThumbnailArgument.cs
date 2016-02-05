using System;
using System.Text;
using MediaToolkit.Util;

namespace MediaToolkit.Test.Features
{
    public class GetThumbnailArgument : IArgument
    {
        public GetThumbnailArgument(string inputFilePath, string outputFilePath, TimeSpan seekPosition)
        {
            this.Argument = new StringBuilder();
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
            SeekPosition = seekPosition;
        }

        public string InputFilePath { get; private set; }
        public string OutputFilePath { get; private set; }
        public TimeSpan SeekPosition { get; private set; }


        public void ComposeArgument()
        {
            this.Argument.Append("-ss {0} ".FormatInvariant(this.SeekPosition.TotalSeconds));
            this.Argument.Append("-i \"{0}\" ".FormatInvariant(this.InputFilePath));
            this.Argument.Append("-vframes {0} ".FormatInvariant(1));
            this.Argument.Append("\"{0}\"".FormatInvariant(this.OutputFilePath));
        }

        public StringBuilder Argument { get; private set; }
    }
}