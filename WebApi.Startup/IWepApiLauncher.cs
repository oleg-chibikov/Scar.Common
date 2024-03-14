using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Scar.Common.WebApi.Startup;

public interface IWepApiLauncher
{
    IConfiguration Configuration { get; }

    Task BuildAndRunHostAsync(string[] args);
}
