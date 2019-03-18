using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowFactory
{
    public interface IWindowFactory<TWindow>
        where TWindow : class, IDisplayable
    {
        Task<TWindow> GetWindowAsync(CancellationToken cancellationToken);

        Task<TWindow?> GetWindowIfExistsAsync(CancellationToken cancellationToken);

        Task<TWindow> ShowWindowAsync(CancellationToken cancellationToken);

        Task<TWindow> ShowWindowAsync<TParam>(CancellationToken cancellationToken, TParam param);

        Task<TWindow> ShowWindowAsync<TSplashWindow>(IWindowFactory<TSplashWindow>? splashWindowFactory, CancellationToken cancellationToken)
            where TSplashWindow : class, IDisplayable;

        Task<TWindow> ShowWindowAsync<TSplashWindow, TParam>(
            IWindowFactory<TSplashWindow>? splashWindowFactory,
            CancellationToken cancellationToken,
            TParam param)
            where TSplashWindow : class, IDisplayable;
    }
}