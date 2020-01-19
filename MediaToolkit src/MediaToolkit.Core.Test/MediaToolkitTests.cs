using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Threading.Tasks;
using MediaToolkit.Core.Infrastructure;

namespace MediaToolkit.Core.Test
{
    [TestFixture]
    public class MediaToolkitTests
    {
        private static string _testDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/TestResults";


        [Test]
        public async Task Instantiation()
        {
            ILoggerFactory factory = LoggerFactory.Create(b =>
            {
                b.AddConsole();
                b.SetMinimumLevel(LogLevel.Trace);
            });
            ILogger<Toolkit> logger = factory.CreateLogger<Toolkit>();

            Toolkit mediaToolkit = new Toolkit(logger);

            await mediaToolkit.ExecuteInstruction(new EmptyInstructionBuilder(), default);
        }

        public const string TestVideoResourceId = "MediaToolkit.Core.Test.Resources.BigBunny.m4v";
        public static string TestDir { get => _testDir; set => _testDir = value; }


        [Test]
        public async Task UseVideo()
        {
            Directory.CreateDirectory(TestDir);
            Console.WriteLine(TestDir);
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            string videoPath = TestDir + "/BigBunny.m4v";
            using (Stream embeddedVideoStream = currentAssembly.GetManifestResourceStream(TestVideoResourceId))
            using (FileStream fileStream = new FileStream(videoPath, FileMode.Create,
                FileAccess.ReadWrite, FileShare.None))
            {
                await embeddedVideoStream.CopyToAsync(fileStream);
            }

            Console.WriteLine(videoPath);
        }

        [Test]
        public async Task TrimMediaInstructionBuilder_Test()
        {
            Directory.CreateDirectory(TestDir);
            Console.WriteLine(TestDir);
            string videoPath = TestDir + "/BigBunny.m4v";

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            using (Stream embeddedVideoStream = currentAssembly.GetManifestResourceStream(TestVideoResourceId))
            using (FileStream fileStream = new FileStream(videoPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                await embeddedVideoStream.CopyToAsync(fileStream);
            }

            ILoggerFactory factory = LoggerFactory.Create(b =>
            {
                b.AddConsole();
                b.SetMinimumLevel(LogLevel.Trace);
            });
            ILogger<Toolkit> logger = factory.CreateLogger<Toolkit>();

            Toolkit mediaToolkit = new Toolkit(logger);

            await mediaToolkit.ExecuteInstruction(new TrimMediaInstructionBuilder{
                SeekFrom = TimeSpan.FromSeconds(30),
                InputFilePath = videoPath,
                OutputFilePath = Path.ChangeExtension(videoPath, "Cut_Video_Test.mp4"),
                Duration = TimeSpan.FromSeconds(25)
            },  default);
        }

    }

    public class EmptyInstructionBuilder : IInstructionBuilder
    {
        public EmptyInstructionBuilder()
        {
            this.Instruction = @"-i ""C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v""  ""C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\Convert_Basic_Test.avi""";

        }
        public string Instruction { get; set; }
        public string BuildInstructions()
        {
            throw new NotImplementedException();
        }
    }

    public class TrimMediaInstructionBuilder : IInstructionBuilder
    {
        public string   InputFilePath  { get; set; }
        public string   OutputFilePath { get; set; }
        public TimeSpan Duration       { get; set; }
        public TimeSpan SeekFrom       { get; set; }

        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ",    this.SeekFrom.TotalSeconds);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -t {0} ",     this.Duration);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ",    this.OutputFilePath);
            return builder.ToString();
        }
    }





    //TODO: Reimplement the existing commands implementing IInstruction
    // CUT VIDEO

    // -nostdin
    // -y
    // -loglevel
    // info
    // -ss
    // 30
    // -i
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v"
    // -t
    // 00:00:25
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\Cut_Video_Test.mp4"



    // CROP VIDEO

    // -nostdin
    // -y
    // -loglevel
    // info
    // -i
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v"
    // -filter:v
    // "crop=50:50:100:100"
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\Crop_Video_Test.mp4"




    // GET THUMBNAIL

    // -nostdin
    // y
    // loglevel
    // nfo
    // -ss
    // 6.51
    // -i
    // C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v"
    // -vframes	
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\Get_Thumbnail_Test.jpg"




    // GET THUMBNAIL HTTP

    // -nostdin
    // -y
    // -loglevel
    // info
    // -ss
    // 30.05
    // -i
    // "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4"
    // -vframes
    // 1
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\Get_Thumbnail_FromHTTP_Test.jpg"
    // 



    // GET METADATA

    // -nostdin
    // -y
    // -loglevel
    // info
    // -i
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v"



    // BASIC CONVERSION

    // -nostdin
    // -y
    // -loglevel
    // info
    // -i
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v"
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\Convert_Basic_Test.avi"




    // GIF CONVERSION

    // -nostdin
    // -y
    // -loglevel
    // info
    // -i
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v"
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\Convert_GIF_Test.gif"




    // DVD CONVERSION

    // -nostdin
    // -y
    // -loglevel
    // info
    // -i
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v"
    // -target
    // pal-dvd
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo/Convert_DVD_Test.vob"


//    COMPLEX TRANSCODING INSTRUCTIONS
//    SCALING CONVERSION
}
