using System;
using System.Threading.Tasks;

namespace Scar.Common.WPF.Commands
{
    public class AsyncCorrelationCommand : BaseCommand<Func<Task>, Func<bool>>
    {
        public AsyncCorrelationCommand(Func<Task> executeFunc, Func<bool> canExecuteFunc = null)
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