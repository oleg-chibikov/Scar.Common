using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Scar.Common.MVVM.Commands
{
    public class ApplicationCommandManager : ICommandManager
    {
        private readonly IList<Action> _raiseCanExecuteChangedActions = new List<Action>();
        private readonly SynchronizationContext _synchronizationContext;

        public ApplicationCommandManager(SynchronizationContext synchronizationContext)
        {
            _synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));
        }

        public void AddRaiseCanExecuteChangedAction(ref Action raiseCanExecuteChangedAction)
        {
            _raiseCanExecuteChangedActions.Add(raiseCanExecuteChangedAction);
        }

        public void RemoveRaiseCanExecuteChangedAction(Action raiseCanExecuteChangedAction)
        {
            _raiseCanExecuteChangedActions.Remove(raiseCanExecuteChangedAction);
        }

        public void AssignOnPropertyChanged(ref PropertyChangedEventHandler propertyEventHandler)
        {
            propertyEventHandler += OnPropertyChanged;
        }

        public void RefreshCommandStates()
        {
            _synchronizationContext.Send(
                x =>
                {
                    foreach (var raiseCanExecuteChangedAction in _raiseCanExecuteChangedActions)
                    {
                        raiseCanExecuteChangedAction?.Invoke();
                    }
                },
                null);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // this if clause is to prevent an infinity loop
            if (e.PropertyName != "CanExecute")
            {
                RefreshCommandStates();
            }
        }
    }
}