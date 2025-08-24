using Serilog;

namespace BookIt.API.Middleware.Logging;

public static class LoggingConfigurator
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder, IConfiguration config)
    {
        var logFolder = config["LogSettings:LogFolder"] ?? "Logs";
        var retainedFileCount = config.GetValue("LogSettings:RetainedFileCountLimit", 30);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.With<CallerEnricher>()
            .CreateLogger();

        return builder.UseSerilog();
    }
}
