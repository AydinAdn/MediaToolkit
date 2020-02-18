namespace MediaToolkit.Core.Services
{
    public interface IProcessServiceConfiguration
    {
        string ExePath { get; set; }
        string GlobalArguments { get; set; }
        string EmbeddedResourceId { get; set; }
    }
}