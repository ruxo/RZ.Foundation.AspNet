using System.Reactive.Concurrency;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation;

[PublicAPI]
public static class BlazorSettings
{
    public static IServiceCollection AddRzBlazorSettings(this IServiceCollection services,
                                                         Func<IServiceProvider, ShellOptions>? options = null)
        => services
          .AddSingleton(TimeProvider.System)
          .AddSingleton<IViewFinder, ViewFinder>()
          .AddScoped<IScheduler>(_ => new SynchronizationContextScheduler(SynchronizationContext.Current!))
          .AddScoped<IActivator, Activator>()
          .AddScoped<IEventBubbleSubscription, EventBubbleSubscription>()
          .AddScoped<AppChromeViewModel>()
          .AddScoped<ShellViewModel>(
               sp => sp.GetRequiredService<IActivator>().Create<ShellViewModel>(options?.Invoke(sp) ?? new ShellOptions()));
}