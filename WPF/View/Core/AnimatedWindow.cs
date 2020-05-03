using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using Scar.Common.WPF.Screen;

namespace Scar.Common.WPF.View.Core
{
    public abstract class AnimatedWindow : BaseWindow
    {
        public static readonly DependencyProperty DraggableProperty = DependencyProperty.Register(nameof(Draggable), typeof(bool), typeof(Window), new PropertyMetadata(null));
        readonly ConcurrentDictionary<DependencyProperty, double> _animations = new ConcurrentDictionary<DependencyProperty, double>();
        readonly Duration _fadeDuration = new Duration(TimeSpan.FromMilliseconds(300));
        readonly Duration _repositionDuration = new Duration(TimeSpan.FromMilliseconds(150));

        static AnimatedWindow()
        {
            var resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,/Scar.Common.WPF.View.Core;component/AnimatedWindowTemplate.xaml", UriKind.Absolute) };
            Application.Current.Resources.MergedDictionaries.Insert(0, resourceDictionary);
        }

        protected AnimatedWindow()
        {
            WindowChrome.SetWindowChrome(this, new WindowChrome { ResizeBorderThickness = new Thickness(5), CaptionHeight = 0 });
            Opacity = 0;

            // ReSharper disable once VirtualMemberCallInConstructor
            Draggable = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AllowsTransparency = true;
            Padding = new Thickness(6);
            Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
            Foreground = Brushes.White;
            BorderBrush = Brushes.Black;
            BorderThickness = new Thickness(2);
            Closing += AnimatedWindow_Closing;
            Loaded += AnimatedWindow_Loaded;
            ContentRendered += AnimatedWindow_ContentRendered;
            StateChanged += (s, e) => CheckBounds();

            // Prevent coverage of Taskbar when maximized
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        public event EventHandler? ContentRenderAnimationFinished;

        public override bool Draggable
        {
            get => (bool)GetValue(DraggableProperty);
            set => SetValue(DraggableProperty, value);
        }

        public bool CustomShell { get; set; }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _ = e ?? throw new ArgumentNullException(nameof(e));

            base.OnMouseLeftButtonDown(e);

            if (Draggable && (e.ButtonState != MouseButtonState.Released))
            {
                DragMove();
            }
        }

        protected override void Reposition(DependencyProperty prop, double change)
        {
            if (change.Equals(0))
            {
                return;
            }

            var newValue = _animations.AddOrUpdate(prop, p => (double)GetValue(p) + change, (p, val) => val + change);

            var animation = new DoubleAnimation { To = newValue, Duration = _repositionDuration };

            BeginAnimation(prop, null);
            BeginAnimation(prop, animation, HandoffBehavior.SnapshotAndReplace);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "SA1313:Parameter '_' should begin with lower -case letter", Justification = "Discarded parameter")]
        void AnimatedWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            // Unsubscribe to prevent recurring call of this method after animation completes
            Closing -= AnimatedWindow_Closing;
            var hideAnimation = new DoubleAnimation { From = 1, To = 0, Duration = _fadeDuration };

            void CompletedHandler(object? s, EventArgs _)
            {
                hideAnimation!.Completed -= CompletedHandler;
                Close();
            }

            hideAnimation.Completed += CompletedHandler;
            BeginAnimation(OpacityProperty, hideAnimation);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "SA1313:Parameter '_' should begin with lower -case letter", Justification = "Discarded parameter")]
        void AnimatedWindow_ContentRendered(object? sender, EventArgs e)
        {
            var showAnimation = new DoubleAnimation { From = 0, To = 1, Duration = _fadeDuration };

            void CompletedHandler(object? s, EventArgs _)
            {
                showAnimation!.Completed -= CompletedHandler;
                BeginAnimation(TopProperty, null);
                BeginAnimation(LeftProperty, null);
                if (Focusable && ShowActivated)
                {
                    Restore();
                }

                ContentRenderAnimationFinished?.Invoke(this, _);
            }

            showAnimation.Completed += CompletedHandler;
            BeginAnimation(OpacityProperty, showAnimation);
        }

        void HeaderPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((ResizeMode != ResizeMode.CanResize) && (ResizeMode != ResizeMode.CanResizeWithGrip))
            {
                return;
            }

            if ((e.ClickCount == 2) && (e.GetPosition(this).Y < 20))
            {
                WindowState = WindowState != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
            }
        }

        void AnimatedWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CustomShell)
            {
                Template = (ControlTemplate)FindResource("AnimatedWindowTemplate");
                ApplyTemplate();

                var headerPanel = (DockPanel)Template.FindName("HeaderPanel", this);
                headerPanel.PreviewMouseDown += HeaderPanel_MouseDown;
            }
        }

        void CheckBounds()
        {
            var screen = WPFScreen.GetScreenFrom(this);
            var workingArea = screen.WorkingArea;

            // Top
            if (Top < workingArea.Y)
            {
                Top = workingArea.Y;
            }

            // Left
            if (Left < workingArea.X)
            {
                Left = workingArea.X;
            }

            // Bottom
            var windowBottom = Top + Height;
            var screenBottom = workingArea.Y + workingArea.Height;
            if ((Height <= workingArea.Height) && (windowBottom > screenBottom))
            {
                Top -= windowBottom - screenBottom;
            }

            // Right
            var windowRight = Left + Width;
            var screenRight = workingArea.X + workingArea.Width;
            if ((Width <= workingArea.Width) && (windowRight > screenRight))
            {
                Left -= windowRight - screenRight;
            }
        }
    }
}
