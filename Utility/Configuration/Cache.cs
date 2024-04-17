using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Utility.Configuration;

public static class Cache
{
    public static void ConfigureCache(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = new AppSettings();
        configuration.GetSection("AppSettings").Bind(appSettings);
    }
}