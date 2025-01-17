using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RZ.Foundation.Blazor;

namespace RZ.Foundation;

interface ITrackBlurRequirements
{
    [UsedImplicitly]
    void OnBlur();
}

class RzBlazorJsInterop(IJSRuntime js) : JsInteropBase(js, "./_content/RZ.Foundation.Blazor.MudBlazor/BlazorViews/ShellNavMenu.razor.js")
{
    public async ValueTask TrackBlur<T>(ElementReference el, DotNetObjectReference<T> netObject) where T: class, ITrackBlurRequirements
        => await InvokeVoidAsync("trackBlur", el, netObject);
}