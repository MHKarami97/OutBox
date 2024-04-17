using System.Globalization;
using OutBoxPublisher.Configuration;
using Serilog;
using Utility.Configuration;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: new CultureInfo("fa-IR"))
    .CreateBootstrapLogger();

Log.Information("Starting up...");

try
{
    var builderConfiguration = builder.Configuration.AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", reloadOnChange: true, optional: true)
        .AddEnvironmentVariables();

    var configuration = builderConfiguration.Build();
    var services = builder.Services;

    services.ConfigureLogger(configuration);
    services.ConfigureHealthCheck(configuration);
    services.ConfigureCache(configuration);
    services.ConfigureSection(configuration);
    services.ConfigureTracing(configuration);
    services.ConfigureMetric(configuration);

    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    services.ConfigureHostedService(configuration);
    services.ConfigureJob(configuration);

    var app = builder.Build();

    app.AddHealthCheck();

    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "An unhandled exception occured during bootstrapping");
    throw;
}
finally
{
    Log.Information("Stopped...");
    
    await Log.CloseAndFlushAsync();
}