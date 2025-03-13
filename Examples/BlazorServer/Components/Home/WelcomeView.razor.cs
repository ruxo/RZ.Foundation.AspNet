using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Blazor.Server.Example.Components.Home;

partial class WelcomeView(IStringLocalizer<Translation> localizer, NavigationManager nav)
{
    void SetLanguage(string lang) {
        Console.WriteLine($"Change language to '{lang}'");
        nav.NavigateTo($"/language/{lang}?returnUrl={Uri.EscapeDataString(nav.Uri)}", forceLoad: true);
    }
}

public sealed class WelcomeViewModel : ViewModel
{
    public WelcomeViewModel(ShellViewModel shell) {
        shell.NavBarMode = NavBarMode.New(NavBarType.Full);
    }
}