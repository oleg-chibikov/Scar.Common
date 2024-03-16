using System;
using System.Threading;

namespace Scar.Common.MVVM.Commands;

public class UiThreadRunner(SynchronizationContext? synchronizationContext) : IUiThreadRunner
{
    public void SetSynchronizationContext()
    {
        synchronizationContext = SynchronizationContext.Current ??
                                 throw new InvalidOperationException("SynchronizationContext.Current is null");
    }

    public void Run(Action action)
    {
        if (synchronizationContext == null)
        {
            throw new InvalidOperationException(
                "Synchronization context is not set");
        }

        synchronizationContext.Post(
            _ => action(),
            null);
    }
}
