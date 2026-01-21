using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.Blazor;

public class VmToolkit(ILogger logger, IHostEnvironment host, ShellViewModel shell, IServiceProvider services, IEventBubbleSubscription bubble)
{
    public ILogger Logger { get; } = logger;
    public IHostEnvironment Host { get; } = host;
    public ShellViewModel Shell { get; } = shell;
    public IServiceProvider Services { get; } = services;
    public IEventBubbleSubscription Bubble { get; } = bubble;
}

[PublicAPI]
public sealed class VmToolkit<T>(ILogger<T> logger, IHostEnvironment host, ShellViewModel shell, IServiceProvider services, IEventBubbleSubscription bubble)
    : VmToolkit(logger, host, shell, services, bubble)
{
    public new ILogger<T> Logger { get; } = logger;
}