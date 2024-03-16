using System;
using System.ComponentModel;

namespace Scar.Common.MVVM.Commands;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1045:Do not pass types by reference", Justification = "OK Here")]
public interface ICommandManager
{
    void AddRaiseCanExecuteChangedAction(ref Action raiseCanExecuteChangedAction);

    void RemoveRaiseCanExecuteChangedAction(Action raiseCanExecuteChangedAction);

    void AssignOnPropertyChanged(ref PropertyChangedEventHandler propertyEventHandler);

    void RefreshCommandStates();
}
