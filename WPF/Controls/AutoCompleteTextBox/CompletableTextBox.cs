using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Scar.Common.RateLimiting;

namespace Scar.Common.WPF.Controls
{
    public class CompletableTextBox : TextBox, IDisposable
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(CompletableTextBox),
            new PropertyMetadata(null, SelectedItemPropertyChanged));

        public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register(
            nameof(DataProvider),
            typeof(IAutoCompleteDataProvider),
            typeof(CompletableTextBox),
            new PropertyMetadata(null));

        readonly IRateLimiter _rateLimiter;
        bool _disposedValue;
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        bool _disabled;
        ListBox _listBox;
        Popup _popup;
        bool _suppressAutoAppend;
        /*+---------------------------------------------------------------------+
          |                                                                     |
          |                  Internal States                                    |
          |                                                                     |
          +---------------------------------------------------------------------*/
        bool _textChangedByCode;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Fields are initialized in the Loaded event and after that remain non-nullable
        public CompletableTextBox()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
            _rateLimiter = new RateLimiter(SynchronizationContext.Current);
            Loaded += CompletableTextBox_Initialized;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool AutoAppend { get; set; }

        public bool AutoCompleting => _popup.IsOpen;

        /*+---------------------------------------------------------------------+
          |                                                                     |
          |                     Public interface                                |
          |                                                                     |
          +---------------------------------------------------------------------*/
        public IAutoCompleteDataProvider DataProvider
        {
            get => (IAutoCompleteDataProvider)GetValue(DataProviderProperty);
            set => SetValue(DataProviderProperty, value);
        }

        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                if (_disabled && (_popup != null))
                {
                    _popup.IsOpen = false;
                }
            }
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
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
                    _cancellationTokenSource.Dispose();
                }

                _disposedValue = true;
            }
        }

        static void SelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CompletableTextBox)d;
            control?.ChangeSelectedItem(e.NewValue);
        }

        void ChangeSelectedItem(object? item)
        {
            Text = item?.ToString() ?? string.Empty;
        }

        void CompletableTextBox_Initialized(object sender, EventArgs e)
        {
            if (Application.Current.Resources.FindName("AutoCompleteTextBoxListBoxStyle") == null)
            {
                var myResourceDictionary = new ResourceDictionary();
                var uri = new Uri("pack://application:,,,/Scar.Common.WPF.Controls.AutoCompleteTextBox;component/resources.xaml", UriKind.RelativeOrAbsolute);
                myResourceDictionary.Source = uri;
                Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
            }

            var ownerWindow = Window.GetWindow(this);
            if (ownerWindow == null)
            {
                throw new InvalidOperationException("Window should not be null");
            }

            if (ownerWindow.IsLoaded)
            {
                Initialize();
            }
            else
            {
                ownerWindow.Loaded += OwnerWindow_Loaded;
            }

            ownerWindow.LocationChanged += OwnerWindow_LocationChanged;
        }

        // ReSharper disable once RedundantAssignment
        IntPtr HookHandler(IntPtr hwnd, int msg, IntPtr firstParam, IntPtr secondParam, ref bool handled)
        {
            const int wmNclbuttondown = 0x00A1;

            const int wmNcrbuttondown = 0x00A4;

            handled = false;

            switch (msg)
            {
                case wmNclbuttondown: // pass through
                case wmNcrbuttondown:
                    _popup.IsOpen = false;
                    break;
            }

            return IntPtr.Zero;
        }

        void Initialize()
        {
            _listBox = new ListBox { Focusable = false, Style = (Style)Application.Current.Resources["AutoCompleteTextBoxListBoxStyle"] };

            _popup = new Popup
            {
                SnapsToDevicePixels = true,
                AllowsTransparency = true,
                Placement = PlacementMode.Bottom,
                PlacementTarget = this,
                Child = _listBox,
                Width = Width,
                IsOpen = true
            };

            SetupEventHandlers();
            var tempItem = new ListBoxItem { Content = "TEMP_ITEM_FOR_MEASUREMENT" };
            _listBox.Items.Add(tempItem);
            _listBox.Items.Clear();
            _popup.IsOpen = false;
        }

        void ListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem? item = null;
            var d = e.OriginalSource as DependencyObject;
            while (d != null)
            {
                if (d is ListBoxItem boxItem)
                {
                    item = boxItem;
                    break;
                }

                d = VisualTreeHelper.GetParent(d);
            }

            if (item != null)
            {
                _popup.IsOpen = false;

                // UpdateText(item.Content as string, true);
                UpdateItem(item.Content, true);
            }
        }

        /*+---------------------------------------------------------------------+
          |                                                                     |
          |                     ListBox Event Handling                          |
          |                                                                     |
          +---------------------------------------------------------------------*/
        void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(_listBox);
            var hitTestResult = VisualTreeHelper.HitTest(_listBox, pos);
            if (hitTestResult == null)
            {
                return;
            }

            var d = hitTestResult.VisualHit;
            while (d != null)
            {
                if (d is ListBoxItem)
                {
                    e.Handled = true;
                    break;
                }

                d = VisualTreeHelper.GetParent(d);
            }
        }

        void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured != null)
            {
                return;
            }

            var pos = e.GetPosition(_listBox);
            var hitTestResult = VisualTreeHelper.HitTest(_listBox, pos);
            if (hitTestResult == null)
            {
                return;
            }

            var d = hitTestResult.VisualHit;
            while (d != null)
            {
                if (d is ListBoxItem item)
                {
                    item.IsSelected = true;

                    // _listBox.ScrollIntoView(item);
                    break;
                }

                d = VisualTreeHelper.GetParent(d);
            }
        }

        /*+---------------------------------------------------------------------+
          |                                                                     |
          |                    Window Event Handling                            |
          |                                                                     |
          +---------------------------------------------------------------------*/
        void OwnerWindow_Deactivated(object? sender, EventArgs e)
        {
            _popup.IsOpen = false;
        }

        void OwnerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        /*+---------------------------------------------------------------------+
          |                                                                     |
          |                       Initializer                                    |
          |                                                                     |
          +---------------------------------------------------------------------*/
        void OwnerWindow_LocationChanged(object? sender, EventArgs e)
        {
            _popup.IsOpen = false;
        }

        void OwnerWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Equals(e.Source, this))
            {
                _popup.IsOpen = false;
            }
        }

        /*+---------------------------------------------------------------------+
          |                                                                     |
          |                    AcTb State And Behaviors                         |
          |                                                                     |
          +---------------------------------------------------------------------*/
        void PopulatePopupList(IEnumerable<object> items)
        {
            var text = Text;

            _listBox.ItemsSource = items;
            if (_listBox.Items.Count == 0)
            {
                _popup.IsOpen = false;
                return;
            }

            var firstSuggestion = _listBox.Items[0];
            if ((_listBox.Items.Count == 1) && text.Equals(firstSuggestion.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _popup.IsOpen = false;
                SelectedItem = firstSuggestion;
            }
            else
            {
                _listBox.SelectedIndex = 0;
                ShowPopup();

                if (!AutoAppend || _suppressAutoAppend || (SelectionLength != 0) || (SelectionStart != Text.Length))
                {
                    return;
                }

                _textChangedByCode = true;
                try
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    var firstSuggestionString = firstSuggestion.ToString() ?? throw new InvalidOperationException("firstSuggestion.ToString() is null");
                    var appendText = DataProvider is IAutoAppendDataProvider appendProvider ? appendProvider.GetAppendText(text, firstSuggestionString) : firstSuggestionString.Substring(Text.Length);
                    if (!string.IsNullOrEmpty(appendText))
                    {
                        SelectedText = appendText;
                    }
                }
                finally
                {
                    _textChangedByCode = false;
                }
            }
        }

        void SetupEventHandlers()
        {
            var ownerWindow = Window.GetWindow(this);
            if (ownerWindow == null)
            {
                throw new InvalidOperationException("Window should not be null");
            }

            ownerWindow.PreviewMouseDown += OwnerWindow_PreviewMouseDown;
            ownerWindow.Deactivated += OwnerWindow_Deactivated;

            var wih = new WindowInteropHelper(ownerWindow);
            var hwndSource = HwndSource.FromHwnd(wih.Handle);
            if (hwndSource == null)
            {
                throw new InvalidOperationException("Window handle should not be null");
            }

            var hwndSourceHook = new HwndSourceHook(HookHandler);
            hwndSource.AddHook(hwndSourceHook);

            // hwndSource.RemoveHook();?
            TextChanged += TextBox_TextChanged;
            PreviewKeyDown += TextBox_PreviewKeyDown;
            LostFocus += TextBox_LostFocus;

            _listBox.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            _listBox.MouseLeftButtonUp += ListBox_MouseLeftButtonUp;
            _listBox.PreviewMouseMove += ListBox_PreviewMouseMove;
        }

        void ShowPopup()
        {
            _popup.IsOpen = true;
        }

        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _popup.IsOpen = false;
            _cancellationTokenSource.Cancel();

            // Text = SelectedItem?.ToString() ?? string.Empty;
        }

        void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _rateLimiter.ThrottleAsync(
                TimeSpan.FromMilliseconds(20),
                () =>
                {
                    _cancellationTokenSource.Cancel();
                    _suppressAutoAppend = (e.Key == Key.Delete) || (e.Key == Key.Back);

                    if (!_popup.IsOpen)
                    {
                        return;
                    }

                    var index = _listBox.SelectedIndex;

                    if (e.Key == Key.Escape)
                    {
                        _popup.IsOpen = false;
                        e.Handled = true;

                        return;
                    }

                    if (e.Key == Key.Up)
                    {
                        if (index == -1)
                        {
                            index = _listBox.Items.Count - 1;
                        }
                        else
                        {
                            --index;
                        }
                    }
                    else if (e.Key == Key.Down)
                    {
                        ++index;
                    }

                    if (e.Key == Key.Enter)
                    {
                        _popup.IsOpen = false;
                        SelectAll();

                        UpdateItem(_listBox.SelectedItem, true);
                    }

                    if (index != _listBox.SelectedIndex)
                    {
                        if ((index < 0) || (index > (_listBox.Items.Count - 1)))
                        {
                            _listBox.SelectedIndex = -1;
                        }
                        else
                        {
                            _listBox.SelectedIndex = index;
                            _listBox.ScrollIntoView(_listBox.SelectedItem);
                        }

                        e.Handled = true;
                    }
                });
        }

        /*+---------------------------------------------------------------------+
          |                                                                     |
          |                   TextBox Event Handling                            |
          |                                                                     |
          +---------------------------------------------------------------------*/
        void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _rateLimiter.DebounceAsync(
                TimeSpan.FromMilliseconds(300),
                async text =>
                {
                    if (_textChangedByCode || Disabled || (DataProvider == null))
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(text))
                    {
                        _popup.IsOpen = false;
                        return;
                    }

                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    try
                    {
                        var items = await DataProvider.GetItemsAsync(text, _cancellationTokenSource.Token).ConfigureAwait(true);
                        PopulatePopupList(items);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                },
                Text);
        }

        void UpdateItem(object item, bool selectAll)
        {
            _textChangedByCode = true;
            SelectedItem = item;
            Text = item?.ToString() ?? string.Empty;

            if (selectAll)
            {
                SelectAll();
            }
            else
            {
                SelectionStart = Text.Length;
            }

            _textChangedByCode = false;
        }
    }
}
