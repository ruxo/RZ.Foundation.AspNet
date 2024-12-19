using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RZ.Foundation.Blazor.Shells;
using RZ.Foundation.Types;

namespace RZ.Foundation.Blazor.MVVM;

[PublicAPI]
public abstract class ViewModel : ReactiveObject, IDisposable
{
    public string Id => GetHashCode().ToString();

    Lazy<CompositeDisposable> disposables = new(() => new());

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

    public virtual void Dispose() {
        if (disposables.IsValueCreated)
            Disposables.Dispose();
    }

    protected CompositeDisposable Disposables => disposables.Value;
}

public abstract class AppViewModel(IHostEnvironment host, ShellViewModel shell, ILogger? logger = null) : ViewModel
{
    protected void RunBackground(ValueTask init, Action<ViewStatus>? setStatus = null, Func<Exception, string>? translator = null) {
        Task.Run(async () => {
            try{
                await init;
                setStatus?.Invoke(ViewStatus.Ready.Instance);
            }
            catch (Exception e){
                var error = ErrorFrom.Exception(e);
                if (logger is null)
                    Trace.WriteLine($"TrapErrors: {error}");
                else if (host.IsDevelopment())
                    logger.LogError("TrapErrors: {@Error}", error);
                else
                    logger.LogError(e, "TrapErrors: [{Code}] {Message}", error.Code, error.Message);

                shell.Notify(new(MessageSeverity.Error, translator?.Invoke(e) ?? error.Message));
                setStatus?.Invoke(new ViewStatus.Failed(error));
            }
        });
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