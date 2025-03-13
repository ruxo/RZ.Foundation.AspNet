using System.Globalization;
using Microsoft.Extensions.Localization;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Blazor.Server.Example.Components.Home;

partial class WelcomeView(IStringLocalizer<Translation> localizer)
{
    string lang = "th";

    void SetLanguage(string language) {
        lang = language;
    }
}

public sealed class WelcomeViewModel : ViewModel
{
    public WelcomeViewModel(IStringLocalizer<Translation> localizer, ShellViewModel shell) {
        shell.NavBarMode = NavBarMode.New(NavBarType.Full);

        var current = CultureInfo.CurrentUICulture;

        CultureInfo.CurrentUICulture = new("th");
        HomeTh = localizer["home"];

        CultureInfo.CurrentUICulture = new("en");
        HomeEn = localizer["home"];

        CultureInfo.CurrentUICulture = current;
    }

    public string HomeTh { get; }
    public string HomeEn { get; }
}