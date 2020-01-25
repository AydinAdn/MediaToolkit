using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Threading.Tasks;
using MediaToolkit.Core.Infrastructure;
using System.Threading;

namespace MediaToolkit.Core.Test
{
    [TestFixture]
    public class MediaToolkitTests
    {
        private const string TestVideoResourceId = "MediaToolkit.Core.Test.Resources.BigBunny.m4v";

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

            this.logger    = factory.CreateLogger<Toolkit>();
            this.toolkit   = new Toolkit(this.logger);
            this.testDir   = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName + "/TestResults";
            this.videoPath = this.testDir + "/BigBunny.m4v";

            this.toolkit.OnWarningEventHandler += (sender, args) =>
            {
                Console.WriteLine($"### Warning > {args.Warning}");
            };

            this.toolkit.OnProgressUpdateEventHandler += (sender, args) =>
            {
                var max = args.UpdateData.Max(x => x.Key.Length) + 1;
                var updateData = args.UpdateData.Select(x => $"{x.Key.PadRight(max)}={x.Value}");
                var updateDataString = string.Join("\n", updateData);
                Console.WriteLine("### Progress Update\n" + updateDataString+"\n");
            };

            this.toolkit.OnCompleteEventHandler += (sender, args) =>
            {
                Console.WriteLine("### Complete");
            };

            if (File.Exists(this.videoPath))
            {
                return;
            }

            Directory.CreateDirectory(this.testDir);
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            using (Stream embeddedVideoStream = currentAssembly.GetManifestResourceStream(TestVideoResourceId))
            using (FileStream fileStream = new FileStream(this.videoPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                // ReSharper disable once PossibleNullReferenceException
                await embeddedVideoStream.CopyToAsync(fileStream);
            }

            
        }

        #region InstructionBuilder Tests

        [Test]
        public async Task Custom_InstructionBuilder_Test()
        {
            IInstructionBuilder custom = new CustomInstructionBuilder
            {
                Instruction = $@" -i ""{this.videoPath}"" ""{Path.ChangeExtension(this.videoPath, ".mp4")}"""
            };

            await this.toolkit.ExecuteInstruction(custom, default);
        }

        [Test]
        public async Task TrimMedia_InstructionBuilder_Test()
        {
            IInstructionBuilder builder = new TrimMediaInstructionBuilder
            {
                InputFilePath = this.videoPath,
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Trim_Video_Test.mp4"),
                SeekFrom = TimeSpan.FromSeconds(5),
                Duration = TimeSpan.FromSeconds(25)
            };

            await this.toolkit.ExecuteInstruction(builder, default);

            // Output video will be only a few seconds, not 25 seconds long because 
            // total video length is 33 or so seconds. The current duration set is longer
            // than the remaining length of video due to seeking 30 seconds forward. 
            
            // tldr: Expected behaviour
        }

        [Test]
        public async Task CropVideo_InstructionBuilder_Test()
        {
            IInstructionBuilder builder = new CropVideoInstructionBuilder
            {
                InputFilePath = this.videoPath,
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Crop_Video_Test.mp4"),
                X = 100,
                Y = 100,
                Width = 50,
                Height = 50
            };

            await this.toolkit.ExecuteInstruction(builder, default);
        }

        [Test]
        public async Task ExctractThumbnail_InstructionBuilder_Test()
        {
            IInstructionBuilder builder = new ExtractThumbnailInstructionBuilder
            {
                InputFilePath = this.videoPath,
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Get_Thumbnail_Test.jpg"),
                SeekFrom = TimeSpan.FromSeconds(10)
            };

            await this.toolkit.ExecuteInstruction(builder, default);
        }

        [Test]
        public async Task ExctractOnlineThumbnail_InstructionBuilder_Test()
        {
            IInstructionBuilder builder = new ExtractThumbnailInstructionBuilder
            {
                InputFilePath = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4",
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Get_Thumbnail_Online_Test.jpg"),
                SeekFrom = TimeSpan.FromSeconds(10)
            };

            await this.toolkit.ExecuteInstruction(builder, default);
        }

