using Microsoft.AspNetCore.ResponseCompression;

namespace RZ.AspNet.Common;

/// <summary>
/// This module is designed to use with SignalR communication so HTTP is expected for compression. Turning HTTPS compression on can
/// subject the application to CRIME/BREACH attacks.
/// </summary>
/// <param name="forHttps"></param>
public class ResponseCompressionModule(bool forHttps = false) : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services.AddResponseCompression(opts => {
            opts.EnableForHttps = forHttps;
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
        });
        return base.InstallServices(builder);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.UseResponseCompression();
        return base.InstallMiddleware(configuration, app);
    }
}