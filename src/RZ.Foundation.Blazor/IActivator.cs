using Microsoft.Extensions.DependencyInjection;

namespace RZ.Foundation;

public interface IActivator
{
    T Create<T>(params object[] args);
}

public sealed class Activator(IServiceProvider serviceProvider) : IActivator
{
    public T Create<T>(params object[] args)
        => ActivatorUtilities.CreateInstance<T>(serviceProvider, args);
}