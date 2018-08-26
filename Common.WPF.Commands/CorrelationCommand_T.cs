using System;

namespace Scar.Common.WPF.Commands
{
    public class CorrelationCommand<T> : BaseCommand<Action<T>, Predicate<T>>
    {
        public CorrelationCommand(Action<T> executeFunc, Predicate<T> canExecuteFunc = null)
            : base(executeFunc, canExecuteFunc)
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