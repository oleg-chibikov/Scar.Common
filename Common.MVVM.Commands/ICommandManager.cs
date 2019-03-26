using System;
using System.ComponentModel;

namespace Scar.Common.MVVM.Commands
{
    public interface ICommandManager
    {
        void AddRaiseCanExecuteChangedAction(ref Action raiseCanExecuteChangedAction);
        void RemoveRaiseCanExecuteChangedAction(Action raiseCanExecuteChangedAction);
        void AssignOnPropertyChanged(ref PropertyChangedEventHandler propertyEventHandler);
        void RefreshCommandStates();
    }
}