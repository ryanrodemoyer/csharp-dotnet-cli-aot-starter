# Native AOT CLI Template - Windows PowerShell Installer
# Supports Windows x64 and arm64

[CmdletBinding()]
param(
    [string]$Version = "latest",
    [string]$Repo = "ryanrodemoyer/csharp-dotnet-cli-aot-starter",
    [string]$InstallDir = "$HOME\.local\bin"
)

$ErrorActionPreference = "Stop"

$AppName = "cli"
$ExeName = "$AppName.exe"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Installing $AppName for Windows" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Detect Architecture
$Arch = if ([System.Environment]::Is64BitOperatingSystem) {
    if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) {
        "arm64"
    } else {
        "x64"
    }
} else {
    Write-Error "Unsupported 32-bit architecture. Only 64-bit systems are supported."
}

$Rid = "win-$Arch"
Write-Host "Detected platform: $Rid" -ForegroundColor Green

# Ensure destination directory exists
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}

$TargetExe = Join-Path $InstallDir $ExeName

# 2. Check if local source build is available
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$CsprojPath = Join-Path $RepoRoot "src\cli\cli.csproj"

if ((Test-Path $CsprojPath) -and (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "Found local project source. Building Native AOT binary locally for $Rid..." -ForegroundColor Yellow
    $PublishDir = Join-Path $RepoRoot "publish\$Rid"
    dotnet publish $CsprojPath -c Release -r $Rid -o $PublishDir --nologo
    Copy-Item (Join-Path $PublishDir $ExeName) $TargetExe -Force
} else {
    # Download pre-built zip from releases
    Write-Host "Downloading release binary for $Rid..." -ForegroundColor Yellow
    $TempZip = Join-Path ([System.IO.Path]::GetTempPath()) "$AppName-$Rid.zip"
    $TempExtract = Join-Path ([System.IO.Path]::GetTempPath()) "$AppName-$Rid-extract"

    $DownloadUrl = if ($Version -eq "latest") {
        "https://github.com/$Repo/releases/latest/download/$AppName-$Rid.zip"
    } else {
        "https://github.com/$Repo/releases/download/$Version/$AppName-$Rid.zip"
    }

    Write-Host "Fetching: $DownloadUrl"
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $TempZip -UseBasicParsing
    Expand-Archive -Path $TempZip -DestinationPath $TempExtract -Force
    Copy-Item (Join-Path $TempExtract $ExeName) $TargetExe -Force

    Remove-Item $TempZip -Force -ErrorAction SilentlyContinue
    Remove-Item $TempExtract -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "`nSuccessfully installed $AppName to $TargetExe" -ForegroundColor Green

# 3. Add to User PATH if not present
$UserPath = [System.Environment]::GetEnvironmentVariable("Path", "User")
$NormalizedInstallDir = (Resolve-Path $InstallDir).Path

if ($UserPath -notlike "*$NormalizedInstallDir*") {
    Write-Host "Adding $NormalizedInstallDir to user PATH..." -ForegroundColor Yellow
    $NewPath = "$UserPath;$NormalizedInstallDir"
    [System.Environment]::SetEnvironmentVariable("Path", $NewPath, "User")
    $env:Path += ";$NormalizedInstallDir"
    Write-Host "Updated PATH. You may need to restart your terminal for changes to take full effect." -ForegroundColor Green
}

Write-Host "`nTest run:" -ForegroundColor Cyan
& $TargetExe info
