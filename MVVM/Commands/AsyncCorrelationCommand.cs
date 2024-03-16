using System;
using System.Threading.Tasks;

namespace Scar.Common.MVVM.Commands;

public class AsyncCorrelationCommand(
        ICommandManager commandManager,
        Func<Task> executeFunc,
        Func<bool>? canExecuteFunc = null,
        string? debugName = null)
    : BaseCommand<Func<Task>, Func<bool>>(
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
