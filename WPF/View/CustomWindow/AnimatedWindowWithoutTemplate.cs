using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Scar.Common.WPF.View.Core;

namespace Scar.Common.WPF.View.CustomWindow
{
    public abstract class AnimatedWindowWithoutTemplate : BaseWindow
    {
        public static readonly DependencyProperty InitialOpacityProperty = DependencyProperty.Register(nameof(InitialOpacity), typeof(double), typeof(Window), new PropertyMetadata(1D));
        public static readonly DependencyProperty DraggableProperty = DependencyProperty.Register(nameof(Draggable), typeof(bool), typeof(Window), new PropertyMetadata(null));
        public static readonly DependencyProperty StayCenteredProperty = DependencyProperty.Register(nameof(StayCentered), typeof(bool), typeof(Window), new PropertyMetadata(null));
        readonly Duration _fadeDuration = new (TimeSpan.FromMilliseconds(300));
        bool _closing;

        protected AnimatedWindowWithoutTemplate()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Draggable = true;
            Opacity = 0;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AllowsTransparency = true;
            Closing += AnimatedWindowWithoutTemplate_Closing;
            ContentRendered += AnimatedWindowWithoutTemplate_ContentRendered;
        }

        public event EventHandler? ContentRenderAnimationFinished;

        public override bool Draggable
        {
            get => (bool)GetValue(DraggableProperty);
            set => SetValue(DraggableProperty, value);
        }

        public bool StayCentered
        {
            get => (bool)GetValue(StayCenteredProperty);
            set => SetValue(StayCenteredProperty, value);
        }

        public double InitialOpacity
        {
            get => (double)GetValue(InitialOpacityProperty);
            set => SetValue(InitialOpacityProperty, value);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            _ = sizeInfo ?? throw new ArgumentNullException(nameof(sizeInfo));

            base.OnRenderSizeChanged(sizeInfo);

            if (!StayCentered || WindowStartupLocation != WindowStartupLocation.CenterScreen)
            {
                return;
            }

            if (sizeInfo.HeightChanged)
            {
                Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;
            }

            if (sizeInfo.WidthChanged)
            {
                Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _ = e ?? throw new ArgumentNullException(nameof(e));

            base.OnMouseLeftButtonDown(e);

            if (Draggable && (e.ButtonState != MouseButtonState.Released))
            {
                DragMove();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "SA1313:Parameter '_' should begin with lower -case letter", Justification = "Discarded parameter")]
        void AnimatedWindowWithoutTemplate_Closing(object? sender, CancelEventArgs e)
        {
            _closing = true;
            e.Cancel = true;

            // Unsubscribe to prevent recurring call of this method after animation completes
            Closing -= AnimatedWindowWithoutTemplate_Closing;
            var hideAnimation = new DoubleAnimation { From = InitialOpacity, To = 0, Duration = _fadeDuration };

            hideAnimation.Completed += CompletedHandler;
            BeginAnimation(OpacityProperty, hideAnimation);
            return;

            void CompletedHandler(object? s, EventArgs _)
            {
                hideAnimation.Completed -= CompletedHandler;
                Close();
            }
        }

        void AnimatedWindowWithoutTemplate_ContentRendered(object? sender, EventArgs e)
        {
            var showAnimation = new DoubleAnimation { From = 0, To = InitialOpacity, Duration = _fadeDuration };

            showAnimation.Completed += CompletedHandler;
            BeginAnimation(OpacityProperty, showAnimation);
            return;

            void CompletedHandler(object? s, EventArgs eventArgs)
            {
                showAnimation.Completed -= CompletedHandler;
                BeginAnimation(TopProperty, null);
                BeginAnimation(LeftProperty, null);
                if (Focusable && ShowActivated && !_closing)
                {
                    Restore();
                }

                ContentRenderAnimationFinished?.Invoke(this, eventArgs);
            }
        }
    }
}
