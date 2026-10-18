# Run in Windows PowerShell 5.1 or PowerShell 7; project structure from 2026-10-17 ZIP.
[CmdletBinding()]
param(
    [switch]$Legacy,
    [switch]$Wasm,
    [switch]$Js,
    [ValidateSet('Debug','Release')][string]$Configuration = 'Debug',
    [ValidateSet('2022','2026')][string]$VisualStudio = '2022'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $root
$results = Join-Path $root 'TestResults\Codex'
New-Item -ItemType Directory -Path $results -Force | Out-Null

function Invoke-Checked([string]$Exe,[string[]]$Arguments) {
    Write-Host ('> ' + $Exe + ' ' + ($Arguments -join ' ')) -ForegroundColor Cyan
    & $Exe @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Command failed with exit code ${LASTEXITCODE}: $Exe" }
}

Write-Host 'BESHStatNG portable regression validation' -ForegroundColor Green
Invoke-Checked 'dotnet' @('test', '.\BESHStat.Core.Tests\BESHStat.Core.Tests.vbproj', '-c', $Configuration, '--logger', 'trx;LogFileName=Core.trx', '--results-directory', $results)
Invoke-Checked 'dotnet' @('test', '.\BESHStat.Excel.Common.Tests\BESHStat.Excel.Common.Tests.vbproj', '-c', $Configuration, '--logger', 'trx;LogFileName=Common.trx', '--results-directory', $results)

if ($Wasm) {
    Invoke-Checked 'dotnet' @('build', '.\BESHStat.WasmSmoke\BESHStat.WasmSmoke.csproj', '-c', $Configuration)
    Write-Host 'WASM project build only; browser/Excel smoke tests remain manual.' -ForegroundColor Yellow
}

if ($Js) {
    Push-Location (Join-Path $root 'BESHStat.WasmSmoke')
    try { Invoke-Checked 'npm.cmd' @('test') }
    finally { Pop-Location }
}

if ($Legacy) {
    if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) { throw 'Legacy tests require Windows.' }
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $vswhere)) { throw 'vswhere.exe not found. Install Visual Studio 2022/2026.' }
    $versionRange = if ($VisualStudio -eq '2022') { '[17.0,18.0)' } else { '[18.0,19.0)' }
    $msbuild = (& $vswhere -version $versionRange -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\MSBuild.exe' | Select-Object -First 1)
    $vsroot = (& $vswhere -version $versionRange -products '*' -property installationPath | Select-Object -First 1)
    if (-not $msbuild -or -not (Test-Path $msbuild)) { throw "MSBuild not found for VS $VisualStudio" }
    if (-not $vsroot) { throw "Visual Studio $VisualStudio installation not found" }
    # Legacy project uses packages.config; restore packages once in VS or via nuget restore beforehand.
    Invoke-Checked $msbuild @('.\BESHStatNG.Test\BESHStatNG.Tests.vbproj', '/t:Build', "/p:Configuration=$Configuration", '/m')
    $dll = Join-Path $root "BESHStatNG.Test\bin\$Configuration\BESHStatNG.Test.dll"
    if (-not (Test-Path $dll)) { throw "Expected legacy test assembly missing: $dll" }
    $vstest = Join-Path $vsroot 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'
    if (-not (Test-Path $vstest)) { throw "vstest.console.exe not found: $vstest" }
    Invoke-Checked $vstest @($dll, '/Logger:trx;LogFileName=Legacy.trx', "/ResultsDirectory:$results")
}

Write-Host "Validation commands completed. TRX reports: $results" -ForegroundColor Green
