using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Scar.Common.MVVM.Commands;

public class ApplicationCommandManager(IUiThreadRunner uiThreadRunner) : ICommandManager
{
    readonly List<Action> _raiseCanExecuteChangedActions = new();

    public void AddRaiseCanExecuteChangedAction(ref Action raiseCanExecuteChangedAction)
    {
        lock (_raiseCanExecuteChangedActions)
        {
            _raiseCanExecuteChangedActions.Add(raiseCanExecuteChangedAction);
        }
    }

    public void RemoveRaiseCanExecuteChangedAction(Action raiseCanExecuteChangedAction)
    {
        lock (_raiseCanExecuteChangedActions)
        {
            _raiseCanExecuteChangedActions.Remove(raiseCanExecuteChangedAction);
        }
    }

    public void AssignOnPropertyChanged(ref PropertyChangedEventHandler propertyEventHandler)
    {
        propertyEventHandler += OnPropertyChanged;
    }

    public void RefreshCommandStates()
    {
        IList<Action> copy;
        lock (_raiseCanExecuteChangedActions)
        {
            // ToList prevents CollectionModifiedException as it is a new object and the original collection can be modified during the enumeration
            copy = _raiseCanExecuteChangedActions.ToList();
        }

        uiThreadRunner.Run(
            () =>
            {
                foreach (var raiseCanExecuteChangedAction in copy)
                {
                    raiseCanExecuteChangedAction?.Invoke();
                }
            });
    }

    void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // this if clause is to prevent an infinite loop
        if (e.PropertyName != "CanExecute")
        {
            RefreshCommandStates();
        }
    }
}
