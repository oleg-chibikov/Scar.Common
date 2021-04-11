using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Scar.Common.MVVM.Commands
{
    public class ApplicationCommandManager : ICommandManager
    {
        readonly IList<Action> _raiseCanExecuteChangedActions = new List<Action>();
        readonly SynchronizationContext _synchronizationContext;

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
            _synchronizationContext.Post(
                x =>
                {
                    // ToList prevents CollectionModifiedException as it is a new object and the original collection can be modified during the enumeration
                    foreach (var raiseCanExecuteChangedAction in _raiseCanExecuteChangedActions.ToList())
                    {
                        raiseCanExecuteChangedAction?.Invoke();
                    }
                },
                null);
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // this if clause is to prevent an infinity loop
            if (e.PropertyName != "CanExecute")
            {
                RefreshCommandStates();
            }
        }
    }
}
