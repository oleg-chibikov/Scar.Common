using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Scar.Common.WPF.Commands
{
    public abstract class BaseCommand<TExecute, TCanExecute> : IRefreshableCommand
        where TExecute : class
        where TCanExecute : class
    {
        protected readonly TCanExecute CanExecuteFunc;
        protected readonly TExecute ExecuteFunc;

        protected BaseCommand(TExecute executeFunc, TCanExecute canExecuteFunc = null)
        {
            ExecuteFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
            CanExecuteFunc = canExecuteFunc;
        }

        public abstract bool CanExecute(object parameter);

        public event EventHandler CanExecuteChanged
        {
            add
            {
                InternalCanExecuteChanged += value;
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                InternalCanExecuteChanged -= value;
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        /// This method can be used to raise the CanExecuteChanged handler.
        /// This will force WPF to re-query the status of this command directly.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteFunc != null)
            {
                InternalCanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }

        public void Execute(object parameter)
        {
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            ExecuteInternal(parameter);
        }

        private event EventHandler InternalCanExecuteChanged;

        public abstract void ExecuteInternal(object parameter);
    }
}