        [Test]
        public async Task Basic_InstructionBuilder_Test()
        {
            IInstructionBuilder builder = new BasicInstructionBuilder
            {
                InputFilePath = this.videoPath,
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Basic_Conversion_Test.mp4")
            };

            await this.toolkit.ExecuteInstruction(builder, default);
        }

        [Test]
        public async Task ConvertToGif_InstructionBuilder_Test()
        {
            IInstructionBuilder builder = new ConvertToGifInstructionBuilder()
            {
                InputFilePath = this.videoPath,
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Get_Gif_Test.gif"),
            };

            await this.toolkit.ExecuteInstruction(builder, default);
        }

        /// <summary>
        ///     HEADS UP! runs a 100 concurrent threads, it 
        ///     will max out your resources for a few minutes.
        /// 
        ///     For reference:
        ///         The test takes 140 to 170 seconds on my machine (DELL XPS 9560)
        ///                         
        ///                                 Utilization     
        ///         CPU: i7-7700HQ     |    100% @ 3.4 GHz
        ///         RAM: 32Gb DDR4     |    22Gb (8-9 Gb used by the test itself)      
        /// </summary>
        /// <param name="execute">
        ///     Set to true to execute
        /// </param>
        /// <param name="threads">
        ///     Default is 100.
        /// </param>
        /// <param name="useLimiter">
        ///     Limits the maximum number of concurrent threads to the number of logical cores you have.
        ///     Set to true if you're limited on RAM, it wont make much difference for CPU utilization 
        ///     but will drastically reduce the amount of ram required.
        /// </param>
        [TestCase(false, 100, false)]
        public async Task Basic_Concurrency_InstructionBuilder_Test(bool execute, int threads, bool useLimiter)
        {
            if (execute == false) { return; }
            int totalTasks = threads;

            Semaphore limiter = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);
            
            Task[] tasks = new Task[totalTasks];

            for (int i = 0; i < totalTasks; i++)
            {
                int icopy = i;

                IInstructionBuilder builder = new BasicInstructionBuilder
                {
                    InputFilePath = this.videoPath,
                    OutputFilePath = Path.ChangeExtension(this.videoPath, "Basic_Conversion_Test" + icopy + ".mp4")
                };

                async Task LimiterWrapper()
                {
                    if (!useLimiter)
                    {
                        await this.toolkit.ExecuteInstruction(builder, default);
                        return;
                    }

                    limiter.WaitOne();
                    await this.toolkit.ExecuteInstruction(builder, default);
                    limiter.Release();
                }

                tasks[icopy] = LimiterWrapper();
            }

            await Task.WhenAll(tasks);

            for (int i = 0; i < totalTasks; i++)
            {
                File.Delete(Path.ChangeExtension(this.videoPath, "Basic_Conversion_Test" + i + ".mp4"));
            }

            limiter.Dispose();
        }

        #endregion

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
        public TimeSpan? SeekFrom       { get; set; }

        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ",    this.SeekFrom.GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalSeconds);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -t {0} ",     this.Duration);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ",    this.OutputFilePath);
            return builder.ToString();
        }
    }

    public class CropVideoInstructionBuilder : IInstructionBuilder
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }


        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -filter:v \"crop={0}:{1}:{2}:{3}\" ", this.Width, this.Height, this.X, this.Y);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ",    this.OutputFilePath);
            return builder.ToString();
        }
    }

    public class ExtractThumbnailInstructionBuilder : IInstructionBuilder
    {
        public TimeSpan? SeekFrom { get; set; }
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
        public int Frames { get; set; } = 1;

        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -ss {0} ",    this.SeekFrom.GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalSeconds);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " -vframes {0} ", this.Frames);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ",    this.OutputFilePath);
            return builder.ToString();
        }
    }

    public class BasicInstructionBuilder : IInstructionBuilder
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }

        public string BuildInstructions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, " -i \"{0}\" ", this.InputFilePath);
            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\" ", this.OutputFilePath);
            return builder.ToString();
        }
    }

    public class ConvertToGifInstructionBuilder : BasicInstructionBuilder
    {
    }


    #endregion



    //TODO: Reimplement the existing commands implementing IInstruction


    // GET METADATA

    // -nostdin
    // -y
    // -loglevel
    // info
    // -i
    // "C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v"


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
