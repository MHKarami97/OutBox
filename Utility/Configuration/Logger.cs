using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;

namespace Utility.Configuration;

public static class Logger
{
    public static void ConfigureLogger(this IServiceCollection builder, IConfiguration configuration)
    {
        builder.AddLogging(config =>
        {
            config.ClearProviders();
    
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Filter.ByExcluding(Matching.FromSource("Quartz"))
                .Filter.ByExcluding(Matching.FromSource("Microsoft.Hosting.Lifetime"))
                .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore.Database.Command"))
                .Filter.ByExcluding(Matching.FromSource("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted"))
                .CreateLogger();
    
            config.AddSerilog(logger);
        });

        Quartz.Logging.LogProvider.IsDisabled = true;
    }
}