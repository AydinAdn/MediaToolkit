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

        public const string InputFilePath  = @"C:\TEST\1.flv";
        public const string OutputFilePath = @"C:\TEST\1.mp4";

        [TestCase]
        public void Can_ConvertBasic()
        {
            var iFile = new MediaFile {Filename = InputFilePath};
            var oFile = new MediaFile {Filename = OutputFilePath};

            Engine.ConvertProgressEvent += EngineConvertProgressEvent;
            using (var engine = new Engine())
            {
                engine.Convert(iFile, oFile);
            }
        }

        [TestCase]
        public void Can_ConvertToDVD()
        {
            var iFile = new MediaFile {Filename = InputFilePath};
            var oFile = new MediaFile {Filename = OutputFilePath};
            var settings = new ConversionOptions {Target = Target.DVD, TargetStandard = TargetStandard.PAL};

            Engine.ConvertProgressEvent += EngineConvertProgressEvent;
            using (var engine = new Engine())
            {
                engine.Convert(iFile, oFile, settings);
            }
        }

        [TestCase]
        public void publicCan_LimitVideoDuration()
        {
            var iFile = new MediaFile {Filename = InputFilePath};
            var oFile = new MediaFile {Filename = OutputFilePath};
            var settings = new ConversionOptions {MaxVideoDuration = TimeSpan.FromSeconds(30)};

            Engine.ConvertProgressEvent += EngineConvertProgressEvent;
            using (var engine = new Engine())
            {
                engine.Convert(iFile, oFile, settings);
            }
        }

        [TestCase]
        public void Can_LimitVideoDurationAndMakeVga()
        {
            var iFile = new MediaFile {Filename = InputFilePath};
            var oFile = new MediaFile {Filename = OutputFilePath};
            var settings = new ConversionOptions
            {
                MaxVideoDuration = TimeSpan.FromSeconds(30),
                VideoSize = VideoSize.Vga
            };

            Engine.ConvertProgressEvent += EngineConvertProgressEvent;
            using (var engine = new Engine())
            {
                engine.Convert(iFile, oFile, settings);
            }
        }

        private void EngineConvertProgressEvent(object sender, ConvertProgressChangedEventArgs e)
        {
            Console.WriteLine("Bitrate: {0}", e.Bitrate);
            Console.WriteLine("Fps: {0}", e.Fps);
            Console.WriteLine("Frame: {0}", e.Frame);
            Console.WriteLine("ProcessedDuration: {0}", e.ProcessedDuration);
            Console.WriteLine("SizeKb: {0}", e.SizeKb);
            Console.WriteLine("TotalDuration: {0}\n", e.TotalDuration);
            Console.WriteLine(" ");
        }
    }
}