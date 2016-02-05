using System;
using System.Collections.Generic;
using System.Text;
using MediaToolkit.Util;
using NUnit.Framework;

namespace MediaToolkit.Test.Features
{
    /*
     * The FFmpeg library supports a huge range of arguments and the current implementation
     * of CommandBuilder isn't really designed to allow clients to take full advantage of 
     * FFmpegs capabilities. So... about time we changed things up a little.
     * 
     * 
     * 
     * ******/


    [TestFixture]
    public class ArgumentsFeature
    {
        [Test]
        public void BuildArgument()
        {
            string expectedOutput = @"-ss 20 -i ""test.mp4"" -vframes 1 ""test.jpg""";

            string inputFilePath = @"test.mp4";
            string outputFilePath = @"test.jpg";
            TimeSpan timespan = TimeSpan.FromSeconds(20);

            IArgument argument = new GetThumbnailArgument(inputFilePath, outputFilePath, timespan);
            argument.ComposeArgument();

            Assert.That(argument.Argument.ToString() == expectedOutput, "{0} != {1}".FormatInvariant(argument.Argument, expectedOutput));
        }


    }
    public interface IArgument
    {
        StringBuilder Argument { get; }

        void ComposeArgument();
    }

    public class GetThumbnailArgument : IArgument
    {
        public GetThumbnailArgument(string inputFilePath, string outputFilePath, TimeSpan seekPosition)
        {
            this.Argument = new StringBuilder();
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
            SeekPosition = seekPosition;
        }

        public string InputFilePath { get; private set; }
        public string OutputFilePath { get; private set; }
        public TimeSpan SeekPosition { get; private set; }


        public void ComposeArgument()
        {
            this.Argument.Append("-ss {0} ".FormatInvariant(this.SeekPosition.TotalSeconds));
            this.Argument.Append("-i \"{0}\" ".FormatInvariant(this.InputFilePath));
            this.Argument.Append("-vframes {0} ".FormatInvariant(1));
            this.Argument.Append("\"{0}\"".FormatInvariant(this.OutputFilePath));
        }

        public StringBuilder Argument { get; private set; }
    }

    public interface IMediaProcessor<TArgument> where TArgument : IArgument
    {


    }
}
