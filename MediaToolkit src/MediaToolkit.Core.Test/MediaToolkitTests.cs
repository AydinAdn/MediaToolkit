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
        private const string  TestVideoResourceId = "MediaToolkit.Core.Test.Resources.BigBunny.m4v";

        private ILogger<Toolkit> logger;
        private Toolkit toolkit;
        private string testDir;
        private string videoPath;



        [OneTimeSetUp]
        public async Task Setup()
        {
            ILoggerFactory factory = LoggerFactory.Create(config =>
            {
                config.AddConsole();
                config.SetMinimumLevel(LogLevel.Trace);
            });

            this.logger = factory.CreateLogger<Toolkit>();
            this.toolkit = new Toolkit(this.logger);
            this.testDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName + "/TestResults";
            this.videoPath = this.testDir + "/BigBunny.m4v";

            if (File.Exists(this.videoPath))
            {
                return;
            }

            Directory.CreateDirectory(this.testDir);
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            using (Stream embeddedVideoStream = currentAssembly.GetManifestResourceStream(TestVideoResourceId))
            using (FileStream fileStream = new FileStream(this.videoPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                await embeddedVideoStream.CopyToAsync(fileStream);
            }

        }

        [Test]
        public async Task CustomInstructionBuilder_Test()
        {
            IInstructionBuilder custom = new CustomInstructionBuilder
            {
                Instruction = $@" -i ""{this.videoPath}"" ""{Path.ChangeExtension(this.videoPath, ".mp4")}"""
            };

            await this.toolkit.ExecuteInstruction(custom, default);
        }

        [Test]
        public async Task TrimMediaInstructionBuilder_Test()
        {
            IInstructionBuilder trim = new TrimMediaInstructionBuilder
            {
                InputFilePath  = this.videoPath,
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Cut_Video_Test.mp4"),
                SeekFrom       = TimeSpan.FromSeconds(30),
                Duration       = TimeSpan.FromSeconds(25)
            };

            await this.toolkit.ExecuteInstruction(trim,  default);
        }

    }

    // TODO: refactor out of test project
    #region Completed InstructionBuilders - TODO: Refactoring
    public class CustomInstructionBuilder : IInstructionBuilder
    {
        public string Instruction { get; set; }

        public string BuildInstructions()
        {
            return this.Instruction;
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

    #endregion

    public class CropInstructionBuilder : IInstructionBuilder
    {
        


        public string BuildInstructions()
        {
            throw new NotImplementedException();
        }
    }

    //TODO: Reimplement the existing commands implementing IInstruction

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
