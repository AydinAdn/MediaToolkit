using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NUnit.Framework;

namespace MediaToolkit.Test
{
    [TestFixture]
    public class ConvertTest
    {
        [TestFixtureSetUp]
        public void Init()
        {
            /* To specify your own files, 
             * uncomment the section below.
             * ******************************/

            /*
            string inputFile = @"C:\TEST\1.flv";
            string outputFile = @"C:\TEST\1.mp4";
            if (inputFile != "")
            {
                _inputFilePath = inputFile;
                if (outputFile != "")
                    _outputFilePath = outputFile;

                return;
            }
            */

            _raiseEvents = true;

            DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            Debug.Assert(directoryInfo.Parent != null, "directoryInfo.Parent != null");
            
            DirectoryInfo testDirectoryInfo = directoryInfo.Parent.Parent;
            Debug.Assert(testDirectoryInfo != null, "testDirectoryInfo != null");
            
            var testDirectoryPath = testDirectoryInfo.FullName + @"\TestVideo\";

            Debug.Assert(Directory.Exists(testDirectoryPath), "Directory not found: " + testDirectoryPath);
            Debug.Assert(File.Exists(testDirectoryPath + @"BigBunny.m4v"), "Test file not found: " + testDirectoryPath + @"BigBunny.m4v");
            
            _inputFilePath  = testDirectoryPath + @"BigBunny.m4v";
            _outputFilePath = testDirectoryPath + @"OuputBunny.mp4";
        }

        private string _inputFilePath  = "";
        private string _outputFilePath = "";
        private bool _raiseEvents;

        [TestCase]
        public void Can_GetMetadata()
        {
            var inputFile = new MediaFile { Filename = _inputFilePath };

            using (var engine = new Engine())
                engine.GetMetaData(inputFile);

            Metadata inputMeta = inputFile.Metadata;

            Debug.Assert(inputMeta.Duration                != TimeSpan.Zero, "Media duration is zero", "  Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.Format        != null         , "Video format not found", "  Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.ColorModel    != null         , "Color model not found", "   Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.FrameSize     != null         , "Frame size not found", "    Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.Fps           != 0            , "Fps not found", "           Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.BitRateKbs    != 0            , "Video bitrate not found", " Likely due to Regex code");
            Debug.Assert(inputMeta.AudioData.Format        != null         , "Audio format not found", "  Likely due to Regex code");
            Debug.Assert(inputMeta.AudioData.SampleRate    != null         , "Sample rate not found", "   Likely due to Regex code");
            Debug.Assert(inputMeta.AudioData.ChannelOutput != null         , "Channel output not found", "Likely due to Regex code");
            Debug.Assert(inputMeta.AudioData.BitRateKbs    != 0            , "Audio bitrate not found", " Likely due to Regex code");

#if(DebugToConsole) 
            PrintMetadata(inputMeta);
#endif
        }

        [TestCase]
        public void Can_ConvertBasic()
        {
            var inputFile = new MediaFile {Filename = _inputFilePath};
            var outputFile = new MediaFile {Filename = _outputFilePath};

            if(_raiseEvents) Engine.ConvertProgressEvent += EngineConvertProgressEvent;
            
            using (var engine = new Engine())
            {
                engine.GetMetaData(inputFile);
                engine.Convert(inputFile, outputFile);
                engine.GetMetaData(outputFile);
            }

            Metadata inputMeta = inputFile.Metadata;
            Metadata outputMeta = outputFile.Metadata;

            Debug.Assert((int)inputMeta.Duration.TotalSeconds > (int)outputMeta.Duration.TotalSeconds - 1, 
                "Output media converted incorrectly", 
                "Output video is too short");
            Debug.Assert((int)inputMeta.Duration.TotalSeconds < (int)outputMeta.Duration.TotalSeconds + 1, 
                "Output media converted incorrectly", 
                "Output video is too long");
            Debug.Assert(inputMeta.VideoData.Format != outputMeta.VideoData.Format, 
                "Input & output video formats are the same");

#if(DebugToConsole) 
            PrintMetadata(inputMeta);
#endif
        }

        //TODO: Further test cases using metadata
        [TestCase]
        public void Can_ConvertToDVD()
        {
            string outputPath = string.Format("{0}/{1}.DVD.mpg",
                                               Path.GetDirectoryName(_outputFilePath),
                                               Path.GetFileNameWithoutExtension(_outputFilePath));

            var inputFile = new MediaFile {Filename = _inputFilePath};
            var outputFile = new MediaFile {Filename = outputPath};

            var conversionOptions = new ConversionOptions { Target = Target.DVD, TargetStandard = TargetStandard.PAL };
            if (_raiseEvents) Engine.ConvertProgressEvent += EngineConvertProgressEvent;

            using (var engine = new Engine())
            {
                engine.Convert(inputFile, outputFile, conversionOptions);
                engine.GetMetaData(inputFile);
                engine.GetMetaData(outputFile);
            }

            PrintMetadata(inputFile.Metadata);
            PrintMetadata(outputFile.Metadata);
        }

        [TestCase]
        public void Can_TranscodeUsingConversionOptions()
        {
            string outputPath = string.Format("{0}/{1}.Transcoded.avi",
                                               Path.GetDirectoryName(_outputFilePath), 
                                               Path.GetFileNameWithoutExtension(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };
            var conversionOptions = new ConversionOptions
            {
                MaxVideoDuration = TimeSpan.FromSeconds(30),
                VideoAspectRatio = VideoAspectRatio.R16_9,
                VideoSize = VideoSize.Hd720,
                AudioSampleRate = AudioSampleRate.Hz44100
            };

            if (_raiseEvents) Engine.ConvertProgressEvent += EngineConvertProgressEvent;

            using (var engine = new Engine())
                engine.Convert(inputFile, outputFile, conversionOptions);
            
        }

        private void EngineConvertProgressEvent(object sender, ConvertProgressEventArgs e)
        {
            Console.WriteLine("Bitrate: {0}", e.Bitrate);
            Console.WriteLine("Fps: {0}", e.Fps);
            Console.WriteLine("Frame: {0}", e.Frame);
            Console.WriteLine("ProcessedDuration: {0}", e.ProcessedDuration);
            Console.WriteLine("SizeKb: {0}", e.SizeKb);
            Console.WriteLine("TotalDuration: {0}\n", e.TotalDuration);
        }

        private void PrintMetadata(Metadata meta)
        {
            Console.WriteLine("File metadata");
            Console.WriteLine("Duration:                {0}", meta.Duration);
            Console.WriteLine("VideoData.Format:        {0}", meta.VideoData.Format);
            Console.WriteLine("VideoData.ColorModel:    {0}", meta.VideoData.ColorModel);
            Console.WriteLine("VideoData.FrameSize:     {0}", meta.VideoData.FrameSize);
            Console.WriteLine("VideoData.Fps:           {0}", meta.VideoData.Fps);
            Console.WriteLine("VideoData.BitRate:       {0}", meta.VideoData.BitRateKbs);
            Console.WriteLine("AudioData.Format:        {0}", meta.AudioData.Format);
            Console.WriteLine("AudioData.SampleRate:    {0}", meta.AudioData.SampleRate);
            Console.WriteLine("AudioData.ChannelOutput: {0}", meta.AudioData.ChannelOutput);
            Console.WriteLine("AudioData.BitRate:       {0}\n", meta.AudioData.BitRateKbs);
        }
    }
}