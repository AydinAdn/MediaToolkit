using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Threading.Tasks;
using MediaToolkit.Core.Infrastructure;
using System.Threading;
using MediaToolkit.Core.Services;

namespace MediaToolkit.Core.Test
{
    [TestFixture]
    public class MediaConverterServiceTests
    {
        private const string TestVideoResourceId = "MediaToolkit.Core.Test.Resources.BigBunny.m4v";

        private ILogger<IMediaConverterService> logger;
        private IMediaConverterService mediaConverter;
        private string testDir;
        private string videoPath;


        public async Task Setup(bool outputToConsole)
        {
            ILoggerFactory factory = LoggerFactory.Create(config =>
            {
                config.AddConsole();
                config.SetMinimumLevel(outputToConsole ? LogLevel.Trace : LogLevel.None);
            });

            this.logger = factory.CreateLogger<IMediaConverterService>();
            this.mediaConverter = new MediaConverterService(this.logger);
            this.testDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName + "/TestResults";
            this.videoPath = this.testDir + "/BigBunny.m4v";

            if (outputToConsole)
            {
                this.mediaConverter.OnWarningEventHandler += (sender, args) =>
                {
                    Console.WriteLine($"### Warning > {args.Warning}");
                };

                this.mediaConverter.OnProgressUpdateEventHandler += (sender, args) =>
                {
                    int max = args.UpdateData.Max(x => x.Key.Length) + 1;
                    IEnumerable<string> updateData = args.UpdateData.Select(x => $"{x.Key.PadRight(max)}={x.Value}");
                    string updateDataString = string.Join("\n", updateData);
                    Console.WriteLine("### Progress Update\n" + updateDataString + "\n");
                };

                this.mediaConverter.OnCompleteEventHandler += (sender, args) => { Console.WriteLine("### Complete"); };

            }

            if (File.Exists(this.videoPath)) { return; }

            Directory.CreateDirectory(this.testDir);
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            using (Stream embeddedVideoStream = currentAssembly.GetManifestResourceStream(TestVideoResourceId))
            using (FileStream fileStream = new FileStream(this.videoPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                // ReSharper disable once PossibleNullReferenceException
                await embeddedVideoStream.CopyToAsync(fileStream);
            }
        }

        [OneTimeSetUp()]
        public async Task Setup()
        {
            await this.Setup(true);
        }

        #region InstructionBuilder Tests

        [Test]
        public async Task Custom_InstructionBuilder_Test()
        {
          
            IInstructionBuilder custom = new CustomInstructionBuilder
            {
                Instruction = $@" -i ""{this.videoPath}"" ""{Path.ChangeExtension(this.videoPath, ".mp4")}"""
            };

            await this.mediaConverter.ExecuteInstructionAsync(custom);
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

            await this.mediaConverter.ExecuteInstructionAsync(builder);

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

            await this.mediaConverter.ExecuteInstructionAsync(builder);
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

            await this.mediaConverter.ExecuteInstructionAsync(builder);
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

            await this.mediaConverter.ExecuteInstructionAsync(builder);
        }

        [Test]
        public async Task Basic_InstructionBuilder_Test()
        {
            IInstructionBuilder builder = new BasicInstructionBuilder
            {
                InputFilePath = this.videoPath,
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Basic_Conversion_Test.mp4")
            };

            await this.mediaConverter.ExecuteInstructionAsync(builder);
        }

        [Test]
        public async Task ConvertToGif_InstructionBuilder_Test()
        {
            IInstructionBuilder builder = new BasicInstructionBuilder
            {
                InputFilePath = this.videoPath,
                OutputFilePath = Path.ChangeExtension(this.videoPath, "Get_Gif_Test.gif"),
            };

            await this.mediaConverter.ExecuteInstructionAsync(builder);
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
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
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
                        await this.mediaConverter.ExecuteInstructionAsync(builder);
                        return;
                    }

                    limiter.WaitOne();
                    await this.mediaConverter.ExecuteInstructionAsync(builder);
                    limiter.Release();
                }

                tasks[icopy] = LimiterWrapper();
            }

            await Task.WhenAll(tasks);

            for (int i = 0; i < totalTasks; i++)
            {
                File.Delete(Path.ChangeExtension(this.videoPath, "Basic_Conversion_Test" + i + ".mp4") ?? throw new InvalidOperationException());
            }

            limiter.Dispose();
        }

        #endregion

    }


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
