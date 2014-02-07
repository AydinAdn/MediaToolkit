using System;

namespace MediaToolkit.Options
{
    public class ConversionOptions
    {
        /// <summary>
        /// <para> --- </para>
        /// <para> Cut audio / video from existing media</para>
        /// <para> Example: To extract a 15 minute piece out of a 30 minute video called TEST.mp4</para> 
        /// <para> starting from the 5th minute:</para>
        /// <para> The start position would be: TimeSpan.FromMinutes(5)</para>
        /// <para> The length would be: TimeSpan.FromMinutes(15)</para>
        /// </summary>
        /// <param name="startPosition">Specify the TimeSpan to start cutting from </param>
        /// <param name="length">Specify the length of the video to cut</param>
        public void CutMedia(TimeSpan startPosition, TimeSpan length)
        {
            Seek = startPosition;
            MaxVideoDuration = length;
        }

        /// <summary>
        ///     Audio sample rate
        /// </summary>
        public AudioSampleRate AudioSampleRate = AudioSampleRate.Default;

        /// <summary>
        ///     The maximum duration
        /// </summary>
        public TimeSpan? MaxVideoDuration = null;

        /// <summary>
        ///     The frame to begin seeking from.
        /// </summary>
        public TimeSpan? Seek = null;

        /// <summary>
        ///     Predefined audio and video options for various file formats,
        ///     <para>Can be used in conjunction with <see cref="TargetStandard" /> option</para>
        /// </summary>
        public Target Target = Target.Default;

        /// <summary>
        ///     Predefined standards to be used with the <see cref="Target" /> option
        /// </summary>
        public TargetStandard TargetStandard = TargetStandard.Default;

        /// <summary>
        ///     Video aspect ratios
        /// </summary>
        public VideoAspectRatio VideoAspectRatio = VideoAspectRatio.Default;

        /// <summary>
        ///     Video bit rate in kbit/s
        /// </summary>
        public int? VideoBitRate = null;

        /// <summary>
        ///     Video frame rate
        /// </summary>
        public int? VideoFps = null;

        /// <summary>
        ///     Video sizes
        /// </summary>
        public VideoSize VideoSize = VideoSize.Default;
    }

}