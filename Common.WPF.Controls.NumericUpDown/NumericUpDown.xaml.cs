using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.Annotations;

namespace Scar.Common.WPF.Controls
{
    public sealed partial class NumericUpDown
    {
        private const int MouseStep = 10;

        /// <summary>
        /// Dependency Object for the value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceValue));

        /// <summary>
        /// Dependency Object for the Minimal Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue),
            typeof(int),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata());

        /// <summary>
        /// Dependency Object for the Maximal Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue),
            typeof(int),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(int.MaxValue));

        /// <summary>
        /// Dependency Object for the Step Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(Step), typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(1));

        private Point _start;

        /// <summary>
        /// Default Constructor (nothing special here x) )
        /// </summary>
        public NumericUpDown()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets / Sets the value that the control is showing
        /// </summary>
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets / Sets the minimal value of the control's value
        /// </summary>
        public int MinValue
        {
            private get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Gets / Sets the maximal value of the control's value
        /// </summary>
        public int MaxValue
        {
            private get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        /// <summary>
        /// Gets / Sets the step size (increment / decrement size) of the control's value
        /// </summary>
        private int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        /// <summary>
        /// Handler for the Down Button Click.
        /// Decrements the <see cref="Value" /> by <see cref="Step" />
        /// </summary>
        /// <param name="sender">The Down Button Control</param>
        /// <param name="e"></param>
        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            Decrement();
        }

        /// <summary>
        /// Handler for the Up Button Click.
        /// Increments the <see cref="Value" /> by <see cref="Step" />
        /// </summary>
        /// <param name="sender">The Up Button Control</param>
        /// <param name="e"></param>
        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            Increment();
        }

        [NotNull]
        private static object CoerceValue([NotNull] DependencyObject d, [NotNull] object value)
        {
            var max = (int)d.GetValue(MaxValueProperty);
            var min = (int)d.GetValue(MinValueProperty);
            var dVal = (int)value;

            return dVal > max ? max : dVal < min ? min : value;
        }

        /// <summary>
        /// Decrements the control's value by the value defined by <see cref="Step" />
        /// </summary>
        /// <remarks>The value doesn't increment over MaxValue or under MinValue</remarks>
        private void Decrement()
        {
            var newVal = Value - Step;
            if (newVal >= MinValue)
            {
                Value = newVal;
            }
        }

        /// <summary>
        /// Increments the control's value by the value defined by <see cref="Step" />
        /// </summary>
        /// <remarks>The value doesn't increment over MaxValue or under MinValue</remarks>
        private void Increment()
        {
            var newVal = Value + Step;
            if (newVal <= MaxValue)
            {
                Value = newVal;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, [NotNull] KeyEventArgs e)
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

        private void TextBox_PreviewMouseDown(object sender, [NotNull] MouseButtonEventArgs e)
        {
            _start = e.GetPosition(this);
            var el = (TextBox)sender;
            el.CaptureMouse();
            Cursor = Cursors.SizeNS;
            el.IsTabStop = false;
            el.IsReadOnly = true;
        }

        private void TextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var el = (TextBox)sender;
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
                //TODO: Disable textbox selection when increment/decremment occured after mousedown and until mouseup
            }
            else if (diffY < -MouseStep)
            {
                _start = currentPosition;
                Decrement();
            }
        }

        private void TextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var el = (TextBox)sender;
            el.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            el.IsTabStop = true;
            el.IsReadOnly = false;
        }
    }
}