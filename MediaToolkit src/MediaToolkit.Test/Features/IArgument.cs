using System.Text;

namespace MediaToolkit.Test.Features
{
    public interface IArgument
    {
        StringBuilder Argument { get; }

        void ComposeArgument();
    }
}