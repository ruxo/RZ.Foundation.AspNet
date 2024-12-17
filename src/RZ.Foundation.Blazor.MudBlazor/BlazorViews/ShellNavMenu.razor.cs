using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using MudBlazor;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.BlazorViews;

public class ShellNavMenuViewModel : ViewModel
{
    readonly ObservableAsPropertyHelper<DrawerVariant> variant;

    readonly AppChromeViewModel chrome;

    public ShellNavMenuViewModel(ILogger<ShellNavMenuViewModel> logger, AppChromeViewModel chrome) {
        this.chrome = chrome;
        logger.LogDebug("Creating ShellNavMenuViewModel {Id} with Shell ID {ShellId}", Id, chrome.Id);

        variant = chrome.WhenAnyValue(x => x.UseMiniDrawer)
                       .Select(x => x ? DrawerVariant.Mini : DrawerVariant.Temporary)
                       .ToProperty(this, x => x.Variant)
                       .DisposeWith(Disposables);

        ForwardPropertyEvents(chrome, nameof(IsDrawerOpen), nameof(IsDrawerVisible), nameof(ShowOnHover), nameof(IconOnly))
           .DisposeWith(Disposables);
    }

    public DrawerVariant Variant => variant.Value;
    public bool ShowOnHover => chrome.ShowOnHover;
    public bool IconOnly => chrome.IconOnly;

    public bool IsDrawerOpen
    {
        get => chrome.IsDrawerOpen;
        set => chrome.IsDrawerOpen = value;
    }

    public bool IsDrawerVisible => chrome.IsDrawerVisible;
}