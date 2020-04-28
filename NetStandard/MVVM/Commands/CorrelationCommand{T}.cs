using System;

namespace Scar.Common.MVVM.Commands
{
    public class CorrelationCommand<T> : BaseCommand<Action<T>, Predicate<T>>
    {
        public CorrelationCommand(ICommandManager commandManager, Action<T> executeFunc, Predicate<T>? canExecuteFunc = null) : base(commandManager, executeFunc, canExecuteFunc)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return CanExecuteFunc?.Invoke((T)parameter) ?? true;
        }

        public override void ExecuteInternal(object parameter)
        {
            ExecuteFunc((T)parameter);
        }
    }
}
