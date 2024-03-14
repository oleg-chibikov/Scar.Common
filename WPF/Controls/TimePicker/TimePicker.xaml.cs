using System;
using System.Windows;

namespace Scar.Common.WPF.Controls;

public sealed partial class TimePicker
{
    public static readonly DependencyProperty DaysProperty = DependencyProperty.Register(
        nameof(Days),
        typeof(int),
        typeof(TimePicker),
        new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.None, OnTimePartChanged));

    public static readonly DependencyProperty HoursProperty = DependencyProperty.Register(
        nameof(Hours),
        typeof(int),
        typeof(TimePicker),
        new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.None, OnTimePartChanged, (d, value) => Coerce(d, value, DaysProperty, MaxHours)));

    public static readonly DependencyProperty MinutesProperty = DependencyProperty.Register(
        nameof(Minutes),
        typeof(int),
        typeof(TimePicker),
        new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.None, OnTimePartChanged, (d, value) => Coerce(d, value, HoursProperty, MaxMinutes)));

    public static readonly DependencyProperty SecondsProperty = DependencyProperty.Register(
        nameof(Seconds),
        typeof(int),
        typeof(TimePicker),
        new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.None, OnTimePartChanged, (d, value) => Coerce(d, value, MinutesProperty, MaxSeconds)));

    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
        nameof(Time),
        typeof(TimeSpan),
        typeof(TimePicker),
        new FrameworkPropertyMetadata(default(TimeSpan), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeChanged));

    const int MaxHours = 24;
    const int MaxMinutes = 60;
    const int MaxSeconds = 60;

    bool _isTimePartChanging;

    public TimePicker()
    {
        InitializeComponent();
    }

    public int Days
    {
        get => (int)GetValue(DaysProperty);
        set => SetValue(DaysProperty, value);
    }

    public int Hours
    {
        get => (int)GetValue(HoursProperty);
        set => SetValue(HoursProperty, value);
    }

    public int Minutes
    {
        get => (int)GetValue(MinutesProperty);
        set => SetValue(MinutesProperty, value);
    }

    public int Seconds
    {
        get => (int)GetValue(SecondsProperty);
        set => SetValue(SecondsProperty, value);
    }

    public TimeSpan Time
    {
        get => (TimeSpan)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    static object Coerce(DependencyObject d, object value, DependencyProperty greater, int max)
    {
        var greaterScale = (int)d.GetValue(greater);
        var dVal = (int)value;

        if (dVal > max)
        {
            d.SetValue(greater, greaterScale + 1);
            return 0;
        }

        if (dVal < 0)
        {
            if (greaterScale > 0)
            {
                d.SetValue(greater, greaterScale - 1);
            }

            return max;
        }

        return dVal;
    }

    static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;

        // To prevent circular property changes
        if (timePicker._isTimePartChanging)
        {
            return;
        }

        var newTime = (TimeSpan)e.NewValue;
        timePicker.Days = newTime.Days;
        timePicker.Hours = newTime.Hours;
        timePicker.Minutes = newTime.Minutes;
        timePicker.Seconds = newTime.Seconds;
    }

    static void OnTimePartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;
        timePicker._isTimePartChanging = true;
        timePicker.Time = new TimeSpan(timePicker.Days, timePicker.Hours, timePicker.Minutes, timePicker.Seconds);
        timePicker._isTimePartChanging = false;
    }
}
