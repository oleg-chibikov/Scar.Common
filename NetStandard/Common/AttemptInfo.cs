namespace Scar.Common;

public sealed class AttemptInfo
{
    public AttemptInfo(int attempt, int maxAttempts)
    {
        Attempt = attempt;
        MaxAttempts = maxAttempts;
    }

    public int Attempt { get; }

    public bool HasAttempts => Attempt < MaxAttempts;

    public int MaxAttempts { get; }

    public override string ToString()
    {
        return $"Attempt {Attempt + 1} of {MaxAttempts}";
    }
}