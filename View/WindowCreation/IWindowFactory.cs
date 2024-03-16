using System;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowCreation;

public interface IWindowFactory<TWindow>
    where TWindow : class, IDisplayable
{
    Task<TWindow> GetWindowAsync(CancellationToken cancellationToken);

    Task<TWindow?> GetWindowIfExistsAsync(CancellationToken cancellationToken);

    Task<Action<Action<TWindow>>> ShowWindowAsync(CancellationToken cancellationToken);

    Task<Action<Action<TWindow>>> ShowWindowAsync<TParam>(TParam param, CancellationToken cancellationToken);
}
