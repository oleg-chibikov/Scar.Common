using System;

namespace Scar.Common.MVVM.Commands;

public interface IUiThreadRunner
{
    void SetSynchronizationContext();

    void Run(Action action);
}
