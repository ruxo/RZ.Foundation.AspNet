﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Views;

namespace RZ.Foundation.Blazor.Shells;

[PublicAPI]
public class ShellViewModel : ViewModel, IEnumerable<ViewState>
{
    readonly ILogger<ShellViewModel> logger;
    readonly TimeProvider clock;
    readonly IActivator activator;
    readonly ShellOptions options;
    readonly Stack<ViewState> content = [];
    readonly Subject<NotificationEvent> notifications = new();
    readonly ObservableAsPropertyHelper<bool> isDrawerVisible;
    readonly ObservableAsPropertyHelper<bool> useMiniDrawer;
    const int MaxNotifications = 20;

    NavBarMode navBarMode;

    public ShellViewModel(ILogger<ShellViewModel> logger, TimeProvider clock,
                          IActivator activator, ShellOptions options) {
        this.logger = logger;
        this.clock = clock;
        this.activator = activator;
        this.options = options;

        logger.LogDebug("Initializing ShellViewModel {Id}", Id);

        navBarMode = options.InitialNavBar;
        isDrawerVisible = this.WhenAnyValue(x => x.NavBarMode,
                                            x => x.AppMode,
                                            x => x.StackCount,
                                            (navBar, app, stackCount) => app is AppMode.Page
                                                                      && stackCount == 1
                                                                      && navBar.Type is NavBarType.Full or NavBarType.Mini
                                                                      && navBar.Visible)
                              .ToProperty(this, x => x.IsDrawerVisible);
        useMiniDrawer = this.WhenAnyValue(x => x.NavBarMode).Select(m => m.Type == NavBarType.Mini).ToProperty(this, x => x.UseMiniDrawer);

        NavItems = new(options.Navigation);
    }

    public string BasePath => options.BasePath;

    public bool IsDarkMode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #region Nav Bar

    public NavBarMode NavBarMode
    {
        get => navBarMode;
        set => this.RaiseAndSetIfChanged(ref navBarMode, value);
    }

    public bool IsDrawerOpen
    {
        get => navBarMode.Expanded;
        set
        {
            if (NavBarMode.Expanded == value)
                return;
            this.RaisePropertyChanging();
            NavBarMode = NavBarMode with { Expanded = value };
            this.RaisePropertyChanged();
        }
    }

    public bool IsDrawerVisible => isDrawerVisible.Value;

    public bool UseMiniDrawer => useMiniDrawer.Value;

    #endregion

    public AppMode AppMode => content.TryPeek(out var v)? v.AppMode : AppMode.Page.Default;
    public ViewModel Content => content.TryPeek(out var v)? v.Content : BlankContentViewModel.Instance;
    public int StackCount => content.Count;

    public ObservableCollection<Navigation> NavItems { get; }

    public IObservable<NotificationEvent> Notifications => notifications;

    public ReactiveCommand<RUnit, RUnit> ToggleDrawer => ReactiveCommand.Create(() => { IsDrawerOpen = !IsDrawerOpen; });

    public string? GetShellPath(string navigationPath) {
        var truePath = navigationPath.StartsWith(BasePath) ? navigationPath[BasePath.Length..] : null;
        if (truePath?.Length == 0) truePath = "/";
        return truePath is not null && truePath.StartsWith('/')? truePath : null;
    }

    public Unit CloseCurrentView()
        => ChangingStack(() => content.Pop());

    public NotificationMessage Notify(NotificationMessage message) {
        notifications.OnNext(new(clock.GetLocalNow(), message.Severity, message.Message));
        return message;
    }

    public Unit CloneState(Func<ViewState, ViewState> stateBuilder) => ChangingStack(() => {
        var newState = stateBuilder(content.Peek());
        content.Push(newState);
    });

    public Unit PushModal(ViewModel? viewModel, Func<AppMode, AppMode> appModeGetter) => ChangingStack(() => {
        var current = content.Peek();
        var appMode = appModeGetter(current.AppMode);

        content.Push(new ViewState(AppMode: appMode, Content: viewModel ?? current.Content));
    });

    public Unit Push(ViewModel viewModel)
        => CloneState(current => current with { Content = viewModel });

    public Unit PushModal(ViewModel? viewModel = null, ReactiveCommand<RUnit, RUnit>? onClose = default) {
        onClose ??= ReactiveCommand.Create(() => {
            CloseCurrentView();
        });
        return PushModal(viewModel, _ => new AppMode.Modal(onClose));
    }

    public Unit Replace(ViewModel replacement, AppMode? appMode = default) {
        this.RaisePropertyChanging(nameof(Content));
        if (appMode is not null)
            this.RaisePropertyChanging(nameof(AppMode));
        var current = content.Pop();
        content.Push(new ViewState(Content: replacement, AppMode: appMode ?? current.AppMode));
        if (appMode is not null)
            this.RaisePropertyChanged(nameof(AppMode));
        this.RaisePropertyChanged(nameof(Content));
        return unit;
    }

    public IEnumerator<ViewState> GetEnumerator() => content.GetEnumerator();

    Unit ChangingStack(Action action) {
        if (content.Count > 0)
            content.Peek().Content.ViewOffScreen();

        this.RaisePropertyChanging(nameof(Content));
        this.RaisePropertyChanging(nameof(AppMode));
        this.RaisePropertyChanging(nameof(StackCount));
        this.RaisePropertyChanging(nameof(IsDrawerVisible));
        action();
        this.RaisePropertyChanged(nameof(IsDrawerVisible));
        this.RaisePropertyChanged(nameof(StackCount));
        this.RaisePropertyChanged(nameof(AppMode));
        this.RaisePropertyChanged(nameof(Content));

        content.Peek().Content.ViewOnScreen();
        return unit;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool NavigateTo(string urlPath) {
        var current = content.TryPeek(out var v) ? v : null;

        if (current is null)
            logger.LogDebug("Shell has no view");
        else
            logger.LogDebug("Shell content is {@Content}", current.Content);

        var navItem = options.Navigation.OfType<Navigation.Item>().FirstOrDefault(x => x.NavPath == urlPath);
        if (navItem is null){
            logger.LogWarning("No navigation item found for path {Path}", urlPath);
            return false;
        }
        logger.LogDebug("Navigating to {Path} for menu [{Title}]", urlPath, navItem.Title);

        ChangingStack(() => {
            content.Clear();
            var view = navItem.View.Invoke(activator);

            logger.LogDebug("Push view for {Path}", urlPath);
            content.Push(new(AppMode.Page.Default, view));
        });
        return true;
    }

    public void NavigateToNotFound() {
        ChangingStack(() => {
            content.Clear();
            content.Push(new(AppMode.Page.Default, BlankContentViewModel.Instance));
        });
    }
}

public sealed record ViewState(AppMode AppMode, ViewModel Content);
