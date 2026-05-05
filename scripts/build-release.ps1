<#
.SYNOPSIS
    DesktopPal release build - Phase A (self-contained single-file publish).

.DESCRIPTION
    Implements Option 1 from docs/design/packaging.md: produces a single
    self-contained DesktopPal.exe under artifacts/ that bundles the .NET 10
    desktop runtime. No installer, no signing - that is Phase B / C work.

    This script is the canonical packaging entry point. CI calls it; humans
    call it. Anything that bypasses it is not a release artifact.

.PARAMETER Configuration
    MSBuild configuration. Defaults to Release.

.PARAMETER Runtime
    .NET runtime identifier. Defaults to win-x64.

.PARAMETER OutputRoot
    Folder that receives the published payload. Defaults to ./artifacts.
    Always wiped before publish so leftovers cannot contaminate the artifact.

.PARAMETER Version
    Optional version stamp. Overrides csproj <Version>. Used to tag the
    artifact folder.

.EXAMPLE
    pwsh ./scripts/build-release.ps1
    pwsh ./scripts/build-release.ps1 -Version 0.2.0

.NOTES
    Owner: Rook (QA & Release Engineer).
    Source of truth: docs/design/packaging.md.
#>

[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime       = 'win-x64',
    [string]$OutputRoot    = 'artifacts',
    [string]$Version       = ''
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

$projectPath = Join-Path $repoRoot 'DesktopPal/DesktopPal.csproj'
if (-not (Test-Path $projectPath)) {
    throw "Project not found: $projectPath"
}

$versionTag = if ($Version) { $Version } else { 'dev' }
$artifactDir = Join-Path $repoRoot "$OutputRoot/DesktopPal-v$versionTag-$Runtime"

Write-Host "==> DesktopPal release build" -ForegroundColor Cyan
Write-Host "    Project       : $projectPath"
Write-Host "    Configuration : $Configuration"
Write-Host "    Runtime       : $Runtime"
Write-Host "    Version tag   : $versionTag"
Write-Host "    Output dir    : $artifactDir"

if (Test-Path $artifactDir) {
    Write-Host "==> Removing stale artifact directory" -ForegroundColor DarkGray
    Remove-Item -Recurse -Force $artifactDir
}
New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null

$publishArgs = @(
    'publish', $projectPath,
    '-c', $Configuration,
    '-r', $Runtime,
    '--self-contained', 'true',
    '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true',
    '-p:EnableWindowsTargeting=true',
    '-p:DebugType=embedded',
    '-o', $artifactDir
)

if ($Version) {
    # AssemblyVersion / FileVersion must be numeric (a.b.c[.d]); SemVer
    # pre-release suffixes (-scaffold, -alpha.1, etc.) are rejected by the
    # WPF MarkupCompilePass (MC1005). Strip the suffix for those two and
    # keep the full string on Version / InformationalVersion.
    $numericVersion = ($Version -split '[-+]', 2)[0]
    $publishArgs += "-p:Version=$Version"
    $publishArgs += "-p:InformationalVersion=$Version"
    $publishArgs += "-p:FileVersion=$numericVersion"
    $publishArgs += "-p:AssemblyVersion=$numericVersion"
}

Write-Host "==> dotnet $($publishArgs -join ' ')" -ForegroundColor Cyan
& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$exePath = Join-Path $artifactDir 'DesktopPal.exe'
if (-not (Test-Path $exePath)) {
    throw "Expected artifact not found: $exePath"
}

$exeInfo  = Get-Item $exePath
$totalBytes = (Get-ChildItem -Recurse $artifactDir | Measure-Object -Property Length -Sum).Sum
$totalMB    = [math]::Round($totalBytes / 1MB, 2)
$exeMB      = [math]::Round($exeInfo.Length / 1MB, 2)

Write-Host ""
Write-Host "==> Build succeeded" -ForegroundColor Green
Write-Host "    Artifact dir  : $artifactDir"
Write-Host "    DesktopPal.exe: $exeMB MB"
Write-Host "    Total payload : $totalMB MB"

if ($env:GITHUB_OUTPUT) {
    "artifact_dir=$artifactDir"  | Out-File -FilePath $env:GITHUB_OUTPUT -Append
    "artifact_exe=$exePath"      | Out-File -FilePath $env:GITHUB_OUTPUT -Append
    "artifact_size_mb=$totalMB"  | Out-File -FilePath $env:GITHUB_OUTPUT -Append
    "version_tag=$versionTag"    | Out-File -FilePath $env:GITHUB_OUTPUT -Append
}
