using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

[assembly: AssemblyCompany("Scar")]
[assembly: Guid("bac04c25-8738-47e5-a776-85a3f57fc088")]
[assembly: AssemblyProduct("Common")]

namespace Scar.Common.WebApi.Startup.Launcher
{
    static class Program
    {
        public static async Task Main(string[] args)
        {
            await new WepApiLauncher().BuildAndRunHostAsync(args).ConfigureAwait(false);
        }
    }
}
