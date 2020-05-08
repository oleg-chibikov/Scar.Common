using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using Scar.Common.WPF.Screen;

namespace Scar.Common.WPF.View.Core
{
    public abstract class AnimatedWindow : AnimatedWindowWithoutTemplate
    {
        readonly ConcurrentDictionary<DependencyProperty, double> _animations = new ConcurrentDictionary<DependencyProperty, double>();
        readonly Duration _repositionDuration = new Duration(TimeSpan.FromMilliseconds(150));

        static AnimatedWindow()
        {
            var resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,/Scar.Common.WPF.View.Core;component/AnimatedWindowTemplate.xaml", UriKind.Absolute) };
            Application.Current.Resources.MergedDictionaries.Insert(0, resourceDictionary);
        }

        protected AnimatedWindow()
        {
            WindowChrome.SetWindowChrome(this, new WindowChrome { ResizeBorderThickness = new Thickness(5), CaptionHeight = 0 });
            Padding = new Thickness(6);
            Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
            Foreground = Brushes.White;
            BorderBrush = Brushes.Black;
            BorderThickness = new Thickness(2);
            Loaded += AnimatedWindow_Loaded;
            StateChanged += (s, e) => CheckBounds();

            // Prevent coverage of Taskbar when maximized
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        public bool CustomShell { get; set; }

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
