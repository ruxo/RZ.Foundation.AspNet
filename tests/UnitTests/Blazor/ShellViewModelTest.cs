using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RZ.Foundation.Blazor.Shells;
using RZ.Foundation.Testing;
using Xunit.Abstractions;

namespace UnitTests.Blazor;

public class ShellViewModelTest(ITestOutputHelper output)
{
    readonly ServiceCollection ioc = new();

    [Theory]
    [InlineData("/app", "/")]
    [InlineData("/app/", "/")]
    [InlineData("/app/shell", "/shell")]
    [InlineData("/app/shell/", "/shell/")]
    [InlineData("/ap", null)]
    public void GetShellPath_Correct(string navigationPath, string? expected) {
        ioc.AddSingleton(new ShellOptions("/app"));
        var shell = ioc.UseLogger(output).BuildAndCreate<ShellViewModel>();

        // when
        var path = shell.GetShellPath(navigationPath);

        // then
        path.Should().Be(expected);
    }
}