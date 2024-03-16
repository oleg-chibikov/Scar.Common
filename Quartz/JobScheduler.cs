using System.Threading.Tasks;
using Quartz;

namespace Scar.Common.Quartz;

public class JobScheduler(IScheduler scheduler)
{
    public async Task ScheduleJobsAsync<T>(JobScheduleOptions options = JobScheduleOptions.Both)
        where T : IJob
    {
        // Grab the Scheduler instance from the Factory
        await scheduler.Start().ConfigureAwait(false);

        if (options.HasFlag(JobScheduleOptions.Immediate))
        {
            await ScheduleImmediateJobAsync<T>().ConfigureAwait(false);
        }

        if (options.HasFlag(JobScheduleOptions.Daily))
        {
            await ScheduleDailyJobAsync<T>().ConfigureAwait(false);
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

    async Task ScheduleDailyJobAsync<T>()
        where T : IJob
    {
        var jobName = "Daily" + typeof(T).Name; // Generate a unique job name

        // define the job and tie it to the class
        var job = JobBuilder.Create<T>().WithIdentity(jobName) // Associate with a job identity
            .Build();

        // Trigger the job to run daily at a specific time
        var dailyTrigger = TriggerBuilder.Create().WithIdentity(jobName).WithDailyTimeIntervalSchedule(
                x => x.WithIntervalInHours(24) // Run every 24 hours
                    .OnEveryDay().StartingDailyAt(
                        TimeOfDay.HourAndMinuteOfDay(
                            0,
                            0))) // Start at midnight
            .ForJob(job) // Associate with the job
            .Build();

        // Tell quartz to schedule the job using our trigger
        await scheduler.ScheduleJob(job, dailyTrigger).ConfigureAwait(false);
    }
}
