using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Scar.Common.MVVM.ViewModel;
using Scar.Common.RateLimiting;
using Scar.Common.View.Contracts;
using Scar.Common.WPF.View.Contracts;

namespace Scar.Common.WPF.View.Core;

public abstract class BaseWindow : Window, IWindow, IDisposable
{
    public static readonly DependencyProperty AutoCloseTimeoutProperty = DependencyProperty.Register(nameof(AutoCloseTimeout), typeof(TimeSpan?), typeof(BaseWindow), new PropertyMetadata(null));
    readonly IList<IDisposable> _associatedDisposables = new List<IDisposable>();
    readonly RateLimiter _sizeChangedRateLimiter;
    readonly TimeSpan _sizeChangedThrottleInterval = TimeSpan.FromMilliseconds(50);
    readonly object _loadedLock = new ();
    double _cumulativeHeightChange;
    double _cumulativeWidthChange;
    EventHandler? _loaded;
    bool _disposedValue;

    protected BaseWindow()
    {
        this.PreventFocusLoss();
        HandleDisposableViewModel();

        // Unsubscribe not needed - same class
        DataContextChanged += BaseWindow_DataContextChanged;
        Closing += BaseWindow_Closing;
        Closed += BaseWindow_Closed;
        Loaded += BaseWindow_Loaded;

        // KeyDown += BaseWindow_KeyDown;
        ContentRendered += BaseWindow_ContentRenderedAsync;
        _sizeChangedRateLimiter = new RateLimiter(SynchronizationContext.Current);
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
                // ReSharper disable once DelegateSubtraction
                _loaded -= value;
            }
        }
    }

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (var disposable in _associatedDisposables)
                {
                    disposable.Dispose();
                }

                _sizeChangedRateLimiter.Dispose();
            }

            _disposedValue = true;
        }
    }

    protected virtual bool CheckCloseShouldBeCancelled()
    {
        return false;
    }

    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "Instance is ok to use here")]
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

            default:
                throw new ArgumentOutOfRangeException(nameof(AdvancedWindowStartupLocation));
        }
    }

    protected virtual void Reposition(DependencyProperty prop, double change)
    {
        if (change.Equals(0))
        {
            return;
        }

        var prevValue = (double)GetValue(prop);
        var newValue = prevValue + change;
        SetValue(prop, newValue);
    }

    void BaseWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        _loaded?.Invoke(sender, e);
    }

    void BaseWindow_Closed(object? sender, EventArgs e)
    {
        Dispose();
    }

    void BaseWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (CheckCloseShouldBeCancelled())
        {
            e.Cancel = true;
            return;
        }

        Owner?.Activate();
    }

    async void BaseWindow_ContentRenderedAsync(object? sender, EventArgs e)
    {
        RepositionWindowAtStartup();
        if (AdvancedWindowStartupLocation != AdvancedWindowStartupLocation.Default)
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

    void BaseWindow_SizeChanged_Reposition(object? sender, SizeChangedEventArgs e)
    {
        if (e.PreviousSize.IsEmpty)
        {
            return;
        }

        var thisWidthChange = e.WidthChanged ? e.PreviousSize.Width - e.NewSize.Width : 0;
        var thisHeightChange = e.HeightChanged ? e.PreviousSize.Height - e.NewSize.Height : 0;
        _cumulativeHeightChange += thisHeightChange;
        _cumulativeWidthChange += thisWidthChange;

        _sizeChangedRateLimiter.ThrottleAsync(
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
            skipImmediate: true).Wait(); // skipping immediate action as it makes little sense due to the small-increment nature of sizeChanged
    }

    /// <summary>
    /// When the data context of the window is changed (for example, manually, or when it is disposed),
    /// then if DataContext implements IRequestCloseViewModel the window should be closed.
    /// </summary>
    void BaseWindow_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs args)
    {
        if (args.NewValue is not IRequestCloseViewModel requestCloseViewModel)
        {
            return;
        }

        requestCloseViewModel.RequestClose += CloseHandler;
        return;

        void CloseHandler(object? s, EventArgs e)
        {
            requestCloseViewModel.RequestClose -= CloseHandler;
            Close();
        }
    }

    void HandleDisposableViewModel()
    {
        RoutedEventHandler? unloadedHandler = null;
        EventHandler? shutdownStartedHandler = null;

        unloadedHandler = (_, _) => Dispose();
        shutdownStartedHandler = (_, _) => Dispose();
        Unloaded += unloadedHandler;
        Dispatcher.ShutdownStarted += shutdownStartedHandler;
        return;

        void Dispose()
        {
            var dataContext = DataContext as IDisposable;
            dataContext?.Dispose();

            // ReSharper disable once AccessToModifiedClosure
            Unloaded -= unloadedHandler;

            // ReSharper disable once AccessToModifiedClosure
            Dispatcher.ShutdownStarted -= shutdownStartedHandler;
        }
    }
}