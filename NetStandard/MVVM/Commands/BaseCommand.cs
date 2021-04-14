using System;
using System.Diagnostics;

namespace Scar.Common.MVVM.Commands
{
    public abstract class BaseCommand<TExecute, TCanExecute> : IRefreshableCommand, IRemovableCommand
        where TExecute : class
        where TCanExecute : class
    {
        readonly Action _raiseCanExecuteChangedAction;

        protected BaseCommand(ICommandManager commandManager, TExecute executeFunc, TCanExecute? canExecuteFunc = null)
        {
            ExecuteFunc = executeFunc ?? throw new ArgumentNullException(nameof(executeFunc));
            CommandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            CanExecuteFunc = canExecuteFunc;
            _raiseCanExecuteChangedAction = RaiseCanExecuteChanged;
            CommandManager.AddRaiseCanExecuteChangedAction(ref _raiseCanExecuteChangedAction);
        }

        public event EventHandler? CanExecuteChanged;

        protected TCanExecute? CanExecuteFunc { get; }

        protected TExecute ExecuteFunc { get; }

        protected ICommandManager CommandManager { get; }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public void Execute(object? parameter)
        {
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            ExecuteInternal(parameter);
            CommandManager.RefreshCommandStates();
        }

        public abstract bool CanExecute(object? parameter);

        public void RemoveCommand()
        {
            CommandManager.RemoveRaiseCanExecuteChangedAction(_raiseCanExecuteChangedAction);
        }

        public abstract void ExecuteInternal(object? parameter);
    }
}
