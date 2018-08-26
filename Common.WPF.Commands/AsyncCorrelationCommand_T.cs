using System;
using System.Threading.Tasks;

namespace Scar.Common.WPF.Commands
{
    public class AsyncCorrelationCommand<T> : BaseCommand<Func<T, Task>, Predicate<T>>
    {
        public AsyncCorrelationCommand(Func<T, Task> executeFunc, Predicate<T> canExecuteFunc = null)
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