namespace RZ.AspNet.Blazor;

public class AssetMapModule : AppModule
{
    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.MapStaticAssets();
        return base.InstallMiddleware(configuration, app);
    }
}