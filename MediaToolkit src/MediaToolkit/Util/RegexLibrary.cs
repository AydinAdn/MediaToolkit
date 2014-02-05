using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MediaToolkit.Util
{
    internal static class RegexLibrary
    {
        internal static Dictionary<Find, Regex> Index = new Dictionary<Find, Regex>
        {
            {Find.ConvertDuration,       new Regex(@"Duration:\s*([^ ]*)")},
            {Find.ConvertProgressFrame,  new Regex(@"frame=\s*([0-9]*)")},
            {Find.ConvertProgressFps,    new Regex(@"fps=\s*([0-9]*\.?[0-9]*?)")},
            {Find.ConvertProgressSize,   new Regex(@"size=\s*([0-9]*)kB")},
            {Find.ConvertProgressTime,   new Regex(@"time=\s*([^ ]*)")},
            {Find.ConvertProgressBitrate,new Regex(@"bitrate=\s*([0-9*\.?[0-9]*?)kbits/s")},
            {Find.HasKeyFrames,          new Regex(@"hasKeyframes\s*:\s*(\w*)")},
            {Find.HasVideo,              new Regex(@"hasVideo *: (\w*)")},
            {Find.HasAudio,              new Regex(@"hasAudio *: (\w*)")},
            {Find.HasMetadata,           new Regex(@"hasMetadata *: (\w*)")},
            {Find.CanSeekToEnd,          new Regex(@"canSeekToEnd *: (\w*)")},
            {Find.DataSize,              new Regex(@"datasize *: (\w*)")},
            {Find.VideoSize,             new Regex(@"videosize *: (\w*)")},
            {Find.AudioSize,             new Regex(@"audiosize *: (\w*)")},
            {Find.LastTimeStamp,         new Regex(@"lasttimestamp *: (\w*)")},
            {Find.LastKeyFrameTimeStamp, new Regex(@"lastkeyframetimestamp *: (\w*)")},
            {Find.LastKeyFrameLocation,  new Regex(@"lastkeyframelocation *: (\w*)")},
            {Find.Duration,              new Regex(@"Duration: ([^,]*), start: ([^,]*), bitrate: ([^ ]*)")}
        };
    }

    internal enum Find
    {
        ConvertDuration,
        ConvertProgress,
        ConvertProgressFrame,
        ConvertProgressFps,
        ConvertProgressSize,
        ConvertProgressTime,
        ConvertProgressBitrate,
        HasKeyFrames,
        HasVideo,
        HasAudio,
        HasMetadata,
        CanSeekToEnd,
        DataSize,
        VideoSize,
        AudioSize,
        LastTimeStamp,
        LastKeyFrameTimeStamp,
        LastKeyFrameLocation,
        Duration
    }
}