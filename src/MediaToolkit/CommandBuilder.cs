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

            // If convert settings is null, build basic command.
            if (settings == null)
            {
                commandBuilder.AppendFormat("-i {0} {1}", iFile.Filename, oFile.Filename);

                return commandBuilder.ToString();
            }


            commandBuilder.AppendFormat("-i {0} ", iFile.Filename);

            /* ***************
             * Target settings
             * ***************/
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


            /* ***************
             * Audio settings
             * ***************/

            // Set audio sample rate
            if (settings.AudioSampleRate != AudioSampleRate.Default)
                commandBuilder.AppendFormat(" -ar {0} ", settings.AudioSampleRate.Remove("Hz"));


            /* ***************
             * Video settings
             * ***************/

            // Set maximum video duration
            if (settings.MaxVideoDuration != null)
                commandBuilder.AppendFormat(" -t {0} ", settings.MaxVideoDuration);

            // Set video bit rate
            if (settings.VideoBitRate != null)
                commandBuilder.AppendFormat(" -b {0}k ", settings.VideoBitRate);

            // Set video size
            if (settings.VideoSize != VideoSize.Default)
            {
                string size = settings.VideoSize.ToLower();
                if (size.StartsWith("_")) size = size.Replace("_", "");
                if (size.Contains("_")) size = size.Replace("_", "-");

                commandBuilder.AppendFormat(" -s {0} ", size);
            }

            // Set video aspect ratio
            if (settings.VideoAspectRatio != VideoAspectRatio.Default)
            {
                string ratio = settings.VideoAspectRatio.ToLower();
                ratio = ratio.Substring(1);
                ratio = ratio.Replace("_", ",");

                commandBuilder.AppendFormat(" -aspect {0} ", ratio);
            }

            commandBuilder.AppendFormat(" {0}", oFile.Filename);
            return commandBuilder.ToString();
        }
    }
}