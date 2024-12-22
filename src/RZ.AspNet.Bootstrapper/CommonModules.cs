using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using RZ.AspNet.Common;

namespace RZ.AspNet;

[PublicAPI]
public static class CommonModules
{
    public static AppModule Localization(string resourcesPath = "Resources", string defaultCulture = "en-US", params string[] supportedCultures)
        => new LocalizationModule(resourcesPath, defaultCulture, supportedCultures);

    public static AppModule AntiForgery() => AntiForgeryModule.Default;

    public static AppModule EnforceHsts() => new HstsModule();
    public static AppModule EnableCompression(bool forHttps = false) => new ResponseCompressionModule(forHttps);
    public static AppModule ForwardHeaders(bool forwardAll = true) => new HeaderForwardingModule(forwardAll);

    public static AppModule ConfigureAuthentication(Action<AuthorizationOptions> authOptions, params Action<IHostApplicationBuilder>[] authBuilders)
        => new AuthorizationModuleBase(authOptions, authBuilders);

    public static AppModule ConfigureAuthentication(params Action<IHostApplicationBuilder>[] authBuilder)
        => new AuthorizationModuleBase(authOptions: null, authBuilder);

    public static AppModule AddSignalR() => new SignalRModule();

    public static Action<IHostApplicationBuilder> Cookie(Action<CookieAuthenticationOptions>? opts = null) => builder => {
        var auth = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
        if (opts is not null) auth.AddCookie(opts);
    };
}