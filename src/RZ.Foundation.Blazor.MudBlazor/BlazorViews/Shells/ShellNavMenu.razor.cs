using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using MudBlazor;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.BlazorViews.Shells;

public class ShellNavMenuViewModel : ViewModel, IDisposable
{
    readonly ObservableAsPropertyHelper<DrawerVariant> variant;
    readonly ObservableAsPropertyHelper<bool> showOnHover;
    readonly ObservableAsPropertyHelper<bool> iconOnly;
    readonly ObservableAsPropertyHelper<bool> isDrawerVisible;

    readonly ILogger<ShellNavMenuViewModel> logger;
    readonly ShellViewModel shell;
    readonly CompositeDisposable disposables = new();

    public ShellNavMenuViewModel(ILogger<ShellNavMenuViewModel> logger, ShellViewModel shell) {
        this.logger = logger;
        this.shell = shell;
        logger.LogDebug("Creating ShellNavMenuViewModel {Id} with Shell ID {ShellId}", Id, shell.Id);

        isDrawerVisible = shell.WhenAnyValue(x => x.IsDrawerVisible)
                              .ToProperty(this, x => x.IsDrawerVisible)
                              .DisposeWith(disposables);
        variant = shell.WhenAnyValue(x => x.UseMiniDrawer)
                       .Select(x => x ? DrawerVariant.Mini : DrawerVariant.Temporary)
                       .ToProperty(this, x => x.Variant)
                       .DisposeWith(disposables);
        showOnHover = shell.WhenAnyValue(x => x.UseMiniDrawer)
                           .ToProperty(this, x => x.ShowOnHover)
                           .DisposeWith(disposables);
        iconOnly = shell.WhenAnyValue(x => x.UseMiniDrawer,
                                      x => x.IsDrawerOpen,
                                      (useMiniDrawer, isDrawerOpen) => useMiniDrawer && !isDrawerOpen)
                        .ToProperty(this, x => x.IconOnly)
                        .DisposeWith(disposables);
    }

    public void Dispose() {
        logger.LogDebug("Disposing ShellNavMenuViewModel {Id}", Id);
        disposables.Dispose();
    }

    public DrawerVariant Variant => variant.Value;
    public bool ShowOnHover => showOnHover.Value;
    public bool IconOnly => iconOnly.Value;

    public bool IsDrawerOpen
    {
        get => shell.IsDrawerOpen;
        set
        {
            this.RaisePropertyChanging();
            shell.IsDrawerOpen = value;
            this.RaisePropertyChanged();
        }
    }

    public bool IsDrawerVisible => isDrawerVisible.Value;
}