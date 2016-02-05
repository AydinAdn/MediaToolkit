using MediaToolkit.Model;
using MediaToolkit.Options;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace MediaToolkit.Test
{
    [TestFixture]
    public class ConvertTest
    {
        [TestFixtureSetUp]
        public void Init()
        {
            // Raise progress events?
            _printToConsoleEnabled = true;

            const string inputFile = @"";
            const string outputFile = @"";

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (inputFile != "")
            {
                _inputFilePath = inputFile;
                if (outputFile != "")
                    _outputFilePath = outputFile;

                return;
            }

            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            Debug.Assert(directoryInfo.Parent != null, "directoryInfo.Parent != null");

            DirectoryInfo testDirectoryInfo = directoryInfo.Parent.Parent;
            Debug.Assert(testDirectoryInfo != null, "testDirectoryInfo != null");

            string testDirectoryPath = testDirectoryInfo.FullName + @"\TestVideo\";

            Debug.Assert(Directory.Exists(testDirectoryPath), "Directory not found: " + testDirectoryPath);
            Debug.Assert(File.Exists(testDirectoryPath + @"BigBunny.m4v"),
                "Test file not found: " + testDirectoryPath + @"BigBunny.m4v");

            _inputFilePath = testDirectoryPath + @"BigBunny.m4v";
            _inputUrlPath = @"http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4";
            _outputFilePath = testDirectoryPath + @"OuputBunny.mp4";
        }

        private string _inputFilePath = "";
        private string _inputUrlPath = "";
        private string _outputFilePath = "";
        private bool _printToConsoleEnabled;

        [TestCase]
        public void Can_CutVideo()
        {
            string filePath = @"{0}\Cut_Video_Test.mp4";
            string outputPath = string.Format(filePath, Path.GetDirectoryName(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };

            using (var engine = new Engine())
            {
                engine.ConvertProgressEvent += engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;

                engine.GetMetadata(inputFile);

                ConversionOptions options = new ConversionOptions();
                options.CutMedia(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(25));

                engine.Convert(inputFile, outputFile, options);
                engine.GetMetadata(outputFile);
            }
            
            Assert.That(File.Exists(outputPath));
            // Input file is 33 seconds long, seeking to the 30th second and then 
            // attempting to cut another 25 seconds isn't possible as there's only 3 seconds
            // of content length, so instead the library cuts the maximumum possible.
        }

        [TestCase]
        public void Can_CropVideo()
        {
            string outputPath = string.Format(@"{0}\Crop_Video_Test.mp4", Path.GetDirectoryName(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };

            using (var engine = new Engine())
            {
                engine.ConvertProgressEvent += engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;

                engine.GetMetadata(inputFile);

                var options = new ConversionOptions();
                options.SourceCrop = new CropRectangle()
                {
                    X = 100,
                    Y = 100,
                    Width = 50,
                    Height = 50
                };

                engine.Convert(inputFile, outputFile, options);
            }
        }

        [TestCase]
        public void Can_GetThumbnail()
        {
            string outputPath = string.Format(@"{0}\Get_Thumbnail_Test.jpg", Path.GetDirectoryName(_outputFilePath));
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var localMediaToolkitFfMpeg = Path.Combine(localAppData, "MediaToolkit", "ffmpeg.exe");

            using (var engine = new Engine(localMediaToolkitFfMpeg))
            {
                engine.ConvertProgressEvent += engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;

                engine.GetMetadata(inputFile);

                var options = new ConversionOptions
                {
                    Seek = TimeSpan.FromSeconds(inputFile.Metadata.Duration.TotalSeconds / 2)
                };
                engine.GetThumbnail(inputFile, outputFile, options);
            }
            Assert.That(File.Exists(outputPath));
        }

        [TestCase]
        public void Can_GetThumbnailFromHTTPLink()
        {
            string outputPath = string.Format(@"{0}\Get_Thumbnail_FromHTTP_Test.jpg", Path.GetDirectoryName(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputUrlPath };
            var outputFile = new MediaFile { Filename = outputPath };

            using (var engine = new Engine())
            {
                engine.ConvertProgressEvent += engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;

                engine.GetMetadata(inputFile);

                var options = new ConversionOptions
                {
                    Seek = TimeSpan.FromSeconds(inputFile.Metadata.Duration.TotalSeconds / 2)
                };
                engine.GetThumbnail(inputFile, outputFile, options);
            }
        }

        [TestCase]
        public void Can_GetMetadata()
        {
            var inputFile = new MediaFile { Filename = _inputFilePath };

            using (var engine = new Engine())
                engine.GetMetadata(inputFile);

            Metadata inputMeta = inputFile.Metadata;

            Debug.Assert(inputMeta.Duration != TimeSpan.Zero, "Media duration is zero", "  Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.Format != null, "Video format not found", "  Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.ColorModel != null, "Color model not found", "   Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.FrameSize != null, "Frame size not found", "    Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.Fps.ToString(CultureInfo.InvariantCulture) != "0", "Fps not found",
                "           Likely due to Regex code");
            Debug.Assert(inputMeta.VideoData.BitRateKbs != 0, "Video bitrate not found", " Likely due to Regex code");
            Debug.Assert(inputMeta.AudioData.Format != null, "Audio format not found", "  Likely due to Regex code");
            Debug.Assert(inputMeta.AudioData.SampleRate != null, "Sample rate not found", "   Likely due to Regex code");
            Debug.Assert(inputMeta.AudioData.ChannelOutput != null, "Channel output not found",
                "Likely due to Regex code");
            // Audio bit rate for some reson isn't returned by FFmreg for WEBM videos.
            //Debug.Assert(inputMeta.AudioData.BitRateKbs != 0, "Audio bitrate not found", " Likely due to Regex code");

            PrintMetadata(inputMeta);
        }

        [TestCase]
        public void Can_ConvertBasic()
        {
            string outputPath = string.Format(@"{0}\Convert_Basic_Test.avi", Path.GetDirectoryName(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };


            using (var engine = new Engine())
            {
                engine.ConvertProgressEvent += engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;

                engine.Convert(inputFile, outputFile);
                engine.GetMetadata(inputFile);
                engine.GetMetadata(outputFile);
            }

            Metadata inputMeta = inputFile.Metadata;
            Metadata outputMeta = outputFile.Metadata;

            PrintMetadata(inputMeta);
            PrintMetadata(outputMeta);
        }

        [TestCase]
        public void Can_ConvertToGif()
        {
            string outputPath = string.Format(@"{0}\Convert_GIF_Test.gif", Path.GetDirectoryName(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };


            using (var engine = new Engine())
            {
                engine.ConvertProgressEvent += engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;

                engine.Convert(inputFile, outputFile);
                engine.GetMetadata(inputFile);
                engine.GetMetadata(outputFile);
            }

            Metadata inputMeta = inputFile.Metadata;
            Metadata outputMeta = outputFile.Metadata;

            PrintMetadata(inputMeta);
            PrintMetadata(outputMeta);
        }


        [TestCase]
        public void Can_ConvertToDVD()
        {
            string outputPath = string.Format("{0}/Convert_DVD_Test.vob", Path.GetDirectoryName(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };

            var conversionOptions = new ConversionOptions { Target = Target.DVD, TargetStandard = TargetStandard.PAL };

            using (var engine = new Engine())
            {
                engine.ConvertProgressEvent += engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;

                engine.Convert(inputFile, outputFile, conversionOptions);

                engine.GetMetadata(inputFile);
                engine.GetMetadata(outputFile);
            }

            PrintMetadata(inputFile.Metadata);
            PrintMetadata(outputFile.Metadata);
        }

        [TestCase]
        public void Can_TranscodeUsingConversionOptions()
        {
            string outputPath = string.Format("{0}/Transcode_Test.avi", Path.GetDirectoryName(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };
            var conversionOptions = new ConversionOptions
            {
                MaxVideoDuration = TimeSpan.FromSeconds(30),
                VideoAspectRatio = VideoAspectRatio.R16_9,
                VideoSize = VideoSize.Hd720,
                AudioSampleRate = AudioSampleRate.Hz44100
            };


            using (var engine = new Engine())
                engine.Convert(inputFile, outputFile, conversionOptions);
        }

        [TestCase]
        public void Can_ScaleDownPreservingAspectRatio()
        {
            string outputPath = string.Format(@"{0}\Convert_Basic_Test.mp4", Path.GetDirectoryName(_outputFilePath));

            var inputFile = new MediaFile { Filename = _inputFilePath };
            var outputFile = new MediaFile { Filename = outputPath };

            using (var engine = new Engine())
            {
                engine.ConvertProgressEvent += engine_ConvertProgressEvent;
                engine.ConversionCompleteEvent += engine_ConversionCompleteEvent;

                engine.Convert(inputFile, outputFile, new ConversionOptions { VideoSize = VideoSize.Custom, CustomHeight = 120 });
                engine.GetMetadata(inputFile);
                engine.GetMetadata(outputFile);
            }

            Assert.AreEqual("214x120", outputFile.Metadata.VideoData.FrameSize);

            PrintMetadata(inputFile.Metadata);
            PrintMetadata(outputFile.Metadata);
        }

        private void engine_ConvertProgressEvent(object sender, ConvertProgressEventArgs e)
        {
            if (!_printToConsoleEnabled) return;

            Console.WriteLine("Bitrate: {0}", e.Bitrate);
            Console.WriteLine("Fps: {0}", e.Fps);
            Console.WriteLine("Frame: {0}", e.Frame);
            Console.WriteLine("ProcessedDuration: {0}", e.ProcessedDuration);
            Console.WriteLine("SizeKb: {0}", e.SizeKb);
            Console.WriteLine("TotalDuration: {0}\n", e.TotalDuration);
        }

        private void engine_ConversionCompleteEvent(object sender, ConversionCompleteEventArgs e)
        {
            if (!_printToConsoleEnabled) return;

            Console.WriteLine("\n------------\nConversion complete!\n------------");
            Console.WriteLine("Bitrate: {0}", e.Bitrate);
            Console.WriteLine("Fps: {0}", e.Fps);
            Console.WriteLine("Frame: {0}", e.Frame);
            Console.WriteLine("ProcessedDuration: {0}", e.ProcessedDuration);
            Console.WriteLine("SizeKb: {0}", e.SizeKb);
            Console.WriteLine("TotalDuration: {0}\n", e.TotalDuration);
        }

        private void PrintMetadata(Metadata meta)
        {
            if (!_printToConsoleEnabled) return;

            Console.WriteLine("\n------------\nMetadata\n------------");
            Console.WriteLine("Duration:                {0}", meta.Duration);
            if (meta.VideoData != null)
            {
                Console.WriteLine("VideoData.Format:        {0}", meta.VideoData.Format);
                Console.WriteLine("VideoData.ColorModel:    {0}", meta.VideoData.ColorModel);
                Console.WriteLine("VideoData.FrameSize:     {0}", meta.VideoData.FrameSize);
                Console.WriteLine("VideoData.Fps:           {0}", meta.VideoData.Fps);
                Console.WriteLine("VideoData.BitRate:       {0}", meta.VideoData.BitRateKbs);
            }
            else if (meta.AudioData != null)
            {
                Console.WriteLine("AudioData.Format:        {0}", meta.AudioData.Format ?? "");
                Console.WriteLine("AudioData.SampleRate:    {0}", meta.AudioData.SampleRate ?? "");
                Console.WriteLine("AudioData.ChannelOutput: {0}", meta.AudioData.ChannelOutput ?? "");
                Console.WriteLine("AudioData.BitRate:       {0}\n", (int?)meta.AudioData.BitRateKbs);
            }

        }
    }
}