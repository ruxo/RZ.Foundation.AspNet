using RZ.Foundation.Json;

namespace RZ.AspNet.Common;

public class SignalRModule : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services
               .AddSignalR()
               .AddJsonProtocol(opts => opts.PayloadSerializerOptions.UseRzRecommendedSettings());
        return base.InstallServices(builder);
    }
}