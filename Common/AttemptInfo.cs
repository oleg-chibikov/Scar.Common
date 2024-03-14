namespace Scar.Common;

public sealed class AttemptInfo(int attempt, int maxAttempts)
{
    public int Attempt { get; } = attempt;

    public bool HasAttempts => Attempt < MaxAttempts;

    public int MaxAttempts { get; } = maxAttempts;

    public override string ToString()
    {
        return $"Attempt {Attempt + 1} of {MaxAttempts}";
    }
}