global using RUnit = System.Reactive.Unit;
using MudBlazor.Services;
using RZ.Blazor.Server.Example.Components;
using RZ.Blazor.Server.Example.Components.Home;
using RZ.Blazor.Server.Example.Components.ShellExample;
using RZ.Foundation;
using RZ.Foundation.Blazor.Views;

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddScoped<WelcomeViewModel>()
       .AddScoped<ContentViewModel>()
       .AddScoped<BlankContentViewModel>();

builder.Services
       .AddMudServices()
       .AddRzBlazorSettings()
       .AddRazorComponents()
       .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
