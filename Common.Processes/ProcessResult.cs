using JetBrains.Annotations;

namespace Scar.Common.Processes
{
    public sealed class ProcessResult
    {
        public ProcessResult([CanBeNull] string output, [CanBeNull] string error, int exitCode)
        {
            Output = output;
            Error = error;
            ExitCode = exitCode;
        }

        [CanBeNull]
        public string Output { get; }

        [CanBeNull]
        public string Error { get; }

        public int ExitCode { get; }
        public bool IsError => ExitCode != 0;
    }
}