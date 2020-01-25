using System;
using System.Globalization;
using System.Text;

namespace MediaToolkit.Core.Infrastructure
{
    public class ExtractThumbnailInstructionBuilder : IInstructionBuilder
    {
        public TimeSpan? SeekFrom { get; set; }
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
        public int Frames { get; set; } = 1;

        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ",    this.SeekFrom.GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalSeconds);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -vframes {0} ", this.Frames);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ",    this.OutputFilePath);
            return builder.ToString();
        }
    }
}