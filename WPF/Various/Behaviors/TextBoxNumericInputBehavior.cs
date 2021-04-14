using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Scar.Common.WPF.Behaviors
{
    public class TextBoxNumericInputBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty OnlyPositiveProperty = DependencyProperty.Register(
            nameof(OnlyPositive),
            typeof(bool),
            typeof(TextBoxNumericInputBehavior),
            new FrameworkPropertyMetadata(false));

        const NumberStyles IntNumberStyles = NumberStyles.AllowThousands;
        const NumberStyles DecimalNumberStyles = IntNumberStyles | NumberStyles.AllowDecimalPoint;
        const string Empty = "0";

        public TextBoxNumericInputBehavior()
        {
            InputMode = TextBoxInputMode.None;
            OnlyPositive = false;
        }

        public TextBoxInputMode InputMode { get; set; }

        public bool OnlyPositive
        {
            get => (bool)GetValue(OnlyPositiveProperty);
            set => SetValue(OnlyPositiveProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += AssociatedObjectPreviewTextInput;
            AssociatedObject.PreviewKeyDown += AssociatedObjectPreviewKeyDown;
            AssociatedObject.TextChanged += AssociatedObjectTextChanged;

            DataObject.AddPastingHandler(AssociatedObject, Pasting);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= AssociatedObjectPreviewTextInput;
            AssociatedObject.PreviewKeyDown -= AssociatedObjectPreviewKeyDown;
            AssociatedObject.TextChanged -= AssociatedObjectTextChanged;

            DataObject.RemovePastingHandler(AssociatedObject, Pasting);
        }

        void AssociatedObjectPreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (!IsValidInput(GetText(" ")))
                {
                    SystemSounds.Beep.Play();
                    e.Handled = true;
                }
            }
        }

        void AssociatedObjectPreviewTextInput(object? sender, TextCompositionEventArgs e)
        {
            if (!IsValidInput(GetText(e.Text)))
            {
                SystemSounds.Beep.Play();
                e.Handled = true;
            }
        }

        void AssociatedObjectTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(AssociatedObject.Text))
            {
                AssociatedObject.Text = Empty;
            }
        }

        string GetText(string input)
        {
            var txt = AssociatedObject;

            var selectionStart = txt.SelectionStart;
            if (txt.Text.Length < selectionStart)
            {
                selectionStart = txt.Text.Length;
            }

            var selectionLength = txt.SelectionLength;
            if (txt.Text.Length < (selectionStart + selectionLength))
            {
                selectionLength = txt.Text.Length - selectionStart;
            }

            var realtext = txt.Text.Remove(selectionStart, selectionLength);

            var caretIndex = txt.CaretIndex;
            if (realtext.Length < caretIndex)
            {
                caretIndex = realtext.Length;
            }

            var newtext = realtext.Insert(caretIndex, input);

            return newtext;
        }

        [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "Switch on property is valid here")]
        bool IsValidInput(string input)
        {
            switch (InputMode)
            {
                case TextBoxInputMode.None:
                    return true;
                case TextBoxInputMode.IntInput:
                    return int.TryParse(input, OnlyPositive ? IntNumberStyles : IntNumberStyles | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out _);
                case TextBoxInputMode.DecimalInput:
                    var result = decimal.TryParse(input, OnlyPositive ? DecimalNumberStyles : DecimalNumberStyles | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out _);
                    return result;
                default:
                    throw new ArgumentOutOfRangeException(nameof(InputMode));
            }
        }

        void Pasting(object? sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var pastedText = (string)e.DataObject.GetData(typeof(string));
                if (pastedText == null)
                {
                    return;
                }

                if (!IsValidInput(GetText(pastedText)))
                {
                    SystemSounds.Beep.Play();
                    e.CancelCommand();
                }
            }
            else
            {
                SystemSounds.Beep.Play();
                e.CancelCommand();
            }
        }
    }
}
