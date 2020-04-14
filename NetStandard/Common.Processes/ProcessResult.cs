namespace Scar.Common.Processes
{
    public sealed class ProcessResult
    {
        public ProcessResult(string? output, string? error, int exitCode)
        {
            Output = output;
            Error = error;
            ExitCode = exitCode;
        }

        public string? Output { get; }

        public string? Error { get; }

        public int ExitCode { get; }

        public bool IsError => ExitCode != 0;
    }
}
