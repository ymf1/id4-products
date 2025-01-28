using System;
using Bff;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

Console.Title = "BFF";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults();

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
        .ReadFrom.Configuration(ctx.Configuration));

    var serviceProviderAccessor = new ServiceProviderAccessor();

    var app = builder
        .ConfigureServices(() => serviceProviderAccessor.ServiceProvider ?? throw new InvalidOperationException("Service Provider must be set"))
        .ConfigurePipeline();

    serviceProviderAccessor.ServiceProvider = app.Services;

    app.Run();

    
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

/// <summary>
/// A workaround to get the service provider available in the ConfigureServices method
/// </summary>
class ServiceProviderAccessor
{
    public IServiceProvider? ServiceProvider { get; set; }

}
