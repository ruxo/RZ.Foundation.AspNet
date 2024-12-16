using JetBrains.Annotations;
using RZ.Foundation.Blazor.MVVM;

namespace RZ.Foundation.Blazor.Shells;

public delegate ViewModel ViewMaker(IActivator factory);

[PublicAPI]
public sealed record ShellOptions(string BasePath = "/")
{
    public NavBarMode InitialNavBar { get; set; } = NavBarMode.New(NavBarType.Full);
    public IEnumerable<Navigation> Navigation { get; init; } = [];
}