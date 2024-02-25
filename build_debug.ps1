Push-Location $PSScriptRoot -StackName DotnetBuild
dotnet build .\SpectreTableHelpers.csproj --configuration Debug
Pop-Location -StackName DotnetBuild
Read-Host "Press any key to exit."

