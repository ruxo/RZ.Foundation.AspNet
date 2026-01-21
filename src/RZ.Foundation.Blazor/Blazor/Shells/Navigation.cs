namespace RZ.Foundation.Blazor.Shells;

[PublicAPI]
public abstract record Navigation
{
    public abstract record CommonItem(
        string Title,
        string NavPath,
        string? Icon = null,
        bool IsPartialMatch = false,
        string? Policy = null) : Navigation;

    public sealed record Item(string Title, ViewMaker View, string NavPath, string? Icon = null, bool IsPartialMatch = false, string? Policy = null)
        : CommonItem(Title, NavPath, Icon, IsPartialMatch, Policy);

    [PublicAPI]
    public sealed record Divider : Navigation
    {
        public static readonly Divider Instance = new();
    }

    [PublicAPI]
    public sealed record Group(string Title, string? Icon = null, string? Policy = null) : Navigation
    {
        public IEnumerable<Navigation> Items { get; init; } = [];
    }

    public sealed record DirectRoute(string Title, string NavPath, string? Icon = null, bool IsPartialMatch = false, string? Policy = null)
        : CommonItem(Title, NavPath, Icon, IsPartialMatch, Policy);
}