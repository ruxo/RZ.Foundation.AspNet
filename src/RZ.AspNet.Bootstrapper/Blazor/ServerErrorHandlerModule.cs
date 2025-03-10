﻿namespace RZ.AspNet.Common;

public class ServerErrorHandlerModule(string path404 = "/") : AppModule
{
    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        if (app.Environment.IsDevelopment()){
            app.UseStatusCodePages();
        }
        else{
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseStatusCodePages(ctx => {
                if (ctx.HttpContext.Response.StatusCode == 404)
                    ctx.HttpContext.Response.Redirect(path404);
                return Task.CompletedTask;
            });
        }
        return new(unit);
    }
}
