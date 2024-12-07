namespace RZ.Foundation.Blazor;

public enum NavBarType
{
    Full, Mini
}

public sealed record NavBarMode(NavBarType Type, bool Visible, bool Expanded)
{
    public static NavBarMode New(NavBarType type) => new(type, Visible: true, Expanded: false);
}