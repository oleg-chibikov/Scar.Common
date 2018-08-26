using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Scar.Common.WPF.View.Contracts;

namespace Scar.Common.WPF.View
{
    public interface IWindowFactory<TWindow>
        where TWindow : class, IWindow
    {
        [NotNull]
        [ItemNotNull]
        Task<TWindow> GetWindowAsync(CancellationToken cancellationToken);

        [NotNull]
        [ItemCanBeNull]
        Task<TWindow> GetWindowIfExistsAsync(CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<TWindow> ShowWindowAsync(CancellationToken cancellationToken);

        [NotNull]
        [ItemNotNull]
        Task<TWindow> ShowWindowAsync<TParam>(CancellationToken cancellationToken, [CanBeNull] TParam param);

        [NotNull]
        [ItemNotNull]
        Task<TWindow> ShowWindowAsync<TSplashWindow>([CanBeNull] IWindowFactory<TSplashWindow> splashWindowFactory, CancellationToken cancellationToken)
            where TSplashWindow : class, IWindow;

        [NotNull]
        [ItemNotNull]
        Task<TWindow> ShowWindowAsync<TSplashWindow, TParam>(
            [CanBeNull] IWindowFactory<TSplashWindow> splashWindowFactory,
            CancellationToken cancellationToken,
            [CanBeNull] TParam param)
            where TSplashWindow : class, IWindow;
    }
}