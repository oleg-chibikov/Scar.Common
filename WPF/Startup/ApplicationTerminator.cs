using System.Windows;
using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.WPF.Startup;

public class ApplicationTerminator : IApplicationTerminator
{
    public void Terminate()
    {
        Application.Current.Shutdown();
    }
}
