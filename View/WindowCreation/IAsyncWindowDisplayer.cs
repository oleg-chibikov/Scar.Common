using System;
using System.Threading;
using System.Threading.Tasks;
using Scar.Common.View.Contracts;

namespace Scar.Common.View.WindowCreation;

public interface IAsyncWindowDisplayer
{
    Task<Action<Action<TWindow>>> DisplayWindowAsync<TWindow>(Func<Task<TWindow>> createWindowAsync, CancellationToken cancellationToken)
        where TWindow : class, IDisplayable;
}