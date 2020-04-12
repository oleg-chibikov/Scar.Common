namespace Scar.Common.ApplicationLifetime.Contracts
{
    public interface IAssemblyInfoProvider
    {
        string AppGuid { get; }
        string Company { get; }
        string Product { get; }
        string ProgramName { get; }
        string SettingsPath { get; }
    }
}