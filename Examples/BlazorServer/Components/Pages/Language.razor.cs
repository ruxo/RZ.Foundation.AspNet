using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Localization;

namespace RZ.Blazor.Server.Example.Components.Pages;

[UsedImplicitly]
partial class Language(NavigationManager nav)
{
    [Parameter] public required string Lang { get; set; }

    [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

    [CascadingParameter] public required HttpContext HttpContext { get; set; }

    protected override void OnInitialized() {
        Console.WriteLine($"Return url: {ReturnUrl}");
        HttpContext.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(Lang, Lang)
            ));
        nav.NavigateTo(ReturnUrl ?? "/");
    }
}