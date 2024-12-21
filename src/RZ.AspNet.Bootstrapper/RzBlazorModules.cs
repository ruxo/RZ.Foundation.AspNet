using RZ.AspNet;

namespace RZ.Foundation;

[PublicAPI]
public static class RzBlazorModules
{
    public static AppModule HandleErrors(string path404 = "/") => new AspNet.Blazor.ErrorHandlerModule(path404);
    public static AppModule EnforceHsts() => new AspNet.Blazor.HstsModule();
    public static AppModule ExposeMappedAssets() => new AspNet.Blazor.AssetMapModule();

    public static AppModule UseBlazorServer<TApp>() => new AspNet.Blazor.BlazorTerminalModule<TApp>();
}