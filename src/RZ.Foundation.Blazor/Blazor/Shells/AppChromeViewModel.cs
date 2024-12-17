using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation.Blazor.Shells;

public enum AppBarDisplayMode
{
    Page, Modal, Stacked
}

public class AppChromeViewModel : ViewModel
{
    readonly ObservableAsPropertyHelper<bool> iconOnly;
    readonly ObservableAsPropertyHelper<bool> isDrawerVisible;
    readonly ObservableAsPropertyHelper<bool> showOnHover;

    public AppChromeViewModel() {
        iconOnly = this.WhenAnyValue(x => x.UseMiniDrawer,
                                     x => x.IsDrawerOpen,
                                     (useMiniDrawer, isDrawerOpen) => useMiniDrawer && !isDrawerOpen)
                       .ToProperty(this, x => x.IconOnly);
        showOnHover = this.WhenAnyValue(x => x.UseMiniDrawer).ToProperty(this, x => x.ShowOnHover);

        isDrawerVisible = this.WhenAnyValue(x => x.AppBarMode, mode => mode is not AppBarDisplayMode.Modal)
                              .ToProperty(this, x => x.IsDrawerVisible);

        AppBarClicked = ReactiveCommand.Create(() => {
            if (AppBarMode == AppBarDisplayMode.Page)
                IsDrawerOpen = !IsDrawerOpen;
        });
    }

    public bool IsDrawerOpen
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool UseMiniDrawer
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Navigation[] SidebarNavItems
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = [];

    public AppBarDisplayMode AppBarMode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = AppBarDisplayMode.Page;

    public bool IconOnly => iconOnly.Value;
    public bool IsDrawerVisible => isDrawerVisible.Value;
    public bool ShowOnHover => showOnHover.Value;

    public ReactiveCommand<RUnit, RUnit> AppBarClicked { get; }
}