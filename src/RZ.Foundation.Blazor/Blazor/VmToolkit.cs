using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.Blazor;

public class VmToolkit(ILogger logger, IHostEnvironment host, ShellViewModel shell, IActivator activator, IEventBubbleSubscription bubble)
{
    public ILogger Logger { get; } = logger;
    public IHostEnvironment Host { get; } = host;
    public ShellViewModel Shell { get; } = shell;
    public IActivator Activator { get; } = activator;
    public IEventBubbleSubscription Bubble { get; } = bubble;
}

public sealed class VmToolkit<T>(ILogger<T> logger, IHostEnvironment host, ShellViewModel shell, IActivator activator, IEventBubbleSubscription bubble)
    : VmToolkit(logger, host, shell, activator, bubble)
{
    public new ILogger<T> Logger { get; } = logger;
}