using System.Text;
using MediaToolkit.Model;
using MediaToolkit.Options;
using MediaToolkit.Util;

namespace MediaToolkit
{
    internal class CommandBuilder
    {
        internal static string Convert(MediaFile iFile, MediaFile oFile, ConversionOptions settings)
        {
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendFormat("-i {0} ", iFile.Filename);

            // A basic convert.
            if (settings == null)
                return commandBuilder.Append(oFile.Filename).ToString();

            // Physical media conversion (DVD etc)
            if (settings.Target != Target.Default)
            {
                commandBuilder.Append(" -target ");
                if (settings.TargetStandard != TargetStandard.Default)
                {
                    commandBuilder.AppendFormat("{0}-{1} {2}", settings.TargetStandard.ToLower(),
                        settings.Target.ToLower(), oFile.Filename);

                    return commandBuilder.ToString();
                }
                commandBuilder.AppendFormat("{0} {1}", settings.Target.ToLower(), oFile.Filename);

                return commandBuilder.ToString();
            }

            // Audio sample rate
            if (settings.AudioSampleRate != AudioSampleRate.Default)
                commandBuilder.AppendFormat(" -ar {0} ", settings.AudioSampleRate.Remove("Hz"));

            // Maximum video duration
            if (settings.MaxVideoDuration != null)
                commandBuilder.AppendFormat(" -t {0} ", settings.MaxVideoDuration);

            // Video bit rate
            if (settings.VideoBitRate != null)
                commandBuilder.AppendFormat(" -b {0}k ", settings.VideoBitRate);

            // Video size / resolution
            if (settings.VideoSize != VideoSize.Default)
            {
                string size = settings.VideoSize.ToLower();
                if (size.StartsWith("_")) size = size.Replace("_", "");
                if (size.Contains("_")) size = size.Replace("_", "-");

                commandBuilder.AppendFormat(" -s {0} ", size);
            }

            // Video aspect ratio
            if (settings.VideoAspectRatio != VideoAspectRatio.Default)
            {
                string ratio = settings.VideoAspectRatio.ToString();
                ratio = ratio.Substring(1);
                ratio = ratio.Replace("_", ":");

                commandBuilder.AppendFormat(" -aspect {0} ", ratio);
            }

            return commandBuilder.AppendFormat(" {0}", oFile.Filename).ToString();
        }
    }
}