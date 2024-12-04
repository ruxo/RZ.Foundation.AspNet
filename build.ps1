$destination = $args[0]

function build {
    param ($path)
    Write-Output "Target: $path"
    Push-Location $path
    Remove-Item ./bin/Release/*.nupkg
    dotnet pack -c Release
    $file = Get-Item ./bin/Release/*.nupkg
    Write-Output "Build $($file.Name)"
    Move-Item ./bin/Release/*.nupkg $destination
    Pop-Location
}

Push-Location ./src

build .\RZ.AspNet.Bootstrapper
build .\RZ.AspNet.Api
build .\RZ.Foundation.Blazor
build .\RZ.Foundation.Blazor.MudBlazor

Pop-Location