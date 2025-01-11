using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using RZ.AspNet;
using RZ.AspNet.Blazor;
using RZ.AspNet.Common;

namespace RZ.Foundation;

[PublicAPI]
public static class RzBlazorModules
{
    public static AppModule ExposeMappedAssets() => new AssetMapModule();

    public static AppModule HandleBlazorErrors(string path404 = "/") => new ServerErrorHandlerModule(path404);

    public static AppModule UseBlazorServer<TApp>(params Assembly[] additionalAssemblies)
        => new BlazorTerminalModule<TApp>(additionalAssemblies: additionalAssemblies);

    public static AppModule UseBlazorServer<TApp>(BlazorSetupMode mode = BlazorSetupMode.Server, params Assembly[] additionalAssemblies)
        => new BlazorTerminalModule<TApp>(mode, additionalAssemblies: additionalAssemblies);

    public static AppModule ConfigureAuthentication(Action<AuthorizationOptions> authOptions, params Action<IHostApplicationBuilder>[] authBuilders)
        => new BlazorAuthModule(authOptions, authBuilders);

    public static AppModule ConfigureAuthentication(params Action<IHostApplicationBuilder>[] authBuilders)
        => new BlazorAuthModule(authOptions: null, authBuilders);
}