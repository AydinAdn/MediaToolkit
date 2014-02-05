using System;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NUnit.Framework;

namespace MediaToolkit.Test
{
    [TestFixture]
    public class ConvertTest
    {
        /*
        * TODO: Implement assertions based on metadata
        *       once the conversion process is finished.
        * *************************************************/

        public const string InputFilePath = @"C:\TEST\1.flv";
        public const string OutputFilePath = @"C:\TEST\1.mp4";

        [TestCase]
        public void Can_ConvertBasic()
        {
            var inputFile = new MediaFile {Filename = InputFilePath};
            var outputFile = new MediaFile {Filename = OutputFilePath};

            Engine.ConvertProgressEvent += EngineConvertProgressEvent;
            using (var engine = new Engine())
            {
                engine.Convert(inputFile, outputFile);
            }
        }

        [TestCase]
        public void Can_ConvertToDVD()
        {
            var inputFile = new MediaFile {Filename = InputFilePath};
            var outputFile = new MediaFile {Filename = OutputFilePath};
            var conversionOptions = new ConversionOptions
            {
                Target = Target.DVD,
                TargetStandard = TargetStandard.PAL
            };

            Engine.ConvertProgressEvent += EngineConvertProgressEvent;
            using (var engine = new Engine())
            {
                engine.Convert(inputFile, outputFile, conversionOptions);
            }
        }

        [TestCase]
        public void Can_TranscodeUsingConversionOptions()
        {
            var inputFile = new MediaFile {Filename = InputFilePath};
            var outputFile = new MediaFile {Filename = OutputFilePath};
            var conversionOptions = new ConversionOptions
            {
                MaxVideoDuration = TimeSpan.FromSeconds(30),
                VideoAspectRatio = VideoAspectRatio.R16_9,
                VideoSize = VideoSize.Hd1080,
                AudioSampleRate = AudioSampleRate.Hz44100
            };

            Engine.ConvertProgressEvent += EngineConvertProgressEvent;
            using (var engine = new Engine())
            {
                engine.Convert(inputFile, outputFile, conversionOptions);
            }
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
    }
}