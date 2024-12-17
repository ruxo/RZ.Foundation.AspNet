﻿using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Foundation.BlazorViews;

public partial class ViewStack(AppChromeViewModel chrome, ShellViewModel shell) : ComponentBase, IDisposable
{
    bool init;
    CompositeDisposable disposables = new();

    [Parameter] public MaxWidth MaxWidth { get; set; } = MaxWidth.False;
    [Parameter, EditorRequired] public required RenderFragment ChildContent { get; set; }

    ViewModel? Content => shell.Content;
    ShellViewModel Shell => shell;

    protected override void OnAfterRender(bool firstRender) {
        if (firstRender || init) return;
        init = true;    // ensure this is only called once

        Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                       h => shell.PropertyChanged += h,
                       h => shell.PropertyChanged -= h)
                  .Where(a => a.EventArgs.PropertyName == nameof(shell.Content))
                  .Subscribe(_ => {
                       chrome.AppBarMode = shell.StackCount == 0
                                               ? AppBarDisplayMode.Page
                                               : shell.AppMode is AppMode.Modal
                                                   ? AppBarDisplayMode.Modal
                                                   : AppBarDisplayMode.Stacked;
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