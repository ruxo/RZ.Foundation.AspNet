using RZ.AspNet.Common;

namespace RZ.AspNet;

[PublicAPI]
public static class CommonModules
{
    public static AppModule Localization(string resourcesPath = "Resources", string defaultCulture = "en-US", params string[] supportedCultures)
        => new LocalizationModule(resourcesPath, defaultCulture, supportedCultures);

    public static AppModule AntiForgery() => AntiForgeryModule.Default;

    public static AppModule ForwardHeaders(bool forwardAll = true) => new HeaderForwardingModule(forwardAll);
}