using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ReactiveUI;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.BlazorViews;

[UsedImplicitly]
partial class ShellAppBar
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? Title { get; set; }
    [Parameter] public RenderFragment? Actions { get; set; }

    public ShellAppBar(IServiceProvider sp) {
        ViewModel = sp.Create<ShellAppBarViewModel>();
    }
}

public class ShellAppBarViewModel : ViewModel, IActivatableViewModel
{
    readonly ObservableAsPropertyHelper<string> icon;

    public ShellAppBarViewModel(AppChromeViewModel chrome) {
        var disposables = new CompositeDisposable();

        icon = chrome.WhenAnyValue(x => x.AppBarMode)
                     .Select(x => x switch {
                          AppBarDisplayMode.Page    => Icons.Material.Filled.Menu,
                          AppBarDisplayMode.Modal   => Icons.Material.Filled.Close,
                          AppBarDisplayMode.Stacked => Icons.Material.Filled.ArrowBack,
                          _                         => Icons.Material.Filled.Cancel
                      })
                     .ToProperty(this, x => x.Icon)
                     .DisposeWith(disposables);

        Clicked = chrome.AppBarClicked;

        this.WhenActivated(d => disposables.DisposeWith(d));
    }

    public string Icon => icon.Value;

    public ReactiveCommand<RUnit, RUnit> Clicked { get; }

    ViewModelActivator IActivatableViewModel.Activator { get; } = new();
}