using System.Reactive.Disposables;
using System.Reactive.Linq;
using MudBlazor;
using ReactiveUI;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.BlazorViews;

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