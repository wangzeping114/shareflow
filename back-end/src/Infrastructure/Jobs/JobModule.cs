using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ShareFlow.Infrastructure.Jobs;

public static class JobModule
{
    public static IServiceCollection AddShareFlowJobScheduling(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            // 分红补算定时任务：每天 02:00 UTC
            var jobKey = new JobKey("DividendCalculationJob");
            q.AddJob<DividendCalculationJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("DividendCalculationJob-trigger")
                .WithCronSchedule("0 0 2 * * ?"));   // 每天 02:00:00 UTC
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
