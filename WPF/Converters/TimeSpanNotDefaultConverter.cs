using System;
using System.Windows.Data;

namespace Scar.Common.WPF.Converters;

[ValueConversion(typeof(TimeSpan), typeof(bool))]
public sealed class TimeSpanNotDefaultConverter : NotDefaultConverter<TimeSpan>
{
}