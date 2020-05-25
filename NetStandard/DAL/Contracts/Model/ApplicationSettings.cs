namespace Scar.Common.DAL.Contracts.Model
{
    public sealed class ApplicationSettings : TrackedEntity<string>
    {
#pragma warning disable 8618
        public string ValueJson { get; set; }
#pragma warning restore 8618
    }
}
