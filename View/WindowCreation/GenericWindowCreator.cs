using System;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowCreation;

public class GenericWindowCreator<TWindow>(Func<TWindow> windowFactory) : IWindowCreator<TWindow>
    where TWindow : IDisplayable
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Scar_Async", "Scar_Async_002:Non async method must not end with Async", Justification = "No need to make it Async")]
    public Task<TWindow> CreateWindowAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(windowFactory());
    }
}
