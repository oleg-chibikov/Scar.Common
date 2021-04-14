using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Scar.Common.WPF.Controls
{
    public class ZoomBorder : Border
    {
        const double MaxScale = 4;
        const double MaxScaleToZoomIn = MaxScale - 1;
        const double DefaultScale = 1;
        const double DefaultZoom = .4;
        readonly ScaleTransform _st = new ScaleTransform();
        readonly TranslateTransform _tt = new TranslateTransform();
        readonly Duration _zoomAnimationDuration = new Duration(TimeSpan.FromMilliseconds(200));
        UIElement _child;
        bool _isReset;
        Point _originBottomRight;
        Point _originTopLeft;
        Point _start;
        bool _isAnimationCancelled = true;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable. Set to not null in Initialize method
        public ZoomBorder()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
            // Unsubscribe not needed - same class
            MouseWheel += ZoomBorder_MouseWheel;
            MouseLeftButtonDown += ZoomBorder_MouseLeftButtonDown;
            MouseLeftButtonUp += ZoomBorder_MouseLeftButtonUp;
            SizeChanged += ZoomBorder_SizeChanged;
            MouseMove += ZoomBorder_MouseMove;
            PreviewMouseDown += ZoomBorder_PreviewMouseButtonDown;
        }

        public override UIElement Child
        {
            get => base.Child;
            set
            {
                if ((value != null) && !Equals(value, Child))
                {
                    Initialize(value);
                }

                base.Child = value;
            }
        }

        public void Reset()
        {
            if (_isReset || (_child == null))
            {
                return;
            }

            CancelPreviousAnimation();

            // reset zoom
            _st.ScaleX = 1.0;
            _st.ScaleY = 1.0;

            // reset pan
            _tt.X = 0.0;
            _tt.Y = 0.0;
            _isReset = true;
        }

        void Initialize(UIElement element)
        {
            _child = element;
            if (_child != null)
            {
                var group = new TransformGroup();
                group.Children.Add(_st);
                group.Children.Add(_tt);
                _child.RenderTransform = group;
                _child.RenderTransformOrigin = new Point(0.0, 0.0);
            }
        }

        void ZoomBorder_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            Zoom(e.Delta > 0 ? DefaultZoom : -DefaultZoom, e, false);
        }

        void ZoomBorder_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (_child == null)
            {
                return;
            }

            if (e.ClickCount >= 2)
            {
                Zoom(_st.ScaleX < MaxScaleToZoomIn ? MaxScale - _st.ScaleX : -_st.ScaleX + DefaultScale, e, true);
            }
            else
            {
                CancelPreviousAnimation();
                _start = e.GetPosition(this);
                _originTopLeft = new Point(_tt.X, _tt.Y);
                _originBottomRight = new Point(_tt.X + (_child.RenderSize.Width * _st.ScaleX), _tt.Y + (_child.RenderSize.Height * _st.ScaleY));
                Cursor = Cursors.Hand;
                _child.CaptureMouse();
            }
        }

        void ZoomBorder_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if ((_child == null) || (e.LeftButton == MouseButtonState.Pressed))
            {
                return;
            }

            _child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }

        void ZoomBorder_PreviewMouseButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                Reset();
            }
        }

        void ZoomBorder_MouseMove(object? sender, MouseEventArgs e)
        {
            // ReSharper disable once MergeSequentialChecksWhenPossible
            if ((_child == null) || !_child.IsMouseCaptured || (e.LeftButton != MouseButtonState.Pressed))
            {
                return;
            }

            var currentPosition = e.GetPosition(this);
            var v = _start - currentPosition;

            // check Bounds
            var newXLeft = _originTopLeft.X - v.X;
            var newXRight = _originBottomRight.X - v.X;
            var newYTop = _originTopLeft.Y - v.Y;
            var newYBottom = _originBottomRight.Y - v.Y;
            bool shiftX = true, shiftY = true;

            // pull to the right
            if (v.X < 0)
            {
                if (newXLeft > 0)
                {
                    shiftX = false;
                }
            }

            // pull to the left
            else
            {
                if (newXRight < _child.RenderSize.Width)
                {
                    shiftX = false;
                }
            }

            // pull to the bottom
            if (v.Y < 0)
            {
                if (newYTop > 0)
                {
                    shiftY = false;
                }
            }

            // pull to the top
            else
            {
                if (newYBottom < _child.RenderSize.Height)
                {
                    shiftY = false;
                }
            }

            if (shiftX || shiftY)
            {
                if (shiftX)
                {
                    _tt.X = newXLeft;
                }

                if (shiftY)
                {
                    _tt.Y = newYTop;
                }
            }
        }

        void ZoomBorder_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            const double tolerance = 0.1;
            if (!e.PreviousSize.Height.Equals(0) && (Math.Abs(e.PreviousSize.Height - e.NewSize.Height) >= tolerance))
            {
                Reset();
            }
        }

        void Zoom(double zoom, MouseEventArgs e, bool animate)
        {
            if (_child == null)
            {
                return;
            }

            var newScale = _st.ScaleX + zoom;
            if (newScale < DefaultScale)
            {
                zoom = DefaultScale - _st.ScaleX;
            }
            else if (newScale > MaxScale)
            {
                zoom = _st.ScaleX - MaxScale;
            }

            if (zoom.Equals(0))
            {
                return;
            }

            var position = e.GetPosition(_child);

            var absoluteX = (position.X * _st.ScaleX) + _tt.X;
            var absoluteY = (position.Y * _st.ScaleY) + _tt.Y;
            var newScaleX = _st.ScaleX + zoom;
            var newScaleY = _st.ScaleY + zoom;

            var diffX = position.X * newScaleX;
            var diffY = position.Y * newScaleY;

            var newX = absoluteX - diffX;
            var newY = absoluteY - diffY;

            // Check Bounds For Zoom Out Only
            if (zoom <= 0)
            {
                var bottomRight = new Point(absoluteX + (_child.RenderSize.Width * newScaleX), absoluteY + (_child.RenderSize.Height * newScaleY));

                var newXRight = bottomRight.X - diffX;
                var newYBottom = bottomRight.Y - diffY;

                if (newXRight < _child.RenderSize.Width)
                {
                    newX += _child.RenderSize.Width - newXRight;
                }

                if (newYBottom < _child.RenderSize.Height)
                {
                    newY += _child.RenderSize.Height - newYBottom;
                }

                newX = newX > 0 ? 0 : newX;
                newY = newY > 0 ? 0 : newY;
            }

            if (animate)
            {
                Animate(newScaleX, newScaleY, newX, newY);
            }
            else
            {
                CancelPreviousAnimation();
                _st.ScaleX = newScaleX;
                _st.ScaleY = newScaleY;
                _tt.X = newX;
                _tt.Y = newY;
            }

            _isReset = false;
        }

        void Animate(double newScaleX, double newScaleY, double newX, double newY)
        {
            var scaleXAnimation = new DoubleAnimation { From = _st.ScaleX, To = newScaleX, Duration = _zoomAnimationDuration };
            var scaleYAnimation = new DoubleAnimation { From = _st.ScaleY, To = newScaleY, Duration = _zoomAnimationDuration };
            var aX = new DoubleAnimation { From = _tt.X, To = newX, Duration = _zoomAnimationDuration };
            var aY = new DoubleAnimation { From = _tt.Y, To = newY, Duration = _zoomAnimationDuration };

            _isAnimationCancelled = false;
            _st.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            _st.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
            _tt.BeginAnimation(TranslateTransform.XProperty, aX);
            _tt.BeginAnimation(TranslateTransform.YProperty, aY);
        }

        void CancelPreviousAnimation()
        {
            if (_isAnimationCancelled)
            {
                return;
            }

            double x = _tt.X, y = _tt.Y, scaleX = _st.ScaleX, scaleY = _st.ScaleY;

            // Remembering the coordinates is needed because setting the animation to null resets the property (unexpectedly)
            _st.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            _st.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            _tt.BeginAnimation(TranslateTransform.XProperty, null);
            _tt.BeginAnimation(TranslateTransform.YProperty, null);
            _tt.X = x;
            _tt.Y = y;
            _st.ScaleX = scaleX;
            _st.ScaleY = scaleY;
            _isAnimationCancelled = true;
        }
    }
}
