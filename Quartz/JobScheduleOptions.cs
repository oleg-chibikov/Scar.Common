using System;

namespace Scar.Common.Quartz;

[Flags]
public enum JobScheduleOptions
{
    None = 0,
    Weekly = 1,
    Immediate = 2,
    Daily = 4,
    TwiceDaily = 8,
    Hourly = 16,
    EveryMinute = 32
}
