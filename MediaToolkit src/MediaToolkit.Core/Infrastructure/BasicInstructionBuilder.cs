using System.Globalization;
using System.Text;

namespace MediaToolkit.Core.Infrastructure
{
    public class BasicInstructionBuilder : IInstructionBuilder
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }

        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ", this.OutputFilePath);
            return builder.ToString();
        }
    }
}