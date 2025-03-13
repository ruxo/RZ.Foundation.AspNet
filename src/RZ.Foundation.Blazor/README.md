# RZ MVVM for Blazor

## Localization

According to [Blazor documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization),
they suggest using redirection in order to switch languages for Blazor **Server**.
This works by setting culture token via cookie. So the way to set the culture is to replace that
cookie value and redirect. Very inconvenient. It is designed to use with static rendering.

Other way is to manually set UI culture during `SetParametersAsync` event, which works both pre-rendering and 
server-side rendering. But it does not work with interactive because this is called once.

```csharp
public override Task SetParametersAsync(ParameterView parameters) {
    CultureInfo.CurrentUICulture = new("th");
    return base.SetParametersAsync(parameters);
}
```

The only way to make it work with all scenarios is to set the culture in the Razor page itself!

```csharp
@{ CultureInfo.CurrentUICulture = new(lang); }
```

Simply put this statement on top of the page, though, not elegant solution.