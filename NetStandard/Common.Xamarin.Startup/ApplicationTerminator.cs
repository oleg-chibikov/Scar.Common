using Scar.Common.ApplicationLifetime.Contracts;

namespace Scar.Common.Xamarin.Startup
{
    public class ApplicationTerminator : IApplicationTerminator
    {
        public void Terminate()
        {
            // TODO: Platform specific https://stackoverflow.com/questions/29257929/how-to-terminate-a-xamarin-application
        }
    }
}
