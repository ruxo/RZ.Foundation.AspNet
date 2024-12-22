using Microsoft.AspNetCore.Authorization;
using RZ.AspNet.Common;

namespace RZ.AspNet.Blazor;

public class BlazorAuthModule(Action<AuthorizationOptions>? authOptions, Action<IHostApplicationBuilder>[] authBuilders)
    : AuthorizationModuleBase(authOptions, authBuilders)
{
    public override ValueTask<Unit> InstallServices(IHostApplicationBuilder builder) {
        builder.Services.AddCascadingAuthenticationState();
        return base.InstallServices(builder);
    }
}