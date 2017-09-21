using System;

namespace MediaToolkit.Options
{
    public class ConversionOptions
    {
        /// <summary>
        ///     <para> --- </para>
        ///     <para> Cut audio / video from existing media                </para>
        ///     <para> Example: To cut a 15 minute section                  </para> 
        ///     <para> out of a 30 minute video starting                    </para>
        ///     <para> from the 5th minute:                                 </para>
        ///     <para> The start position would be: TimeSpan.FromMinutes(5) </para>
        ///     <para> The length would be:         TimeSpan.FromMinutes(15)</para>
        /// </summary>
        /// <param name="seekToPosition">
        ///     <para>Specify the position to seek to,                  </para>
        ///     <para>if you wish to begin the cut starting             </para>
        ///     <para>from the 5th minute, use: TimeSpan.FromMinutes(5);</para>
        /// </param>
        /// <param name="length">
        ///     <para>Specify the length of the video to cut,           </para>
        ///     <para>to cut out a 15 minute duration                   </para>
        ///     <para>simply use: TimeSpan.FromMinutes(15);             </para>
        /// </param>
        public void CutMedia(TimeSpan seekToPosition, TimeSpan length)
        {
            this.Seek = seekToPosition;
            this.MaxVideoDuration = length;
        }

        /// <summary>
        ///     Audio bit rate
        /// </summary>
        public int? AudioBitRate = null;

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

        /// <summary>
        ///     Custom Width when VideoSize.Custom is set
        /// </summary>
        public int? CustomWidth { get; set; }

        /// <summary>
        ///     Custom Height when VideoSize.Custom is set
        /// </summary>
        public int? CustomHeight { get; set; }

        /// <summary>
        ///     Specifies an optional rectangle from the source video to crop
        /// </summary>
        public CropRectangle SourceCrop { get; set; }

        /// <summary>
        ///     Specifies wheter or not to use H.264 Baseline Profile
        /// </summary>
        public bool BaselineProfile { get; set; }
    }

}