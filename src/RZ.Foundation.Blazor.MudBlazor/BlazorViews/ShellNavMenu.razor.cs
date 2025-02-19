using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using LanguageExt.UnitsOfMeasure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using ReactiveUI;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.BlazorViews;

[UsedImplicitly]
partial class ShellNavMenu : ITrackBlurRequirements
{
    readonly IServiceProvider sp;
    DotNetObjectReference<ShellNavMenu> me = null!;

    ElementReference drawer;
    CompositeDisposable disposables = new();

    bool blurTracking;

    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public RenderFragment? Header { get; set; }
    [Parameter] public RenderFragment? Footer { get; set; }

    [Inject] public required ILogger<ShellNavMenu> Logger { get; set; }
    [Inject] public required NavigationManager NavManager { get; set; }

    public ShellNavMenu(IServiceProvider sp) {
        this.sp = sp;
        ViewModel = sp.Create<ShellNavMenuViewModel>();
        this.WhenActivated(d => {
            me = DotNetObjectReference.Create(this).DisposeWith(d);
            this.WhenAnyValue(x => x.ViewModel!.IsDrawerVisible)
                .Where(identity)
                .Subscribe(_ => blurTracking = false)
                .DisposeWith(d);

            ViewModel!.DisposeWith(d);
            disposables.DisposeWith(d);
        });
    }

    [JSInvokable]
    public void OnBlur() {
        if (ViewModel!.IsDrawerOpen)
            ViewModel.IsDrawerOpen = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (!firstRender && !blurTracking){
            blurTracking = true;

            this.WhenAnyValue(x => x.ViewModel!.IsDrawerOpen)
                .Where(identity)
                .Subscribe(async void (_) => {
                     var (e, _) = await Try(drawer.FocusAsync().AsTask());
                     if (e is not null)
                         Logger.LogWarning(e, "Cannot focus drawer");
                 })
                .DisposeWith(disposables);

            await using var js = sp.GetRequiredService<RzBlazorJsInterop>();
            await js.TrackBlur(drawer, me);
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}

public class ShellNavMenuViewModel : ViewModel, IDisposable
{
    readonly CompositeDisposable disposables = new();
    readonly ObservableAsPropertyHelper<DrawerVariant> variant;

    readonly AppChromeViewModel chrome;

    public ShellNavMenuViewModel(ILogger<ShellNavMenuViewModel> logger, AppChromeViewModel chrome) {
        this.chrome = chrome;
        logger.LogDebug("Creating ShellNavMenuViewModel {Id} with Shell ID {ShellId}", ViewModelId, chrome.ViewModelId);

        variant = chrome.WhenAnyValue(x => x.UseMiniDrawer)
                       .Select(x => x ? DrawerVariant.Mini : DrawerVariant.Temporary)
                       .ToProperty(this, x => x.Variant)
                       .DisposeWith(disposables);

        ForwardPropertyEvents(chrome, nameof(IsDrawerOpen), nameof(IsDrawerVisible), nameof(ShowOnHover), nameof(IconOnly))
           .DisposeWith(disposables);
    }

    public void Dispose() {
        disposables.Dispose();
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