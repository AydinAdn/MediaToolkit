namespace MediaToolkit.Core.Infrastructure
{
    public class CustomInstructionBuilder : IInstructionBuilder
    {
        public string Instruction { get; set; }

        public string BuildInstructions()
        {
            return this.Instruction;
        }
    }
}