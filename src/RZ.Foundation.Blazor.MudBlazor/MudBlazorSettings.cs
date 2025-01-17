using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace RZ.Foundation;

[PublicAPI]
public static class MudBlazorSettings
{
    public static IServiceCollection AddRzMudBlazorSettings(this IServiceCollection services)
        => services.AddRzBlazorSettings()
                   .AddScoped<RzBlazorJsInterop>();
}