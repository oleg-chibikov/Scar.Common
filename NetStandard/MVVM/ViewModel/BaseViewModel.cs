using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Scar.Common.MVVM.Commands;

namespace Scar.Common.MVVM.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable, IRequestCloseViewModel
    {
        readonly HashSet<IRemovableCommand> _commandsList;
        readonly ICommandManager _commandManager;
        bool _disposedValue;

        protected BaseViewModel(ICommandManager commandManager)
        {
            _commandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            _commandManager.AssignOnPropertyChanged(ref PropertyChanged!);
            _commandsList = new HashSet<IRemovableCommand>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? RequestClose;

        public void RemoveCommands()
        {
            foreach (var removableCommand in _commandsList)
            {
                removableCommand.RemoveCommand();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected IRefreshableCommand AddCommand(Func<Task> executeFunc, Func<bool>? canExecuteFunc = null)
        {
            var tempCmd = new AsyncCorrelationCommand(_commandManager, executeFunc, canExecuteFunc);
            return AddCommandToList(tempCmd);
        }

        protected IRefreshableCommand AddCommand<T>(Func<T, Task> executeFunc, Predicate<T>? canExecuteFunc = null)
        {
            var tempCmd = new AsyncCorrelationCommand<T>(_commandManager, executeFunc, canExecuteFunc);
            return AddCommandToList(tempCmd);
        }

        protected IRefreshableCommand AddCommand(Action executeFunc, Func<bool>? canExecuteFunc = null)
        {
            var tempCmd = new CorrelationCommand(_commandManager, executeFunc, canExecuteFunc);
            return AddCommandToList(tempCmd);
        }

        protected IRefreshableCommand AddCommand<T>(Action<T> executeFunc, Predicate<T>? canExecuteFunc = null)
        {
            var tempCmd = new CorrelationCommand<T>(_commandManager, executeFunc, canExecuteFunc);
            return AddCommandToList(tempCmd);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1045:Do not pass types by reference", Justification = "OK Here")]
        protected void ChangeProperty<T>(ref T property, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(property, value))
            {
                return;
            }

            property = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChangedEmpty()
        {
            OnPropertyChanged(string.Empty);
        }

        protected void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void CloseWindow()
        {
            RequestClose?.Invoke(this, new EventArgs());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    RemoveCommands();
                }

                _disposedValue = true;
            }
        }

        T AddCommandToList<T>(T tempCmd)
            where T : IRemovableCommand
        {
            _commandsList.Add(tempCmd);
            return tempCmd;
        }
    }
}
