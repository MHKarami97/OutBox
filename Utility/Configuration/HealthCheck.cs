using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Utility.Configuration;

public static class HealthCheck
{
    public static void ConfigureHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var omsDatabase = configuration.GetConnectionString("OmsDatabase");
        var tsetmcReporterDatabase = configuration.GetConnectionString("TsetmcReporterDatabase");
        var bourseDatabase = configuration.GetConnectionString("BourseDatabase");
        var redis = configuration.GetConnectionString("RedisCache");

        if (string.IsNullOrEmpty(omsDatabase) ||
            string.IsNullOrEmpty(tsetmcReporterDatabase) ||
            string.IsNullOrEmpty(bourseDatabase) ||
            string.IsNullOrEmpty(redis))
        {
            throw new InvalidOperationException("Could not find a connection string named " +
                                                "'omsDatabase' or " +
                                                "'tsetmcReporterDatabase' or " +
                                                "'bourseDatabase' or " +
                                                "'redis'");
        }

        _ = services.AddHealthChecks()
            .AddSqlServer(omsDatabase, name: "OmsDatabase")
            .AddSqlServer(tsetmcReporterDatabase, name: "TsetmcReporterDatabase")
            .AddOracle(bourseDatabase, name: "BourseDatabase")
            .AddRedis(redis, name: "Redis")
            .AddCheck<EndpointHealthChecker>("Apm", HealthStatus.Degraded, new[] { nameof(EndpointHealthChecker) })
            .AddCheck<AppSettingChecker>("AppSettingChecker", HealthStatus.Degraded, new[] { nameof(AppSettingChecker) })
            .AddCheck<SystemMemoryHealthcheck>("SystemMemoryHealthcheck", HealthStatus.Degraded, new[] { nameof(SystemMemoryHealthcheck) });
    }

    public static void AddHealthCheck(this IEndpointRouteBuilder app)
    {
        _ = app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = WriteHealthCheckResponseAsync,
        });
    }

    private static Task WriteHealthCheckResponseAsync(HttpContext httpContext, HealthReport healthReport)
    {
        httpContext.Response.ContentType = "application/json";

        var dependencyHealthChecks = healthReport.Entries.Select(entry => new
        {
            Name = entry.Key,
            Status = entry.Value.Status.ToString(),
            DurationInSeconds = entry.Value.Duration.TotalSeconds.ToString("0:0.000", new CultureInfo("en-US")),
            Discription = entry.Value.Description,
            Exception = entry.Value.Exception?.Message,
            Data = entry.Value.Data
        });

        var healthCheckResponse = new
        {
            Status = healthReport.Status.ToString(),
            TotalCheckExecutionTimeInSeconds = healthReport.TotalDuration.TotalSeconds.ToString("0:0.000", new CultureInfo("en-US")),
            DependencyHealthChecks = dependencyHealthChecks,
        };

        var responseString = JsonSerializer.Serialize(healthCheckResponse, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        return httpContext.Response.WriteAsync(responseString);
    }
}

public class EndpointHealthChecker : IHealthCheck
{
    private readonly TracingSettings _appSettings;

    public EndpointHealthChecker(IOptionsMonitor<TracingSettings> appSettings)
    {
        _appSettings = appSettings.CurrentValue;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var baseUrl = _appSettings.OtlpEndpoint;

        try
        {
            var result = await baseUrl.AllowAnyHttpStatus().GetAsync(cancellationToken: cancellationToken);

            return result is { StatusCode: 200 }
                ? HealthCheckResult.Healthy($"Apm available ({baseUrl})")
                : HealthCheckResult.Unhealthy($"Apm not available ({baseUrl})");
        }
        catch (Exception)
        {
            return HealthCheckResult.Degraded($"Exception on call ({baseUrl})");
        }
    }
}

public class AppSettingChecker : IHealthCheck
{
    private readonly AppSettings _appSettings;

    public AppSettingChecker(IOptionsMonitor<AppSettings> appSetting)
    {
        _appSettings = appSetting.CurrentValue;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return HealthCheckResult.Healthy(null, _appSettings.ToDictionary());
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("Exception on appSetting");
        }
    }
}

public class SystemMemoryHealthcheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var client = new MemoryMetricsClient();
        var metrics = client.GetMetrics();
        var percentUsed = 100 * metrics.Used / metrics.Total;
        var status = HealthStatus.Healthy;

        if (percentUsed > 80)
        {
            status = HealthStatus.Degraded;
        }

        if (percentUsed > 90)
        {
            status = HealthStatus.Unhealthy;
        }

        var data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "Total", metrics.Total },
            { "Used", metrics.Used },
            { "Free", metrics.Free },
        };

        var result = new HealthCheckResult(status, description: null, exception: null, data);

        return await Task.FromResult(result);
    }
}