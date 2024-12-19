using System.Collections;
using System.Diagnostics;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Views;
using RZ.Foundation.Types;

namespace RZ.Foundation.Blazor.Shells;

[PublicAPI]
public class ShellViewModel : ViewModel, IEnumerable<ViewState>
{
    readonly ILogger<ShellViewModel> logger;
    readonly TimeProvider clock;
    readonly IActivator activator;
    readonly Stack<ViewState> content = [];
    readonly Subject<NotificationEvent> notifications = new();
    const int MaxNotifications = 20;

    NavBarMode navBarMode;

    public ShellViewModel(ILogger<ShellViewModel> logger, TimeProvider clock, IActivator activator) {
        this.logger = logger;
        this.clock = clock;
        this.activator = activator;

        logger.LogDebug("Initializing ShellViewModel {Id}", Id);

        navBarMode = NavBarMode.New(NavBarType.Full);

        ToggleDrawer = ReactiveCommand.Create(() => { IsDrawerOpen = !IsDrawerOpen; });
    }

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

    #endregion

    public AppMode AppMode => content.TryPeek(out var v)? v.AppMode : AppMode.Page.Default;
    public ViewModel? Content => content.TryPeek(out var v)? v.Content : null;
    public int StackCount => content.Count;

    public IObservable<NotificationEvent> Notifications => notifications;

    public ReactiveCommand<RUnit, RUnit> ToggleDrawer { get; }

    public void CloseCurrentView()
        => ChangingStack(() => content.Pop());

    public NotificationMessage Notify(NotificationMessage message) {
        notifications.OnNext(new(clock.GetLocalNow(), message.Severity, message.Message));
        return message;
    }

    public void CloneState(Func<ViewState, ViewState> stateBuilder) => ChangingStack(() => {
        var viewState = content.TryPeek(out var v) ? v : new ViewState(AppMode.Page.Default, BlankContentViewModel.Instance);
        var newState = stateBuilder(viewState);
        content.Push(newState);
    });

    public void PushModal(ViewModel? viewModel, Func<AppMode, AppMode> appModeGetter) {
        if (viewModel is null && StackCount == 0)
            throw new ErrorInfoException(StandardErrorCodes.InvalidRequest, "Push a null model while the stack is empty");

        ChangingStack(() => {
            var current = Content;
            var appMode = appModeGetter(AppMode);

            Debug.Assert(current is not null || viewModel is not null);
            content.Push(new ViewState(AppMode: appMode, Content: (viewModel ?? current)!));
        });
    }

    public void Push(ViewModel viewModel)
        => CloneState(current => current with { Content = viewModel });

    public void PushModal(ViewModel? viewModel = null, ReactiveCommand<RUnit, RUnit>? onClose = null) {
        onClose ??= ReactiveCommand.Create(CloseCurrentView);
        PushModal(viewModel, _ => new AppMode.Modal(onClose));
    }

    public Unit Replace(ViewModel replacement, AppMode? appMode = null) {
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

    void ChangingStack(Action action) {
        Content?.ViewOffScreen();

        this.RaisePropertyChanging(nameof(Content));
        this.RaisePropertyChanging(nameof(AppMode));
        this.RaisePropertyChanging(nameof(StackCount));
        action();
        this.RaisePropertyChanged(nameof(StackCount));
        this.RaisePropertyChanged(nameof(AppMode));
        this.RaisePropertyChanged(nameof(Content));

        Content?.ViewOnScreen();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed record ViewState(AppMode AppMode, ViewModel Content);
