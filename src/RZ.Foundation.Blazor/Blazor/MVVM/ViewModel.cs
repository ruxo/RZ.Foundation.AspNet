using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using ReactiveUI;

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

[PublicAPI]
public abstract class ActivatableViewModel : ViewModel, IActivatableViewModel
{
    public ActivatableViewModel() {
        this.WhenActivated(OnActivated);
    }

    public ViewModelActivator Activator { get; } = new();

    protected abstract void OnActivated(CompositeDisposable disposables);
}