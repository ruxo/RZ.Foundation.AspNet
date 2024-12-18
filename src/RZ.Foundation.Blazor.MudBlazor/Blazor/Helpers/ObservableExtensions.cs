using System.Reactive.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.Blazor.Helpers;

[PublicAPI]
public static class ObservableExtensions
{
    public static IObservable<Outcome<T>> ReportFailure<T>(this IObservable<Outcome<T>> source, ShellViewModel shell, ILogger logger)
        => source.Do(v => {
            if (v.IfFail(out var e, out _)){
                logger.LogError("Operation failed: {@Error}", e);
                shell.Notify(new(MessageSeverity.Error, e.Message));
            }
        });
}