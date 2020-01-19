using MediaToolkit.Core.CommandHandler;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Threading.Tasks;

namespace MediaToolkit.Core.Test
{
    [TestFixture]
    public class MediaToolkitTests
    {
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

            await mediaToolkit.ExecuteInstruction(new EmptyInstruction(), default);
        }

    }

    public class EmptyInstruction : IInstruction
    {
        public EmptyInstruction()
        {
            this.Instruction = @"-i ""C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\BigBunny.m4v""  ""C:\Users\Aydin\source\repos\AydinAdn\MediaToolkit\MediaToolkit src\MediaToolkit.Test\TestVideo\Convert_Basic_Test.avi""";

        }
        public string Instruction { get; set; }
    }
}
