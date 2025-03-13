# MudBlazor chrome for RZ.Foundation.Blazor

## Design Justification

We have a namespace `BlazorViews` to contain all components so that the user can simply refer to
a single namespace to get all the components.

## Localization

According to [Blazor documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization),
they suggest using redirection in order to switch languages for Blazor **Server**.
This works by setting culture token via cookie. So the way to set the culture is to replace that
cookie value and redirect. Very inconvenient.