using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Utility.Configuration;

public static class Section
{
    public static void ConfigureSection(this IServiceCollection service, IConfiguration configuration)
    {
        service.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        service.Configure<TracingSettings>(configuration.GetSection("Tracing"));
    }
}