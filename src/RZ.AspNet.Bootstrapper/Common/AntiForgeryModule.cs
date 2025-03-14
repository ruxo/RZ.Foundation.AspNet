﻿namespace RZ.AspNet.Common;

[PublicAPI]
public class AntiForgeryModule : AppModule
{
    public static readonly AntiForgeryModule Default = new();

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.UseAntiforgery();
        return new(unit);
    }
}