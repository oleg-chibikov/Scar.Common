using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Scar.Common.WPF.View.Contracts;

namespace Scar.Common.WPF.View
{
    public interface IWindowCreator<T>
        where T : IWindow
    {
        [NotNull]
        [ItemNotNull]
        Task<T> CreateWindowAsync(CancellationToken cancellationToken);
    }

    public interface IWindowCreator<TWindow, in TParam>
        where TWindow : IWindow
    {
        [NotNull]
        [ItemNotNull]
        Task<TWindow> CreateWindowAsync([NotNull] TParam param, CancellationToken cancellationToken);
    }
}