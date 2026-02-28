using ReactiveUI.SourceGenerators;
using RZ.Foundation.Blazor;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;

namespace RZ.Blazor.Server.Example.Components.ShellExample;

public sealed partial class ContentViewModel : ViewModel
{
    readonly ShellViewModel shell;

    public ContentViewModel(ShellViewModel shell) {
        this.shell = shell;
        shell.NavBarMode = NavBarMode.New(NavBarType.Mini);
    }

    [ReactiveCommand]
    void OpenModal() {
        // TODO: test onClose!!!!
        shell.PushModal(new PopupViewModel());
    }

    [ReactiveCommand]
    void ShowNewPage() {
        shell.Push(new PopupViewModel());
    }

    int n = 0;

    [ReactiveCommand]
    void PopupSuccess() {
        shell.Notify(new(MessageSeverity.Info, $"Popup Success {++n}"));
    }

    [ReactiveCommand]
    void PopupFailure() {
        shell.Notify(new(MessageSeverity.Error, $"Popup Failure {++n}"));
    }
}