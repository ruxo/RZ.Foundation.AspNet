using Microsoft.AspNetCore.Authorization;

namespace RZ.AspNet.Common;

[PublicAPI]
public class AuthorizationModuleBase(Action<AuthorizationOptions>? authOptions, Action<IHostApplicationBuilder>[] authBuilders) : AppModule
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        if (authOptions is not null)
            builder.Services.AddAuthorizationCore(authOptions);

        authBuilders.Iter(b => b(builder));
        return base.InstallServices(builder);
    }

    public override ValueTask<Unit> InstallMiddleware(IHostApplicationBuilder configuration, WebApplication app) {
        app.UseAuthentication();
        app.UseAuthorization();
        return new(unit);
    }
}