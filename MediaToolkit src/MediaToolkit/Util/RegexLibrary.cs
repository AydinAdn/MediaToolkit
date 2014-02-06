using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MediaToolkit.Util
{
    internal static class RegexLibrary
    {
        internal static Dictionary<Find, Regex> Index = new Dictionary<Find, Regex>
        {
            {Find.BitRate,               new Regex(@"([0-9]*)\s*kb/s")},
            {Find.Duration,              new Regex(@"Duration: ([^,]*), ")},

            {Find.ConvertProgressFrame,  new Regex(@"frame=\s*([0-9]*)")},
            {Find.ConvertProgressFps,    new Regex(@"fps=\s*([0-9]*\.?[0-9]*?)")},
            {Find.ConvertProgressSize,   new Regex(@"size=\s*([0-9]*)kB")},
            {Find.ConvertProgressTime,   new Regex(@"time=\s*([^ ]*)")},
            {Find.ConvertProgressBitrate,new Regex(@"bitrate=\s*([0-9]*\.?[0-9]*?)kbits/s")},

            {Find.MetaAudio,             new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Audio:.*)")},
            {Find.AudioFormatHzChannel,  new Regex(@"Audio:\s*([^,]*),\s([^,]*),\s([^,]*)")},

            {Find.MetaVideo,             new Regex(@"(Stream\s*#[0-9]*:[0-9]*\(?[^\)]*?\)?: Video:.*)")},
            {Find.VideoFormatColorSize,  new Regex(@"Video:\s*([^,]*),\s*([^,]*,?[^,]*?),?\s*(?=[0-9]*x[0-9]*)([0-9]*x[0-9]*)")},
            {Find.VideoFps,              new Regex(@"([0-9]*)\s*tbr")},
        };
    }

    internal enum Find
    {
        AudioBitRate,
        AudioFormatHzChannel,
        AudioSampleRate,
        ConvertProgress,
        ConvertProgressBitrate,
        ConvertProgressFps,
        ConvertProgressFrame,
        ConvertProgressSize,
        ConvertProgressTime,
        Duration,
        MetaAudio,
        MetaVideo,
        BitRate,
        VideoFormatColorSize,
        VideoFps
    }
}