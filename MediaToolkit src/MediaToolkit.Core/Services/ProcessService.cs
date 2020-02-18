using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaToolkit.Core.Events;
using MediaToolkit.Core.Infrastructure;
using MediaToolkit.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace MediaToolkit.Core.Services
{
    public abstract class ProcessService : IProcessService
    {
        protected readonly ILogger Logger;
        private readonly IOUtilities utilities;
        private readonly IProcessServiceConfiguration configuration;

        protected ProcessService(IProcessServiceConfiguration configuration, ILogger logger = null)
        {
            this.configuration = configuration;
            this.utilities = new IOUtilities();
            this.Logger = logger;

            if (!this.configuration.EmbeddedResourceId.IsNullOrWhiteSpace())
            {
                this.utilities.DecompressResourceStream(this.configuration.EmbeddedResourceId, this.configuration.ExePath);
            }
        }                               

        public abstract event EventHandler<RawDataReceivedEventArgs> OnRawDataReceivedEventHandler;

        public abstract Task ExecuteInstructionAsync(string instruction, CancellationToken token = default);
        public abstract Task ExecuteInstructionAsync(IInstructionBuilder instruction, CancellationToken token = default);

        #region Helpers

        protected ProcessStartInfo GetProcessStartInfo(string instruction)
        {
            string arguments = $"{this.configuration.GlobalArguments} {instruction}";

            return new ProcessStartInfo
            {
                Arguments =  arguments,
                FileName = this.configuration.ExePath,
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        /// <summary>
        ///      Creates a temporary copy of the executable to enable the client to
        ///      process jobs in parallel.
        /// </summary>
        /// <returns>Path of temporary copy</returns>
        protected async Task<string> GetTempExe()
        {
            string exeCopyPath = this.utilities.ChangeFilePathName(this.configuration.ExePath, Path.GetRandomFileName());

            await this.utilities.CopyFileAsync(this.configuration.ExePath, exeCopyPath);

            return exeCopyPath;
        }

        #endregion
    }

}