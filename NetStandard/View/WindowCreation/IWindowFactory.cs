using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common
{
    public interface IWindowFactory<TWindow>
        where TWindow : class, IDisplayable
    {
        Task<TWindow> GetWindowAsync(CancellationToken cancellationToken);

        Task<TWindow?> GetWindowIfExistsAsync(CancellationToken cancellationToken);

        Task<TWindow> ShowWindowAsync(CancellationToken cancellationToken);

        Task<TWindow> ShowWindowAsync<TParam>(TParam param, CancellationToken cancellationToken);

        Task<TWindow> ShowWindowAsync<TSplashWindow>(IWindowFactory<TSplashWindow>? splashWindowFactory, CancellationToken cancellationToken)
            where TSplashWindow : class, IDisplayable;

        Task<TWindow> ShowWindowAsync<TSplashWindow, TParam>(IWindowFactory<TSplashWindow>? splashWindowFactory, TParam param, CancellationToken cancellationToken)
            where TSplashWindow : class, IDisplayable;
    }
}
