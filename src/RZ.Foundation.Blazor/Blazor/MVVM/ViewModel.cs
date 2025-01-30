using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RZ.Foundation.Blazor.Shells;
using RZ.Foundation.Types;

namespace RZ.Foundation.Blazor.MVVM;

[PublicAPI]
public abstract class ViewModel : ReactiveObject
{
    public string ViewModelId => GetHashCode().ToString();

    public virtual void ViewOnScreen() {}
    public virtual void ViewOffScreen() {}

    public IDisposable ForwardPropertyEvents(ViewModel another, params string[] properties) {
        var changing = Observable.FromEventPattern<PropertyChangingEventHandler, PropertyChangingEventArgs>(
                                      h => another.PropertyChanging += h,
                                      h => another.PropertyChanging -= h)
                                 .Where(a => properties.Contains(a.EventArgs.PropertyName))
                                 .Subscribe(a => this.RaisePropertyChanging(a.EventArgs.PropertyName));
        var changed = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                     h => another.PropertyChanged += h,
                                     h => another.PropertyChanged -= h)
                                .Where(a => properties.Contains(a.EventArgs.PropertyName))
                                .Subscribe(a => this.RaisePropertyChanged(a.EventArgs.PropertyName));
        return new CompositeDisposable(changing, changed);
    }
}

[PublicAPI]
public abstract class AppViewModel(VmToolkit tool) : ViewModel, IActivatableViewModel
{
    ViewModelActivator IActivatableViewModel.Activator { get; } = new();

    protected IServiceProvider Services => tool.Services;
    protected ILogger Logger => tool.Logger;
    protected IHostEnvironment Host => tool.Host;
    protected ShellViewModel Shell => tool.Shell;
    protected IEventBubbleSubscription Bubble => tool.Bubble;

    protected void RunBackground(Func<Task> init, Action<ViewStatus>? setStatus = null, Func<Exception, string>? translator = null, [CallerMemberName] string? caller = null) {
        Task.Run(() => RunInit(init, setStatus, translator, caller));
    }

    protected async Task RunInit(Func<Task> init, Action<ViewStatus>? setStatus = null, Func<Exception, string>? translator = null, [CallerMemberName] string? caller = null) {
        try{
            await init();
            setStatus?.Invoke(ViewStatus.Ready.Instance);
        }
        catch (Exception e){
            var error = LogError(e, caller);

            tool.Shell.Notify(new(MessageSeverity.Error, translator?.Invoke(e) ?? error.Message));
            setStatus?.Invoke(new ViewStatus.Failed(error));
        }
    }

    protected async ValueTask Execute(ValueTask task, Action<bool>? setProcessing = null, Func<Exception, string>? translator = null, [CallerMemberName] string? caller = null) {
        setProcessing?.Invoke(true);
        // Save the subscription
        try{
            await task;
        }
        catch (Exception e){
            var error = LogError(e, caller);

            tool.Shell.Notify(new(MessageSeverity.Error, translator?.Invoke(e) ?? error.Message));
        }
        finally{
            setProcessing?.Invoke(false);
        }
    }

    protected ErrorInfo LogError(Exception e, [CallerMemberName] string? caller = null) {
        var error = ErrorFrom.Exception(e);
        if (tool.Host.IsDevelopment())
            tool.Logger.LogError("{Caller}: {@Error}", caller, error);
        else
            tool.Logger.LogError(e, "{Caller}: [{Code}] {Message}", caller, error.Code, error.Message);
        return error;
    }
}

[PublicAPI]
public abstract class ActivatableViewModel : ViewModel, IActivatableViewModel
{
    public ActivatableViewModel() {
        this.WhenActivated(OnActivated);
    }

    public ViewModelActivator Activator { get; } = new();

    protected abstract void OnActivated(CompositeDisposable disposables);
}