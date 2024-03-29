using System;
using System.Threading.Tasks;

namespace Scar.Common.MVVM.Commands;

public class AsyncCorrelationCommand<T>(
        ICommandManager commandManager,
        Func<T, Task> executeFunc,
        Predicate<T>? canExecuteFunc = null,
        string? debugName = null)
    : BaseCommand<Func<T, Task>, Predicate<T>>(
        commandManager, executeFunc, canExecuteFunc, debugName)
{
    public override bool CanExecute(object? parameter)
    {
        return CanExecuteFunc?.Invoke((T)(parameter ?? throw new InvalidOperationException("parameter is null"))) ?? true;
    }

    public override void ExecuteInternal(object? parameter)
    {
        ExecuteFunc((T)(parameter ?? throw new InvalidOperationException("parameter is null")));
    }
}
