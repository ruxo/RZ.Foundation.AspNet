using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace RZ.AspNet;

[PublicAPI]
public interface IAspHostEvents
{
    IAspHostEvents OnStart(Action<IServiceProvider> onStart);
    IAspHostEvents OnStart(Func<IServiceProvider, Task> onStart);
    IAspHostEvents OnShutdown(Action<IServiceProvider> onShutdown);
    IAspHostEvents OnShutdown(Func<IServiceProvider, Task> onShutdown);
    IServiceCollection Then { get; }
}

[PublicAPI]
public static class AspHost
{
    public static IAspHostEvents AspHostEvents(this IServiceCollection services) {
        var collector = new HostEventCollector(services);
        services.AddHostedService(sp => new HostEvents(collector, sp));
        return collector;
    }

    public static async Task<Unit> RunPipeline(this WebApplicationBuilder builder, params AppModule[] modules) {
        foreach(var module in modules)
            await module.InstallServices(builder);

        var app = builder.Build();
        foreach(var module in modules)
            await module.InstallMiddleware(builder, app);

        await app.RunAsync();
        return unit;
    }
    const string EnvironmentKey = "ASPNETCORE_ENVIRONMENT";

    [ExcludeFromCodeCoverage]
    public static IConfiguration CreateDefaultConfigurationSettings() {
        return new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .AddJsonFile($"appsettings.{Environment}.json", optional: true)
              .AddEnvironmentVariables()
              .Build();
    }

    public static string Environment
        => System.Environment.GetEnvironmentVariable(EnvironmentKey) ?? "Production";

    public class HostEventCollector(IServiceCollection services) : IAspHostEvents
    {
        public IServiceCollection Then => services;

        internal Func<IServiceProvider, Task>? Start { get; private set; }
        internal Func<IServiceProvider, Task>? Shutdown { get; private set; }

        public IAspHostEvents OnStart(Action<IServiceProvider> onStart) {
            OnStart(sp => {
                onStart(sp);
                return Task.CompletedTask;
            });
            return this;
        }

        public IAspHostEvents OnStart(Func<IServiceProvider, Task> onStart) {
            Start = onStart;
            return this;
        }

        public IAspHostEvents OnShutdown(Action<IServiceProvider> onShutdown) {
            OnShutdown(sp => {
                onShutdown(sp);
                return Task.CompletedTask;
            });
            return this;
        }

        public IAspHostEvents OnShutdown(Func<IServiceProvider, Task> onShutdown) {
            Shutdown = onShutdown;
            return this;
        }
    }

    public class HostEvents(HostEventCollector collector, IServiceProvider sp) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken) {
            if (collector.Start is not null)
                await collector.Start.Invoke(sp);
        }
        public async Task StopAsync(CancellationToken cancellationToken) {
            if (collector.Shutdown is not null)
                await collector.Shutdown.Invoke(sp);
        }
    }
}