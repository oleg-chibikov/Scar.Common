using System;

namespace Scar.Common.MVVM.Commands
{
    public class CorrelationCommand : BaseCommand<Action, Func<bool>>
    {
        public CorrelationCommand(ICommandManager commandManager, Action executeFunc, Func<bool>? canExecuteFunc = null) : base(commandManager, executeFunc, canExecuteFunc)
        {
        }

        public override bool CanExecute(object? parameter)
        {
            return CanExecuteFunc?.Invoke() ?? true;
        }

        public override void ExecuteInternal(object? parameter)
        {
            ExecuteFunc();
        }
    }
}
