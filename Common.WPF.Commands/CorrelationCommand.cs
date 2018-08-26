using System;

namespace Scar.Common.WPF.Commands
{
    public class CorrelationCommand : BaseCommand<Action, Func<bool>>
    {
        public CorrelationCommand(Action executeFunc, Func<bool> canExecuteFunc = null)
            : base(executeFunc, canExecuteFunc)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return CanExecuteFunc?.Invoke() ?? true;
        }

        public override void ExecuteInternal(object parameter)
        {
            ExecuteFunc();
        }
    }
}