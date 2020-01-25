using System.Globalization;
using System.Text;

namespace MediaToolkit.Core.Infrastructure
{
    public class CropVideoInstructionBuilder : IInstructionBuilder
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }


        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -filter:v \"crop={0}:{1}:{2}:{3}\" ", this.Width, this.Height, this.X, this.Y);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ",    this.OutputFilePath);
            return builder.ToString();
        }
    }
}