using System.Diagnostics;
using Microsoft.JSInterop;

namespace RZ.Foundation.Blazor;

[PublicAPI]
public abstract class JsInteropBase(IJSRuntime js, string modulePath) : IAsyncDisposable
{
    readonly Lazy<Task<IJSObjectReference>> importModule = new (() => js.InvokeAsync<IJSObjectReference>("import", modulePath).AsTask());

    protected async ValueTask InvokeVoidAsync(string identifier, params object?[]? args) {
        var module = await importModule.Value;
        await module.InvokeVoidAsync(identifier, args);
    }

    protected async ValueTask<T> InvokeAsync<T>(string identifier, params object?[]? args) {
        var module = await importModule.Value;
        return await module.InvokeAsync<T>(identifier, args);
    }

    protected async ValueTask InvokeVoidAsync(string identifier, CancellationToken cancelToken, params object?[]? args) {
        var module = await importModule.Value;
        await module.InvokeVoidAsync(identifier, cancelToken, args);
    }

    protected async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancelToken, params object?[]? args) {
        var module = await importModule.Value;
        return await module.InvokeAsync<T>(identifier, cancelToken, args);
    }

    public virtual async ValueTask DisposeAsync() {
        if (importModule.IsValueCreated){
            using var import = importModule.Value;
            var module = await import;
            var (error, _) = await Try(module, async m => await m.DisposeAsync());   // It's possible to error here
            if (error is not null)
                Trace.WriteLine($"Error disposing {GetType().Name}: {error}");
        }
        GC.SuppressFinalize(this);
    }
}