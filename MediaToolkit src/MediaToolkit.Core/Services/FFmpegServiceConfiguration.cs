using System.IO;

namespace MediaToolkit.Core.Services
{
    public class FFmpegServiceConfiguration : IProcessServiceConfiguration
    {
        public FFmpegServiceConfiguration(string exePath = null, 
            string globalArguments = null, 
            string embeddedResourceId = null)
        {
            this.ExePath            = exePath            ?? Directory.GetCurrentDirectory() + @"/MediaToolkit/ffmpeg.exe";
            this.GlobalArguments    = globalArguments    ?? @"-nostdin -progress pipe:2 -y -loglevel warning ";
            this.EmbeddedResourceId = embeddedResourceId ?? "MediaToolkit.Core.Resources.FFmpeg.exe.gz";
        }

        public string ExePath            { get; set; }
        public string GlobalArguments    { get; set; }
        public string EmbeddedResourceId { get; set; }
    }
}