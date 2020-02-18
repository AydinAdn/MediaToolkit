using System.Globalization;
using System.Text;

namespace MediaToolkit.Core.Infrastructure
{
    public class GetMetadataInstructionBuilder : IInstructionBuilder
    {
        public string InputFilePath { get; set; }

        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ", this.InputFilePath);
            return builder.ToString();
        }
    }
}