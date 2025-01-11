using Microsoft.Extensions.DependencyInjection;

namespace RZ.Foundation;

public static class Activator
{
    public static T Create<T>(this IServiceProvider sp, params object[] args)
        => ActivatorUtilities.CreateInstance<T>(sp, args);
}