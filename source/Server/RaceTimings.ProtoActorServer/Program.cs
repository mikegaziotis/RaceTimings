using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Proto.Persistence;
using RaceTimings.ProtoActorServer;
using RaceTimings.ProtoActorServer.Stores;
using StackExchange.Redis;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        
        SystemConfiguration.Mqqt = configuration.GetValue<MqqtConfiguration>("Mqtt")
            ?? throw new InvalidOperationException("Missing Mqtt configuration");
        SystemConfiguration.Redis = configuration.GetValue<RedisConfiguration>("Redis")
                                   ?? throw new InvalidOperationException("Missing Redis configuration");
        
        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect("localhost"));
        
        
        // Register your ActorSystemService
        services.AddHostedService<ActorSystemService>();
        services.AddSingleton<ActorDependencyResolver>();
            
        // OpenTelemetry Tracing with Resource Info
        services.ConfigureOpenTelemetryTracerProvider(builder => builder
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService("ActorSystemApp"))
                .AddConsoleExporter() // Console trace exporter
        );

        // Add OpenTelemetry Logging
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddOpenTelemetry(options =>
            {
                options.AddConsoleExporter(); // Log exporter to console
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
            });
        });
    })
    .UseConsoleLifetime() // Keeps the console app running until cancellation
    .Build();

// Run the host
await host.RunAsync();