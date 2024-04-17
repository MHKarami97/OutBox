using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Utility.Configuration;

public static class TracingConfig
{
    public static void ConfigureTracing(this IServiceCollection services, IConfiguration configuration)
    {
        var tracingSettings = new TracingSettings();
        configuration.GetSection("Tracing").Bind(tracingSettings);

        if (!tracingSettings.Enabled)
        {
            return;
        }

        _ = services.AddOpenTelemetry()
            .ConfigureResource(builder =>
            {
                var attributes = new List<KeyValuePair<string, object>>
                {
                    new("deployment.environment", Tracing.Source),
                    new("host.name", Environment.MachineName),
                };

                _ = builder.AddService(serviceName: tracingSettings.ServiceName, serviceNamespace: tracingSettings.ServiceName, serviceVersion: Tracing.Version,
                        serviceInstanceId: Environment.MachineName + $":{tracingSettings.ServiceName}")
                    .AddAttributes(attributes)
                    .AddEnvironmentVariableDetector();
            })
            .WithTracing(builder =>
            {
                _ = builder.SetErrorStatusOnException()
                    .AddSource(Tracing.Source)
                    .AddHttpClientInstrumentation(options => { options.RecordException = true; })
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.RecordException = true;
                    })
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(tracingSettings.OtlpEndpoint);
                        options.Headers = "Authorization= ApiKey " + tracingSettings.OtlpAuthorizationHeader;
                    });
            });
    }
}