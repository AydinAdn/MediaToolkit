using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MediaToolkit.Core.Infrastructure;
using MediaToolkit.Core.Services;
using NUnit.Framework;

namespace MediaToolkit.Core.Test
{
    [TestFixture]
    public class MetadataServiceTests
    {
        private const string TestVideoResourceId = "MediaToolkit.Core.Test.Resources.BigBunny.m4v";

        private MetadataService metadataService;
        private string testDir;
        private string videoPath;

        [OneTimeSetUp]
        public async Task Setup()
        {
            this.metadataService = new MetadataService();

            this.testDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName + "/TestResults";
            this.videoPath = this.testDir + "/BigBunny.m4v";

            this.metadataService.OnMetadataProcessedEventHandler += (sender, args) =>
            {
                Console.WriteLine(args.Metadata.RawMetaData);
            };

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

        #region Metadata Tests

        [Test]
        public async Task GetMetadataTest()
        {
            //MediaConverterTests tests = new MediaConverterTests();
            //await tests.Setup(false);
            //await tests.Basic_InstructionBuilder_Test();
            //await tests.ConvertToGif_InstructionBuilder_Test();
            //await tests.CropVideo_InstructionBuilder_Test();
            //await tests.Custom_InstructionBuilder_Test();
            //await tests.ExctractOnlineThumbnail_InstructionBuilder_Test();
            //await tests.ExctractThumbnail_InstructionBuilder_Test();
            //await tests.TrimMedia_InstructionBuilder_Test();

            foreach (string file in Directory.GetFiles(this.testDir))
            {
                IInstructionBuilder custom = new GetMetadataInstructionBuilder()
                {
                    InputFilePath = file
                };

                await this.metadataService.ExecuteInstructionAsync(custom);
            }
            
        }


        #endregion

    }
}
