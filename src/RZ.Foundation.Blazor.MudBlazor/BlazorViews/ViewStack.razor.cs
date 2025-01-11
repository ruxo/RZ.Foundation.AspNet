using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.BlazorViews;

public partial class ViewStack(IScheduler scheduler, AppChromeViewModel chrome, ShellViewModel shell, IJSRuntime js) : ComponentBase, IDisposable
{
    bool init;
    CompositeDisposable disposables = new();

    [CascadingParameter(Name="DefaultMaxWidth")] public MaxWidth? DefaultMaxWidth { get; set; }

    [Parameter] public MaxWidth? MaxWidth { get; set; }
    [Parameter, EditorRequired] public required RenderFragment ChildContent { get; set; }

    ViewModel? Content => shell.Content;
    ShellViewModel Shell => shell;

    bool needScrollToTop;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (needScrollToTop){
            needScrollToTop = false;

            await js.InvokeVoidAsync("window.scrollTo", 0, 0);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnAfterRender(bool firstRender) {
        if (firstRender || init) return;
        init = true;    // ensure this is only called once

        Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                       h => shell.PropertyChanged += h,
                       h => shell.PropertyChanged -= h)
                  .Where(a => a.EventArgs.PropertyName == nameof(shell.Content))
                  .ObserveOn(scheduler)
                  .Subscribe(_ => {
                       chrome.AppBarMode = shell.StackCount == 0
                                               ? AppBarDisplayMode.Page
                                               : shell.AppMode is AppMode.Modal
                                                   ? AppBarDisplayMode.Modal
                                                   : AppBarDisplayMode.Stacked;
                       needScrollToTop = true;
                       StateHasChanged();
                   })
                  .DisposeWith(disposables);

        chrome.AppBarClicked.Subscribe(_ => {
            if (shell.StackCount > 0)
                shell.CloseCurrentView();
        }).DisposeWith(disposables);
    }

    public void Dispose() {
        disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}