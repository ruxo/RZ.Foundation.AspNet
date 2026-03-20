using Microsoft.AspNetCore.Components.Web;

namespace RZ.Blazor.Server.Example.Components;

public partial class App
{
    public static readonly InteractiveServerRenderMode RenderMode = new InteractiveServerRenderMode(prerender: false);
}