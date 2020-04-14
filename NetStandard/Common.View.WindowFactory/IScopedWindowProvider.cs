using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common
{
    public interface IScopedWindowProvider
    {
        Task<TWindow> GetScopedWindowAsync<TWindow, TParam>(TParam param, CancellationToken cancellationToken)
            where TWindow : IDisplayable;
    }
}
