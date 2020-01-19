using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.Core.CommandHandler
{
    public interface IInstruction
    {
        string Instruction { get; set; }
    }
}
