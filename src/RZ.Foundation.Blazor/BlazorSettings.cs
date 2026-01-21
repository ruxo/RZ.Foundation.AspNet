using System.Reactive.Concurrency;
using Microsoft.Extensions.DependencyInjection;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation;

[PublicAPI]
public static class BlazorSettings
{
    public static IServiceCollection AddRzBlazorSettings(this IServiceCollection services)
        => services
          .AddSingleton(TimeProvider.System)
          .AddSingleton<IViewFinder, ViewFinder>()
          .AddScoped<IScheduler>(_ => new SynchronizationContextScheduler(SynchronizationContext.Current!))
          .AddScoped<IEventBubbleSubscription, EventBubbleSubscription>()
          .AddScoped(typeof(VmToolkit<>))
          .AddScoped<AppChromeViewModel>()
          .AddScoped<ShellViewModel>(sp => sp.Create<ShellViewModel>());
}