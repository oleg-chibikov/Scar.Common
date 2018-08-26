using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using JetBrains.Annotations;
using Xceed.Wpf.DataGrid;

namespace Scar.Common.WPF.Xceed
{
    public sealed class XceedAutoFitColumnGrid : DataGridControl
    {
        public static readonly DependencyProperty ColumnsWidthRecalculationIntervalProperty = DependencyProperty.Register(
            nameof(ColumnsWidthRecalculationInterval),
            typeof(TimeSpan),
            typeof(XceedAutoFitColumnGrid),
            new PropertyMetadata(TimeSpan.FromMilliseconds(300)));

        private static readonly RoutedEventHandler OnDataCellLostFocus = (sender, args) =>
        {
            var cell = (DataCell)sender;
            if (cell.ParentRow.IsBeingEdited)
            {
                cell.ParentRow.EndEdit();
            }
        };

        private readonly IRateLimiter _rateLimiter;

        public XceedAutoFitColumnGrid()
        {
            IsVisibleChanged += XceedAutoFitColumnGrid_IsVisibleChanged;
            ItemsSourceChangeCompleted += XceedAutoFitColumnGrid_ItemsSourceChangeCompleted;
            _rateLimiter = new RateLimiter(SynchronizationContext.Current);

            Resources[typeof(DataCell)] = new Style(typeof(DataCell), (Style)TryFindResource(typeof(DataCell)))
            {
                Setters =
                {
                    new EventSetter(LostFocusEvent, OnDataCellLostFocus)
                }
            };
        }

        public TimeSpan ColumnsWidthRecalculationInterval
        {
            get => (TimeSpan)GetValue(ColumnsWidthRecalculationIntervalProperty);
            set => SetValue(ColumnsWidthRecalculationIntervalProperty, value);
        }

        private void AdjustColumnsWidths()
        {
            foreach (var columnBase in Columns)
            {
                if (columnBase.HasFixedWidth)
                {
                    continue;
                }

                var fittedWidth = columnBase.GetFittedWidth();
                double headerWidth = 0;
                if (columnBase.Title != null)
                {
                    headerWidth = MeasureString(columnBase.Title.ToString()).Width + 7;
                }

                var max = Math.Max(headerWidth, fittedWidth);
                if (max > 0)
                {
                    columnBase.Width = max;
                }
            }
        }

        private Size MeasureString([NotNull] string candidate)
        {
            var dpiScale = VisualTreeHelper.GetDpi(this);
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black,
                dpiScale.PixelsPerDip);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private void XceedAutoFitColumnGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                LayoutUpdated += XceedAutoFitColumnGrid_LayoutUpdated;
            }
            else
            {
                LayoutUpdated -= XceedAutoFitColumnGrid_LayoutUpdated;
            }
        }

        private void XceedAutoFitColumnGrid_ItemsSourceChangeCompleted(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        private void XceedAutoFitColumnGrid_LayoutUpdated(object sender, EventArgs e)
        {
            _rateLimiter.Throttle(ColumnsWidthRecalculationInterval, AdjustColumnsWidths);
        }
    }
}