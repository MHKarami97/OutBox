using Utility.Configuration;

namespace OutBoxConsumer.Configuration;

public static class HostedService
{
    public static void ConfigureHostedService(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = new AppSettings();
        configuration.GetSection("AppSettings").Bind(appSettings);

        services.AddHostedService<Worker>();

        services.Configure<HostOptions>(hostOptions => { hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore; });
    }
}