param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputFolder = "out\EasySave"
)

Write-Host "==========================================="
Write-Host "  EasySave - Flat Package Publisher"
Write-Host "==========================================="

if (Test-Path $OutputFolder) {
    Write-Host "Cleaning existing folder..."
    Remove-Item $OutputFolder -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputFolder | Out-Null

$commonArgs = @(
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "/p:PublishSingleFile=true",
    "/p:IncludeNativeLibrariesForSelfExtract=true",
    "/p:PublishTrimmed=false",
    "/p:DebugType=None",
    "-o", $OutputFolder
)

Write-Host "Publishing Console..."
dotnet publish "src/EasySave.App.Console/EasySave.App.Console.csproj" @commonArgs

Write-Host "Publishing GUI..."
dotnet publish "src/EasySave.App.Gui/EasySave.App.Gui.csproj" @commonArgs

Write-Host "Publishing CryptoSoft..."
dotnet publish "src/CryptoSoft/CryptoSoft.csproj" @commonArgs

Write-Host ""
Write-Host "Package ready in: $OutputFolder"
Write-Host ""
Get-ChildItem $OutputFolder