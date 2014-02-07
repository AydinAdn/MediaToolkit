using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MediaToolkit.Model;

namespace MediaToolkit.Util
{
    /// <summary>
    ///     Contains all Regex tasks
    /// </summary>
    internal static class RegexEngine
    {
        /// <summary>
        ///     Dictionary containing every Regex test.
        /// </summary>
        internal static Dictionary<Find, Regex> Index = new Dictionary<Find, Regex>
        {
            {Find.BitRate, new Regex(@"([0-9]*)\s*kb/s")},
            {Find.Duration, new Regex(@"Duration: ([^,]*), ")},
            {Find.ConvertProgressFrame, new Regex(@"frame=\s*([0-9]*)")},
            {Find.ConvertProgressFps, new Regex(@"fps=\s*([0-9]*\.?[0-9]*?)")},
            {Find.ConvertProgressSize, new Regex(@" size=\s*([0-9]*)kB")},
            {Find.ConvertProgressFinished, new Regex(@"Lsize=\s*([0-9]*)kB")},
            {Find.ConvertProgressTime, new Regex(@"time=\s*([^ ]*)")},
            {Find.ConvertProgressBitrate, new Regex(@"bitrate=\s*([0-9]*\.?[0-9]*?)kbits/s")},
            {Find.MetaAudio, new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Audio:.*)")},
            {Find.AudioFormatHzChannel, new Regex(@"Audio:\s*([^,]*),\s([^,]*),\s([^,]*)")},
            {Find.MetaVideo, new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Video:.*)")},
            {
                Find.VideoFormatColorSize,
                new Regex(@"Video:\s*([^,]*),\s*([^,]*,?[^,]*?),?\s*(?=[0-9]*x[0-9]*)([0-9]*x[0-9]*)")
            },
            {Find.VideoFps, new Regex(@"([0-9]*)\s*tbr")}
        };

        /// <summary>
        ///     <para> ---- </para>
        ///     <para>Establishes whether the data contains progress information.</para>
        /// </summary>
        /// <param name="data">Event data from the FFmpeg console.</param>
        /// <param name="progressEventArgs">
        ///     <para>If successful, outputs a <see cref="ConvertProgressEventArgs"/> which is </para>
        ///     <para>generated from the data. </para>
        /// </param>
        internal static bool IsProgressData(string data, out ConvertProgressEventArgs progressEventArgs)
        {
            progressEventArgs = null;

            Match matchFrame = Index[Find.ConvertProgressFrame].Match(data);
            Match matchFps = Index[Find.ConvertProgressFps].Match(data);
            Match matchSize = Index[Find.ConvertProgressSize].Match(data);
            Match matchTime = Index[Find.ConvertProgressTime].Match(data);
            Match matchBitrate = Index[Find.ConvertProgressBitrate].Match(data);

            if (!matchFrame.Success || !matchFps.Success || !matchSize.Success || !matchTime.Success ||
                !matchBitrate.Success) return false;

            TimeSpan processedDuration;
            TimeSpan.TryParse(matchTime.Groups[1].Value, out processedDuration);

            long frame = Convert.ToInt64(matchFrame.Groups[1].Value);
            double fps = Convert.ToDouble(matchFps.Groups[1].Value);
            int sizeKb = Convert.ToInt32(matchSize.Groups[1].Value);
            double bitrate = Convert.ToDouble(matchBitrate.Groups[1].Value);

            progressEventArgs = new ConvertProgressEventArgs(processedDuration, TimeSpan.Zero, frame, fps, sizeKb, bitrate);

            return true;
        }

