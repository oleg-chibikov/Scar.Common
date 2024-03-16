using System;

namespace Scar.Common.MVVM.Commands;

public class CorrelationCommand(ICommandManager commandManager, Action executeFunc, Func<bool>? canExecuteFunc = null,
        string? debugName = null)
    : BaseCommand<Action, Func<bool>>(
        commandManager, executeFunc, canExecuteFunc, debugName)
{
    public override bool CanExecute(object? parameter)
    {
        return CanExecuteFunc?.Invoke() ?? true;
    }

    public override void ExecuteInternal(object? parameter)
    {
        ExecuteFunc();
    }
}
