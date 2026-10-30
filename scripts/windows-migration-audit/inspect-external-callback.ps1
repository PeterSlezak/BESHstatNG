[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$relative = 'packages/ExcelDna.Integration.1.9.0/lib/net462/ExcelDna.Integration.dll'
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $relative).Path)
$type = $assembly.GetType('ExcelDna.Integration.CustomUI.ExcelRibbon', $true)
$method = $type.GetMethod('LoadImage', [Type[]]@([string]))
if (-not $method) { throw 'Installed ExcelRibbon.LoadImage(String) declaration absent.' }
$evidence = [ordered]@{
    callback='LoadImage'; declaration=$method.ToString(); declaring_type=$method.DeclaringType.FullName
    assembly_path=$relative; assembly_sha256=(Get-FileHash -LiteralPath $relative -Algorithm SHA256).Hash.ToLowerInvariant()
    evidence_status='manually-reviewed reflection metadata; method not invoked'; runtime_proven=$false
    owner_role='Windows Ribbon maintainer'; future_batch='04A.45'; verification='Owner Excel image-loading/resource/bitness smoke'
}
ConvertTo-Json -Depth 4 -InputObject $evidence | Set-Content -Encoding UTF8 docs/architecture/WindowsMigrationLedger/external-callback-evidence.json
Write-Output 'Resolved inherited LoadImage declaration using installed assembly metadata; no Excel invocation.'
