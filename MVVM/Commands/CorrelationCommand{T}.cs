using System;

namespace Scar.Common.MVVM.Commands;

public class CorrelationCommand<T>(
        ICommandManager commandManager,
        Action<T> executeFunc,
        Predicate<T>? canExecuteFunc = null,
        string? debugName = null)
    : BaseCommand<Action<T>, Predicate<T>>(
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
