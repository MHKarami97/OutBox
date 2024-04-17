using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Utility.Configuration;

public static class BackgroundJob
{
    public static void ConfigureJob(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = new AppSettings();
        configuration.GetSection("AppSettings").Bind(appSettings);

        services.AddQuartz(q => { });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }
}