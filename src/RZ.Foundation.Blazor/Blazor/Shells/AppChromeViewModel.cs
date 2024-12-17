using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation.Blazor.Shells;

public enum AppBarDisplayMode
{
    Page, Modal, Stacked
}

public partial class AppChromeViewModel : ViewModel
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

    [ObservableProperty] public partial bool IsDrawerOpen { get; set; }
    [ObservableProperty] public partial bool UseMiniDrawer { get; set; }
    [ObservableProperty] public partial Navigation[] SidebarNavItems { get; set; } = [];
    [ObservableProperty] public partial AppBarDisplayMode AppBarMode { get; set; } = AppBarDisplayMode.Page;

    public bool IconOnly => iconOnly.Value;
    public bool IsDrawerVisible => isDrawerVisible.Value;
    public bool ShowOnHover => showOnHover.Value;

    public ReactiveCommand<RUnit, RUnit> AppBarClicked { get; }
}