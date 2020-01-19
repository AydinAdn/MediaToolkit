using MediaToolkit.Core.CommandHandler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MediaToolkit.Core.Test
{
    [TestFixture]
    public class MediaToolkitTests
    {
        [Test]
        public async Task Instantiation()
        {
            var factory = LoggerFactory.Create(b =>
            {
                b.AddConsole();
            });
            var logger = factory.CreateLogger<Toolkit>();

            var mediaToolkit = new Toolkit(logger);

            await mediaToolkit.ExecuteInstruction(new EmptyInstruction(), default);
        }

    }

    public class EmptyInstruction : IInstruction
    {
        public EmptyInstruction()
        {
            this.Instruction = "";
        }
        public string Instruction { get; set; }
    }
}
