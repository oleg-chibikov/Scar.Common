namespace Scar.Common.DAL.Contracts.Model;

public sealed class ApplicationSettings : TrackedEntity<string>
{
    public required string ValueJson { get; set; }
}
