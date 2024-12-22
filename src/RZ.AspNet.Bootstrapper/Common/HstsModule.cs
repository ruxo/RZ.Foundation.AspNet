namespace RZ.AspNet.Common;

public class HstsModule : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services
               .AddHsts(opts => {
                    opts.Preload = true;
                    opts.IncludeSubDomains = true;
                });
        return base.InstallServices(builder);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseHttpsRedirection();
        return new(unit);
    }
}