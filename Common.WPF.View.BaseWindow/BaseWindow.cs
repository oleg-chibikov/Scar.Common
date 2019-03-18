using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using JetBrains.Annotations;
using Scar.Common.View.Contracts;
using Scar.Common.WPF.View.Contracts;
using Scar.Common.WPF.ViewModel;
using Point = System.Windows.Point;

namespace Scar.Common.WPF.View
{
    public abstract class BaseWindow : Window, IWindow
    {
        public static readonly DependencyProperty AutoCloseTimeoutProperty = DependencyProperty.Register(
            nameof(AutoCloseTimeout),
            typeof(TimeSpan?),
            typeof(BaseWindow),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsFullHeightProperty = DependencyProperty.Register(
            nameof(IsFullHeight),
            typeof(bool),
            typeof(BaseWindow),
            new PropertyMetadata(null));

        private readonly IList<IDisposable> _associatedDisposables = new List<IDisposable>();

        [NotNull]
        private readonly IRateLimiter _rateLimiter;

        private readonly TimeSpan _sizeChangedThrottleInterval = TimeSpan.FromMilliseconds(50);

        private double _cumulativeHeightChange;
        private double _cumulativeWidthChange;

        protected BaseWindow()
        {
            this.PreventFocusLoss();
            this.HandleDisposableViewModel();
            IRateLimiter rateLimiter = new RateLimiter(SynchronizationContext.Current);

            void LocationChangedAction(object s, EventArgs e)
            {
                rateLimiter.Throttle(TimeSpan.FromMilliseconds(300), window => ActiveScreenArea = Screen.FromHandle(new WindowInteropHelper(window).Handle).WorkingArea, this);
            }

            // Unsubscribe not needed - same class
            DataContextChanged += BaseWindow_DataContextChanged;
            Closing += BaseWindow_Closing;
            Closed += BaseWindow_Closed;
            SizeChanged += BaseWindow_SizeChanged_IsFullHeight;
            Loaded += BaseWindow_Loaded;

            // KeyDown += BaseWindow_KeyDown;
            ContentRendered += BaseWindow_ContentRendered;
            LocationChanged += LocationChangedAction;
            LocationChangedAction(this, new EventArgs());
            _rateLimiter = new RateLimiter(SynchronizationContext.Current);
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded?.Invoke(sender, e);
        }

        private EventHandler _sizeChanged;
        private EventHandler _loaded;
        private readonly object _sizeChangedLock = new object();
        private readonly object _loadedLock = new object();
        event EventHandler IDisplayable.SizeChanged
        {
            add
            {
                lock (_sizeChangedLock)
                {
                    _sizeChanged += value;
                }
            }

            remove
            {
                lock (_sizeChangedLock)
                {
                    _sizeChanged -= value;
                }
            }
        }

        event EventHandler IDisplayable.Loaded
        {
            add
            {
                lock (_loadedLock)
                {
                    _loaded += value;
                }
            }

            remove
            {
                lock (_loadedLock)
                {
                    _loaded -= value;
                }
            }
        }

        public Rectangle ActiveScreenArea { get; private set; }

        public AdvancedWindowStartupLocation AdvancedWindowStartupLocation { get; set; }

        public TimeSpan? AutoCloseTimeout
        {
            get => (TimeSpan?)GetValue(AutoCloseTimeoutProperty);
            set => SetValue(AutoCloseTimeoutProperty, value);
        }

        public virtual bool Draggable
        {
            get => true;
            set => throw new NotSupportedException();
        }

        public bool IsFullHeight
        {
            get => (bool)GetValue(IsFullHeightProperty);
            set => SetValue(IsFullHeightProperty, value);
        }

        public void AssociateDisposable(IDisposable disposable)
        {
            _associatedDisposables.Add(disposable);
        }

        public void Restore()
        {
            var prevTopmost = Topmost;
            Topmost = true;
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            if (ShowActivated)
            {
                Activate();

                if (Focusable)
                {
                    Focus();
                }
            }

            Topmost = prevTopmost;
        }

        public bool UnassociateDisposable(IDisposable disposable)
        {
            return _associatedDisposables.Remove(disposable);
        }

        protected virtual bool CheckCloseShouldBeCancelled()
        {
            return false;
        }

        protected void RepositionWindowAtStartup()
        {
            if (WindowStartupLocation != WindowStartupLocation.Manual)
            {
                return;
            }

            switch (AdvancedWindowStartupLocation)
            {
                case AdvancedWindowStartupLocation.Default:
                    break;
                case AdvancedWindowStartupLocation.TopLeft:
                    {
                        var screenArea = SystemParameters.WorkArea;
                        Left = screenArea.Left;
                        Top = screenArea.Top;
                        break;
                    }

                case AdvancedWindowStartupLocation.TopRight:
                    {
                        var screenArea = SystemParameters.WorkArea;
                        Left = screenArea.Right - Width;
                        Top = screenArea.Top;
                        break;
                    }

                case AdvancedWindowStartupLocation.BottomLeft:
                    {
                        var screenArea = SystemParameters.WorkArea;
                        Left = screenArea.Left;
                        Top = screenArea.Bottom - Height;
                        break;
                    }

                case AdvancedWindowStartupLocation.BottomRight:
                    {
                        var screenArea = SystemParameters.WorkArea;
                        Left = screenArea.Right - Width;
                        Top = screenArea.Bottom - Height;
                        break;
                    }

                case AdvancedWindowStartupLocation.MouseCursor:
                    {
                        MoveBottomRightEdgeOfWindowToMousePosition();
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Point GetMousePosition()
        {
            var point = Control.MousePosition;
            return new Point(point.X, point.Y);
        }

        private void BaseWindow_Closed(object sender, EventArgs e)
        {
            foreach (var disposable in _associatedDisposables)
            {
                disposable.Dispose();
            }
        }

        private void BaseWindow_Closing([NotNull] object sender, [NotNull] CancelEventArgs e)
        {
            if (CheckCloseShouldBeCancelled())
            {
                e.Cancel = true;
                return;
            }

            Owner?.Activate();
        }

        private async void BaseWindow_ContentRendered(object sender, EventArgs e)
        {
            RepositionWindowAtStartup();
            if (AdvancedWindowStartupLocation != AdvancedWindowStartupLocation.Default && AdvancedWindowStartupLocation != AdvancedWindowStartupLocation.MouseCursor)
            {
                SizeChanged += BaseWindow_SizeChanged_Reposition;
            }

            var timeout = AutoCloseTimeout;
            if (timeout != null)
            {
                // retaining context here by ConfigureAwait true
                await Task.Delay(timeout.Value).ConfigureAwait(true);
                Close();
            }
        }

        protected virtual void Reposition([NotNull] DependencyProperty prop, double change)
        {
            if (change.Equals(0))
            {
                return;
            }

            var prevValue = (double)GetValue(prop);
            var newValue = prevValue + change;
            SetValue(prop, newValue);
        }

        private void BaseWindow_SizeChanged_Reposition(object sender, [NotNull] SizeChangedEventArgs e)
        {
            if (e.PreviousSize.IsEmpty)
            {
                return;
            }

            var thisWidthChange = e.WidthChanged ? e.PreviousSize.Width - e.NewSize.Width : 0;
            var thisHeightChange = e.HeightChanged ? e.PreviousSize.Height - e.NewSize.Height : 0;
            _cumulativeHeightChange += thisHeightChange;
            _cumulativeWidthChange += thisWidthChange;

            _rateLimiter.Throttle(
                _sizeChangedThrottleInterval,
                () =>
                {
                    var heightDiff = _cumulativeHeightChange;
                    var widthDiff = _cumulativeWidthChange;
                    _cumulativeHeightChange = 0;
                    _cumulativeWidthChange = 0;

                    switch (AdvancedWindowStartupLocation)
                    {
                        case AdvancedWindowStartupLocation.BottomRight:
                            Reposition(TopProperty, heightDiff);
                            Reposition(LeftProperty, widthDiff);
                            break;
                        case AdvancedWindowStartupLocation.BottomLeft:
                            Reposition(TopProperty, heightDiff);
                            break;
                        case AdvancedWindowStartupLocation.TopRight:
                            Reposition(LeftProperty, widthDiff);
                            break;
                    }
                },
                //skipping immediate action as it makes little sense due to the small-increment nature of sizeChanged
                true);
        }

        /// <summary>
        /// When the data context of the window is changed (for example, manually, or when it is disposed),
        /// then if DataContext implements IRequestCloseViewModel the window should be closed.
        /// </summary>
        private void BaseWindow_DataContextChanged([NotNull] object sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(args.NewValue is IRequestCloseViewModel requestCloseViewModel))
            {
                return;
            }

            void CloseHandler(object s, EventArgs e)
            {
                requestCloseViewModel.RequestClose -= CloseHandler;
                Close();
            }

            requestCloseViewModel.RequestClose += CloseHandler;
        }

        /*
        private void BaseWindow_KeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }*/

        private void BaseWindow_SizeChanged_IsFullHeight([NotNull] object sender, [NotNull] SizeChangedEventArgs e)
        {
            _sizeChanged?.Invoke(sender, e);
            if (!e.HeightChanged)
            {
                return;
            }

            var fullHeight = ActiveScreenArea.Height;
            var newHeight = e.NewSize.Height;
            IsFullHeight = Math.Abs(newHeight - fullHeight) < 50;
        }

        private void MoveBottomRightEdgeOfWindowToMousePosition()
        {
            var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice;
            if (transform == null)
            {
                return;
            }

            var mouse = transform.Value.Transform(GetMousePosition());
            Left = mouse.X - ActualWidth / 2;
            Top = mouse.Y - ActualHeight / 2;
        }
    }
}