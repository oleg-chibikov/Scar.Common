using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using JetBrains.Annotations;

namespace Scar.Common.WPF.Controls.Behaviors
{
    public class InvokeCommandWithArgsAction : TriggerAction<DependencyObject>
    {
        private string _commandName;

        public string CommandName
        {
            get
            {
                ReadPreamble();
                return _commandName;
            }
            set
            {
                if (CommandName != value)
                {
                    WritePreamble();
                    _commandName = value;
                    WritePostscript();
                }
            }
        }

        protected override void Invoke(object parameter)
        {
            if (AssociatedObject != null)
            {
                var command = ResolveCommand();

                if (CommandParameterConverter != null)
                {
                    parameter = CommandParameterConverter.Convert(parameter, typeof(object), null, CultureInfo.CurrentCulture);
                }

                if (command?.CanExecute(parameter) == true)
                {
                    command.Execute(parameter);
                }
            }
        }

        [CanBeNull]
        private ICommand ResolveCommand()
        {
            ICommand command = null;
            if (Command != null)
            {
                return Command;
            }

            if (AssociatedObject != null)
            {
                foreach (var info in AssociatedObject.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(info => typeof(ICommand).IsAssignableFrom(info.PropertyType) && string.Equals(info.Name, CommandName, StringComparison.Ordinal)))
                {
                    command = (ICommand)info.GetValue(AssociatedObject, null);
                }
            }

            return command;
        }

        #region Command

        [CanBeNull]
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(InvokeCommandWithArgsAction),
            new UIPropertyMetadata(null));

        #endregion

        #region Command

        [CanBeNull]
        public IValueConverter CommandParameterConverter
        {
            get => (IValueConverter)GetValue(CommandParameterConverterProperty);
            set => SetValue(CommandParameterConverterProperty, value);
        }

        public static readonly DependencyProperty CommandParameterConverterProperty = DependencyProperty.Register(
            "CommandParameterConverter",
            typeof(IValueConverter),
            typeof(InvokeCommandWithArgsAction),
            new UIPropertyMetadata(null));

        #endregion
    }
}