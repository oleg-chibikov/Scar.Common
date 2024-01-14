namespace Scar.Common.Processes;

public sealed class ProcessResult(string? output, string? error, int exitCode)
{
    public string? Output { get; } = output;

    public string? Error { get; } = error;

    public int ExitCode { get; } = exitCode;

    public bool IsError => ExitCode != 0;
}
