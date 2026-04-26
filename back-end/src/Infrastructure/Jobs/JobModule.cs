using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ShareFlow.Infrastructure.Jobs;

public static class JobModule
{
    public static IServiceCollection AddShareFlowJobScheduling(this IServiceCollection services)
    {
        services.AddQuartz();

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
