using System;
using System.Threading.Tasks;

namespace Scar.Common.MVVM.Commands;

public class AsyncCorrelationCommand<T> : BaseCommand<Func<T, Task>, Predicate<T>>
{
    public AsyncCorrelationCommand(ICommandManager commandManager, Func<T, Task> executeFunc, Predicate<T>? canExecuteFunc = null) : base(commandManager, executeFunc, canExecuteFunc)
    {
    }

    public override bool CanExecute(object? parameter)
    {
        return CanExecuteFunc?.Invoke((T)(parameter ?? throw new InvalidOperationException("parameter is null"))) ?? true;
    }

    public override void ExecuteInternal(object? parameter)
    {
        ExecuteFunc((T)(parameter ?? throw new InvalidOperationException("parameter is null")));
    }
}