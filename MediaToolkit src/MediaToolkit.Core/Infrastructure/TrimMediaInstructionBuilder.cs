using System;
using System.Globalization;
using System.Text;

namespace MediaToolkit.Core.Infrastructure
{
    public class TrimMediaInstructionBuilder : IInstructionBuilder
    {
        public string   InputFilePath  { get; set; }
        public string   OutputFilePath { get; set; }
        public TimeSpan Duration       { get; set; }
        public TimeSpan? SeekFrom       { get; set; }

        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ",    this.SeekFrom.GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalSeconds);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -t {0} ",     this.Duration);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ",    this.OutputFilePath);
            return builder.ToString();
        }
    }
}