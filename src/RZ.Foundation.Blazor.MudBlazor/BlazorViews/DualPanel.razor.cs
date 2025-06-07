namespace RZ.Foundation.BlazorViews;

[UsedImplicitly]
partial class DualPanel
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public required RenderFragment LeftPanel { get; set; }

    [Parameter] public RenderFragment? RightPanel { get; set; }

    [Parameter] public bool ShowRightPanel { get; set; } = true;

    const string PaperStyle = "overflow: auto";

    string EffectiveClass => $"pa-4 max-height {Class}";
}