        /// <summary>
        ///     <para> ---- </para>
        ///     <para>Establishes whether the data indicates the conversion is complete</para>
        /// </summary>
        /// <param name="data">Event data from the FFmpeg console.</param>
        /// <param name="conversionCompleteEvent">
        ///     <para>If successful, outputs a <see cref="ConversionCompleteEventArgs"/> which is </para>
        ///     <para>generated from the data. </para>
        /// </param>
        internal static bool IsConvertCompleteData(string data, out ConversionCompleteEventArgs conversionCompleteEvent)
        {
            conversionCompleteEvent = null;

            Match matchFrame = Index[Find.ConvertProgressFrame].Match(data);
            Match matchFps = Index[Find.ConvertProgressFps].Match(data);
            Match matchFinished = Index[Find.ConvertProgressFinished].Match(data);
            Match matchTime = Index[Find.ConvertProgressTime].Match(data);
            Match matchBitrate = Index[Find.ConvertProgressBitrate].Match(data);

            if (!matchFrame.Success || !matchFps.Success || !matchFinished.Success || !matchTime.Success ||
                !matchBitrate.Success) return false;

            TimeSpan processedDuration;
            TimeSpan.TryParse(matchTime.Groups[1].Value, out processedDuration);

            long frame = Convert.ToInt64(matchFrame.Groups[1].Value);
            double fps = Convert.ToDouble(matchFps.Groups[1].Value);
            int sizeKb = Convert.ToInt32(matchFinished.Groups[1].Value);
            double bitrate = Convert.ToDouble(matchBitrate.Groups[1].Value);

            conversionCompleteEvent = new ConversionCompleteEventArgs(processedDuration, TimeSpan.Zero, frame, fps, sizeKb, bitrate);

            return true;
        }

        internal static void TestVideo(string data, Engine.EngineParameters engine)
        {
            Match matchMetaVideo = Index[Find.MetaVideo].Match(data);

            if (!matchMetaVideo.Success) return;

            string fullMetadata = matchMetaVideo.Groups[1].ToString();

            GroupCollection matchVideoFormatColorSize = Index[Find.VideoFormatColorSize].Match(fullMetadata).Groups;
            GroupCollection matchVideoFps = Index[Find.VideoFps].Match(fullMetadata).Groups;
            Match matchVideoBitRate = Index[Find.BitRate].Match(fullMetadata);

            if (engine.InputFile.Metadata == null)
                engine.InputFile.Metadata = new Metadata();

            if (engine.InputFile.Metadata.VideoData == null)
                engine.InputFile.Metadata.VideoData = new Metadata.Video
                {
                    Format = matchVideoFormatColorSize[1].ToString(),
                    ColorModel = matchVideoFormatColorSize[2].ToString(),
                    FrameSize = matchVideoFormatColorSize[3].ToString(),
                    Fps = Convert.ToInt32(matchVideoFps[1].ToString()),
                    BitRateKbs =
                        matchVideoBitRate.Success
                            ? (int?) Convert.ToInt32(matchVideoBitRate.Groups[1].ToString())
                            : null
                };
        }

        internal static void TestAudio(string data, Engine.EngineParameters engine)
        {
            Match matchMetaAudio = Index[Find.MetaAudio].Match(data);

            if (!matchMetaAudio.Success) return;

            string fullMetadata = matchMetaAudio.Groups[1].ToString();

            GroupCollection matchAudioFormatHzChannel = Index[Find.AudioFormatHzChannel].Match(fullMetadata).Groups;
            GroupCollection matchAudioBitRate = Index[Find.BitRate].Match(fullMetadata).Groups;

            if (engine.InputFile.Metadata == null)
                engine.InputFile.Metadata = new Metadata();

            if (engine.InputFile.Metadata.AudioData == null)
                engine.InputFile.Metadata.AudioData = new Metadata.Audio
                {
                    Format = matchAudioFormatHzChannel[1].ToString(),
                    SampleRate = matchAudioFormatHzChannel[2].ToString(),
                    ChannelOutput = matchAudioFormatHzChannel[3].ToString(),
                    BitRateKbs = Convert.ToInt32(matchAudioBitRate[1].ToString())
                };
        }

        internal enum Find
        {
            AudioFormatHzChannel,
            ConvertProgressBitrate,
            ConvertProgressFps,
            ConvertProgressFrame,
            ConvertProgressSize,
            ConvertProgressFinished,
            ConvertProgressTime,
            Duration,
            MetaAudio,
            MetaVideo,
            BitRate,
            VideoFormatColorSize,
            VideoFps
        }
    }
}
