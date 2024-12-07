using Microsoft.Extensions.DependencyInjection;

namespace RZ.Foundation.Blazor.MVVM;

public interface IViewModelFactory
{
    T Create<T>(params object[] args) where T : ViewModel;
}

public sealed class ViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
    public T Create<T>(params object[] args) where T : ViewModel {
        var instance = ActivatorUtilities.CreateInstance<T>(serviceProvider, args);

        // This is for F#-style initialization which needs the instance to be fully constructed before calling its properties
        instance.Initialize();
        return instance;
    }
}