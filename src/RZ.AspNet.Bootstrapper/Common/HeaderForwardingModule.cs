using Microsoft.AspNetCore.HttpOverrides;

namespace RZ.AspNet.Common;

[PublicAPI]
public class HeaderForwardingModule(bool forwardAll = true) : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        if (forwardAll)
            builder.Services.Configure<ForwardedHeadersOptions>(opts => opts.ForwardedHeaders = ForwardedHeaders.All);
        return new(unit);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.UseForwardedHeaders();
        return new(unit);
    }
}