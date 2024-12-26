using Microsoft.AspNetCore.Hosting;

namespace RZ.AspNet.Blazor;

public class AssetMapModule(bool useAssetInAllNonProdEnvironments = true) : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        if (useAssetInAllNonProdEnvironments && !builder.Environment.IsDevelopment() && !builder.Environment.IsProduction())
            (builder as WebApplicationBuilder)?.WebHost.UseStaticWebAssets();
        return base.InstallServices(builder);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.MapStaticAssets();
        return base.InstallMiddleware(configuration, app);
    }
}