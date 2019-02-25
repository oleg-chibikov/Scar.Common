using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using JetBrains.Annotations;

namespace Scar.Common.WPF.View
{
    public abstract class AnimatedWindow : BaseWindow
    {
        public static readonly DependencyProperty DraggableProperty = DependencyProperty.Register(nameof(Draggable), typeof(bool), typeof(Window), new PropertyMetadata(null));

        private readonly ConcurrentDictionary<DependencyProperty, double> _animations = new ConcurrentDictionary<DependencyProperty, double>();

        private readonly Duration _fadeDuration = new Duration(TimeSpan.FromMilliseconds(300));

        private readonly Duration _repositionDuration = new Duration(TimeSpan.FromMilliseconds(150));

        static AnimatedWindow()
        {
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,/Scar.Common.WPF.View.AnimatedWindow;component/AnimatedWindowTemplate.xaml", UriKind.Absolute)
            };
            Application.Current.Resources.MergedDictionaries.Insert(0, resourceDictionary);
        }

        protected AnimatedWindow()
        {
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

        public bool CustomShell { get; set; }

        public override bool Draggable
        {
            get => (bool)GetValue(DraggableProperty);
            set => SetValue(DraggableProperty, value);
        }

        public event EventHandler ContentRenderAnimationFinished;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (Draggable && e.ButtonState != MouseButtonState.Released)
            {
                DragMove();
            }

            if (ResizeMode != ResizeMode.CanResize)
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                WindowState = WindowState != WindowState.Maximized ? WindowState.Maximized : WindowState.Normal;
            }
        }

        protected override void Reposition(DependencyProperty prop, double change)
        {
            if (change.Equals(0))
            {
                return;
            }

            var newValue = _animations.AddOrUpdate(prop, p => (double)GetValue(p) + change, (p, val) => val + change);

            var animation = new DoubleAnimation
            {
                To = newValue,
                Duration = _repositionDuration
            };

            BeginAnimation(prop, null);
            BeginAnimation(prop, animation, HandoffBehavior.SnapshotAndReplace);
        }

        private void AnimatedWindow_Closing(object sender, [NotNull] CancelEventArgs e)
        {
            e.Cancel = true;

            // Unsubscribe to prevent recurring call of this method after animation completes
            Closing -= AnimatedWindow_Closing;
            var hideAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = _fadeDuration
            };

            void CompletedHandler(object s, EventArgs _)
            {
                hideAnimation.Completed -= CompletedHandler;
                Close();
            }

            hideAnimation.Completed += CompletedHandler;
            BeginAnimation(OpacityProperty, hideAnimation);
        }

        private void AnimatedWindow_ContentRendered(object sender, EventArgs e)
        {
            var showAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = _fadeDuration
            };

            void CompletedHandler(object s, EventArgs _)
            {
                showAnimation.Completed -= CompletedHandler;
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

        private void AnimatedWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CustomShell)
            {
                Template = (ControlTemplate)FindResource("AnimatedWindowTemplate");
            }
        }

        private void CheckBounds()
        {
            // Top
            if (Top < ActiveScreenArea.Y)
            {
                Top = ActiveScreenArea.Y;
            }

            // Left
            if (Left < ActiveScreenArea.X)
            {
                Left = ActiveScreenArea.X;
            }

            // Bottom
            var windowBottom = Top + Height;
            var screenBottom = ActiveScreenArea.Y + ActiveScreenArea.Height;
            if (Height <= ActiveScreenArea.Height && windowBottom > screenBottom)
            {
                Top -= windowBottom - screenBottom;
            }

            // Right
            var windowRight = Left + Width;
            var screenRight = ActiveScreenArea.X + ActiveScreenArea.Width;
            if (Width <= ActiveScreenArea.Width && windowRight > screenRight)
            {
                Left -= windowRight - screenRight;
            }
        }
    }
}