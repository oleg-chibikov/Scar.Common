using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Scar.Common.WPF.View.Contracts;

namespace Scar.Common.WPF.View
{
    public interface IScopedWindowProvider
    {
        [NotNull]
        [ItemNotNull]
        Task<TWindow> GetScopedWindowAsync<TWindow, TParam>([CanBeNull] TParam param, CancellationToken cancellationToken)
            where TWindow : IWindow;
    }
}