namespace Scar.Common.WebApi;

public interface ISslSettings
{
    string? PfxCertificatePath { get; }

    string? PfxPassword { get; }
}
