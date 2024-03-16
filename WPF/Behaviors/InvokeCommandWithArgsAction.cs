using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scar.Common.WPF.Behaviors;

public class InvokeCommandWithArgsAction : TriggerAction<DependencyObject>
{
    // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(InvokeCommandWithArgsAction), new UIPropertyMetadata(null));
    public static readonly DependencyProperty CommandParameterConverterProperty = DependencyProperty.Register(
        nameof(CommandParameterConverter),
        typeof(IValueConverter),
        typeof(InvokeCommandWithArgsAction),
        new UIPropertyMetadata(null));

    string _commandName = string.Empty;

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

    public ICommand? Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public IValueConverter? CommandParameterConverter
    {
        get => (IValueConverter)GetValue(CommandParameterConverterProperty);
        set => SetValue(CommandParameterConverterProperty, value);
    }

    protected override void Invoke(object? parameter)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        var command = ResolveCommand();

        var converter = CommandParameterConverter;
        if (converter != null)
        {
            parameter = converter.Convert(parameter, typeof(object), null, CultureInfo.InvariantCulture);
        }

        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }

    ICommand? ResolveCommand()
    {
        ICommand? command = null;
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
                command = info.GetValue(AssociatedObject, null) as ICommand;
            }
        }

        return command;
    }
}
