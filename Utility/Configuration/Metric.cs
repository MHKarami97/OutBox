using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace Utility.Configuration;

public static class Metric
{
    public static void ConfigureMetric(this IServiceCollection services, IConfiguration configuration)
    {
        var tracingSettings = new TracingSettings();
        configuration.GetSection("Tracing").Bind(tracingSettings);

        if (!tracingSettings.Enabled)
        {
            return;
        }

        _ = services.AddOpenTelemetry()
            .WithMetrics(opts => opts
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(tracingSettings.OtlpEndpoint);
                    options.Headers = "Authorization= ApiKey " + tracingSettings.OtlpAuthorizationHeader;
                })
            );
    }
}