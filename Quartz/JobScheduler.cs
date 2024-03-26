using System;
using System.Threading.Tasks;
using Quartz;

namespace Scar.Common.Quartz
{
    public class JobScheduler(IScheduler scheduler)
    {
        public async Task ScheduleJobsAsync<T>(JobScheduleOptions options = JobScheduleOptions.Hourly, TimeSpan? dailyRunTime = null)
            where T : IJob
        {
            // Grab the Scheduler instance from the Factory
            await scheduler.Start().ConfigureAwait(false);

            if (options.HasFlag(JobScheduleOptions.Weekly))
            {
                await ScheduleWeeklyJobAsync<T>(DayOfWeek.Monday, dailyRunTime).ConfigureAwait(false);
            }

            if (options.HasFlag(JobScheduleOptions.Immediate))
            {
                await ScheduleImmediateJobAsync<T>().ConfigureAwait(false);
            }

            if (options.HasFlag(JobScheduleOptions.Daily))
            {
                await ScheduleDailyJobAsync<T>(dailyRunTime).ConfigureAwait(false);
            }

            if (options.HasFlag(JobScheduleOptions.TwiceDaily))
            {
                await ScheduleTwiceDailyJobAsync<T>(dailyRunTime).ConfigureAwait(false);
            }

            if (options.HasFlag(JobScheduleOptions.Hourly))
            {
                await ScheduleHourlyJobAsync<T>().ConfigureAwait(false);
            }

            if (options.HasFlag(JobScheduleOptions.EveryMinute))
            {
                await ScheduleEveryMinuteJobAsync<T>().ConfigureAwait(false);
            }
        }

        async Task ScheduleImmediateJobAsync<T>()
            where T : IJob
        {
            var jobName = "Immediate" + typeof(T).Name; // Generate a unique job name

            // define the job and tie it to the class
            var job = JobBuilder.Create<T>().WithIdentity(jobName) // Associate with a job identity
                .Build();

            // Trigger the job to run immediately
            var trigger = TriggerBuilder.Create().WithIdentity(jobName).StartNow() // Start immediately
                .ForJob(job) // Associate with the job
                .Build();

            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger).ConfigureAwait(false);
        }

        async Task ScheduleDailyJobAsync<T>(TimeSpan? dailyRunTime)
            where T : IJob
        {
            var jobName = "Daily" + typeof(T).Name; // Generate a unique job name

            // define the job and tie it to the class
            var job = JobBuilder.Create<T>().WithIdentity(jobName) // Associate with a job identity
                .Build();

            var dailyTriggerBuilder = TriggerBuilder.Create().WithIdentity(jobName);

            dailyTriggerBuilder = dailyRunTime.HasValue
                ? dailyTriggerBuilder.WithDailyTimeIntervalSchedule(
                    x => x.WithIntervalInHours(24).OnEveryDay().StartingDailyAt(
                        TimeOfDay.HourAndMinuteOfDay(
                            dailyRunTime.Value.Hours,
                            dailyRunTime.Value.Minutes)))
                : dailyTriggerBuilder.WithSchedule(
                    CronScheduleBuilder.DailyAtHourAndMinute(
                        10,
                        0));

            // Trigger the job to run daily at a specific time
            var dailyTrigger = dailyTriggerBuilder.ForJob(job) // Associate with the job
                .Build();

            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(
                job,
                dailyTrigger).ConfigureAwait(false);
        }

        async Task ScheduleHourlyJobAsync<T>()
            where T : IJob
        {
            var jobName = "Hourly" + typeof(T).Name; // Generate a unique job name

            // define the job and tie it to the class
            var job = JobBuilder.Create<T>().WithIdentity(jobName) // Associate with a job identity
                .Build();

            // Trigger the job to run hourly
            var trigger = TriggerBuilder.Create().WithIdentity(jobName)
                .WithSchedule(SimpleScheduleBuilder.RepeatHourlyForever())
                .ForJob(job) // Associate with the job
                .Build();

            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger).ConfigureAwait(false);
        }

        async Task ScheduleTwiceDailyJobAsync<T>(TimeSpan? startTime = null)
            where T : IJob
        {
            var jobName = "TwiceDaily" + typeof(T).Name; // Generate a unique job name

            // Define the job and tie it to the class
            var job = JobBuilder.Create<T>().WithIdentity(jobName) // Associate with a job identity
                .Build();

            var firstRunTime = startTime ?? TimeSpan.FromHours(10); // Default to 10 AM if startTime is not provided
            var secondRunTime = startTime?.Add(TimeSpan.FromHours(12)) ?? TimeSpan.FromHours(22); // Default to 10 PM if startTime is not provided

            // Trigger the job to run twice daily
            var trigger1 = TriggerBuilder.Create().WithIdentity(jobName + "Trigger1")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(firstRunTime.Hours, firstRunTime.Minutes))
                .ForJob(job) // Associate with the job
                .Build();

            var trigger2 = TriggerBuilder.Create().WithIdentity(jobName + "Trigger2")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(secondRunTime.Hours, secondRunTime.Minutes))
                .ForJob(job) // Associate with the job
                .Build();

            // Tell Quartz to schedule the job using our triggers
            await scheduler.ScheduleJob(job, trigger1).ConfigureAwait(false);
            await scheduler.ScheduleJob(job, trigger2).ConfigureAwait(false);
        }

        async Task ScheduleEveryMinuteJobAsync<T>()
            where T : IJob
        {
            var jobName = "EveryMinute" + typeof(T).Name; // Generate a unique job name

            // Define the job and tie it to the class
            var job = JobBuilder.Create<T>().WithIdentity(jobName) // Associate with a job identity
                .Build();

            // Trigger the job to run every minute
            var trigger = TriggerBuilder.Create().WithIdentity(jobName)
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever()) // Cron expression for every minute
                .ForJob(job) // Associate with the job
                .Build();

            // Tell Quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger).ConfigureAwait(false);
        }

        async Task ScheduleWeeklyJobAsync<T>(DayOfWeek? dayOfWeek = DayOfWeek.Monday, TimeSpan? timeOfDay = null)
            where T : IJob
        {
            var jobName = "Weekly" + typeof(T).Name; // Generate a unique job name

            // Define the job and tie it to the class
            var job = JobBuilder.Create<T>().WithIdentity(jobName) // Associate with a job identity
                .Build();

            // Set default values if parameters are not provided
            dayOfWeek ??= DayOfWeek.Monday;
            timeOfDay ??= new TimeSpan(11, 0, 0); // Default to 11:00 AM

            // Trigger the job to run weekly at a specific day and time
            var weeklyTrigger = TriggerBuilder.Create().WithIdentity(jobName)
                .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(dayOfWeek.Value, timeOfDay.Value.Hours, timeOfDay.Value.Minutes))
                .ForJob(job) // Associate with the job
                .Build();

            // Tell Quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, weeklyTrigger).ConfigureAwait(false);
        }
    }
}
