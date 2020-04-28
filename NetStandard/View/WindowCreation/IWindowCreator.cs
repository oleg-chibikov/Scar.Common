using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowCreation
{
    public interface IWindowCreator<T>
        where T : IDisplayable
    {
        Task<T> CreateWindowAsync(CancellationToken cancellationToken);
    }

    public interface IWindowCreator<TWindow, in TParam>
        where TWindow : IDisplayable
    {
        Task<TWindow> CreateWindowAsync(TParam param, CancellationToken cancellationToken);
    }
}
