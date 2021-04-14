using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Scar.Common.WPF.Controls
{
    public sealed partial class NumericUpDown
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceValue));

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(nameof(MinValue), typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata());

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(int.MaxValue));

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(Step), typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(1));

        const int MouseStep = 10;

        Point _start;

        public NumericUpDown()
        {
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "Good name")]
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        static object CoerceValue(DependencyObject d, object value)
        {
            var max = (int)d.GetValue(MaxValueProperty);
            var min = (int)d.GetValue(MinValueProperty);
            var dVal = (int)value;

            return dVal > max ? max : dVal < min ? min : value;
        }

        void BtnDown_Click(object? sender, RoutedEventArgs e)
        {
            Decrement();
        }

        void BtnUp_Click(object? sender, RoutedEventArgs e)
        {
            Increment();
        }

        /// <remarks>The value doesn't increment over MaxValue or under MinValue.</remarks>
        void Decrement()
        {
            var newVal = Value - Step;
            if (newVal >= MinValue)
            {
                Value = newVal;
            }
        }

        /// <remarks>The value doesn't increment over MaxValue or under MinValue.</remarks>
        void Increment()
        {
            var newVal = Value + Step;
            if (newVal <= MaxValue)
            {
                Value = newVal;
            }
        }

        void TextBox_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    Increment();
                    break;
                case Key.Down:
                    Decrement();
                    break;
            }
        }

        void TextBox_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
        {
            _start = e.GetPosition(this);
            var el = (TextBox)(sender ?? throw new InvalidOperationException("sender is null"));
            el.CaptureMouse();
            Cursor = Cursors.SizeNS;
            el.IsTabStop = false;
            el.IsReadOnly = true;
        }

        void TextBox_PreviewMouseMove(object? sender, MouseEventArgs e)
        {
            var el = (TextBox)(sender ?? throw new InvalidOperationException("sender is null"));
            if (!el.IsMouseCaptured)
            {
                return;
            }

            var currentPosition = e.GetPosition(this);
            var vector = _start - currentPosition;
            var diffY = vector.Y;
            if (diffY > MouseStep)
            {
                _start = currentPosition;
                Increment();

                // TODO: Disable textbox selection when increment/decremment occured after mousedown and until mouseup
            }
            else if (diffY < -MouseStep)
            {
                _start = currentPosition;
                Decrement();
            }
        }

        void TextBox_PreviewMouseUp(object? sender, MouseButtonEventArgs e)
        {
            var el = (TextBox)(sender ?? throw new InvalidOperationException("sender is null"));
            el.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            el.IsTabStop = true;
            el.IsReadOnly = false;
        }
    }
}
