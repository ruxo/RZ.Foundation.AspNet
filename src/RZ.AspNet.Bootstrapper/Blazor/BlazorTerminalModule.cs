using System.Reflection;

namespace RZ.AspNet.Blazor;

public enum BlazorSetupMode { Static, Server, WebAssembly, HostedWebAssembly }

public class BlazorTerminalModule<TApp>(BlazorSetupMode mode = BlazorSetupMode.Server, IEnumerable<Assembly>? additionalAssemblies = null) : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services
               .AddRazorComponents()
               .AddInteractiveServerComponents();
        return base.InstallServices(builder);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        if (mode is BlazorSetupMode.WebAssembly or BlazorSetupMode.HostedWebAssembly)
            throw new NotSupportedException("Use another module for WebAssembly setup");

        var razorMap = app.MapRazorComponents<TApp>();

        var additional = additionalAssemblies?.ToArray() ?? [];
        if (additional.Length != 0)
            razorMap = razorMap.AddAdditionalAssemblies(additional);

        if (mode is BlazorSetupMode.Server)
            razorMap.AddInteractiveServerRenderMode();
        return base.InstallMiddleware(configuration, app);
    }
}