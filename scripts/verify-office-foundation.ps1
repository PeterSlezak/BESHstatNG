# Narrow Batch 04.01 gate. Run alongside verify-beshstatng.ps1; no live Office or publication.
[CmdletBinding()]
param([ValidateSet('Debug','Release')][string]$Configuration = 'Debug')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $root
$results = Join-Path $root 'TestResults/04-01'
New-Item -ItemType Directory -Path $results -Force | Out-Null

function Invoke-Checked([string]$Exe, [string[]]$Arguments) {
    Write-Host ('> ' + $Exe + ' ' + ($Arguments -join ' '))
    & $Exe @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Command failed: $Exe ($LASTEXITCODE)" }
}

Invoke-Checked 'dotnet' @('build', 'BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj', '-c', $Configuration)
Invoke-Checked 'dotnet' @('test', 'BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj', '-c', $Configuration,
    '--logger', "trx;LogFileName=OfficeAddin-$Configuration.trx", '--results-directory', $results)
Push-Location (Join-Path $root 'BESHStat.OfficeAddin')
try { Invoke-Checked 'npm.cmd' @('test') }
finally { Pop-Location }

# Blazor build serves source assets through its static-web-assets manifest. A build does
# not physically copy index.html into bin/wwwroot (publish is deliberately not invoked).
$project = Join-Path $root 'BESHStat.OfficeAddin'
$manifest = Get-Content (Join-Path $project "obj/$Configuration/net10.0/staticwebassets.build.json") -Raw | ConvertFrom-Json
foreach ($asset in @('index.html','js/officeHost.mjs','js/officeDiagnostics.js','js/workbookAdapter.mjs','css/app.css')) {
    $expectedIdentity = Join-Path $project ('wwwroot/' + $asset)
    $matches = @($manifest.Assets | Where-Object { $_.Identity -eq $expectedIdentity })
    if ($matches.Count -ne 1 -or -not (Test-Path -LiteralPath $matches[0].Identity)) {
        throw "Missing or ambiguous built static asset: $asset"
    }
}
$framework = Join-Path $project "bin/$Configuration/net10.0/wwwroot/_framework"
foreach ($assembly in @('BESHStat.Core','BESHStat.Excel.Common','BESHStat.OfficeAddin')) {
    if (@(Get-ChildItem -LiteralPath $framework -Filter "$assembly.*.wasm").Count -ne 1) {
        throw "Expected one fingerprinted WASM assembly: $assembly"
    }
}
if (@($manifest.Assets | Where-Object { $_.RelativePath -match '(^|/)tests/' }).Count -ne 0) {
    throw 'Test assets leaked into host static assets.'
}
Write-Host "Office foundation $Configuration build, tests, manifest and asset checks passed. Live browser/Excel acceptance remains manual."
