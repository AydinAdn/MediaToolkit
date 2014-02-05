using System;

namespace MediaToolkit.Options
{
    public class ConversionOptions
    {
        /// <summary>
        ///     Audio sample rate
        /// </summary>
        public AudioSampleRate AudioSampleRate = AudioSampleRate.Default;

        /// <summary>
        ///     The maximum duration to limit media to
        /// </summary>
        public TimeSpan? MaxVideoDuration = null;

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
        ///     Video sizes
        /// </summary>
        public VideoSize VideoSize = VideoSize.Default;
    }
}