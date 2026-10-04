# Fast build & Native AOT publish script for Windows PowerShell

[CmdletBinding()]
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$SlnPath = Join-Path $RepoRoot "starter.slnx"
$CsprojPath = Join-Path $RepoRoot "src\cli\cli.csproj"

Write-Host "==> Building solution ($Configuration)..." -ForegroundColor Cyan
dotnet build $SlnPath -c $Configuration

Write-Host "==> Running test suite..." -ForegroundColor Cyan
dotnet test $SlnPath -c $Configuration --no-build

$Arch = if ([System.Environment]::Is64BitOperatingSystem) {
    if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) {
        "arm64"
    } else {
        "x64"
    }
} else {
    Write-Error "Unsupported 32-bit architecture."
}

$Rid = "win-$Arch"
$PublishDir = Join-Path $RepoRoot "publish\$Rid"

Write-Host "==> Publishing Native AOT binary for $Rid..." -ForegroundColor Yellow
dotnet publish $CsprojPath -c $Configuration -r $Rid -o $PublishDir

$ExePath = Join-Path $PublishDir "cli.exe"
if (Test-Path $ExePath) {
    Write-Host "==> Publish successful! Binary size:" -ForegroundColor Green
    Get-Item $ExePath | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
    
    Write-Host "`n==> Running diagnostics:" -ForegroundColor Cyan
    & $ExePath info
}
