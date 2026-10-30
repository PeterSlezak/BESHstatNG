[CmdletBinding()]
param([string]$PythonExe = 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe',
      [string]$EvidencePath = 'TestResults/04A-01-amendment')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location $root
$evidence = Join-Path $root $EvidencePath
$env:RestoreSources = Join-Path $root 'packages'
$env:NuGetAudit = 'false'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:PythonExe = $PythonExe
if (-not (Test-Path -LiteralPath $PythonExe)) { throw 'Installed Python absent; do not install prerequisites.' }
$before = Get-Content -Raw (Join-Path $evidence 'before.json') | ConvertFrom-Json
$restoreAllowed = @('BESHstatNG/Generated/VersionAttributes.vb','BESHstatNG/src/Help/HelpLinks.vb','BESHstatNG/src/Help/HelpContext.vb')
foreach ($standard in @('Codex','04-01')) {
    $backup = Join-Path $evidence ('standard-before/' + $standard)
    if (Test-Path -LiteralPath $backup) { throw ('Prior suite backup already exists: ' + $backup) }
    if (Test-Path -LiteralPath ('TestResults/' + $standard)) {
        New-Item -ItemType Directory -Force (Split-Path $backup) | Out-Null
        Copy-Item -LiteralPath ('TestResults/' + $standard) -Destination $backup -Recurse
    }
}
$results = @()
foreach ($configuration in @('Debug','Release')) {
    $configDir = Join-Path $evidence $configuration
    New-Item -ItemType Directory -Force $configDir | Out-Null
    foreach ($kind in @('full','foundation')) {
        & $PythonExe -B scripts/windows-migration-audit/amendment.py check
        if ($LASTEXITCODE -ne 0) { throw 'Pre-suite source/evidence guard failed.' }
        $script = if ($kind -eq 'full') {'scripts/verify-beshstatng.ps1'} else {'scripts/verify-office-foundation.ps1'}
        $arguments = @('-NoProfile','-ExecutionPolicy','Bypass','-File',$script,'-Configuration',$configuration)
        if ($kind -eq 'full') { $arguments += @('-VisualStudio','2026','-Legacy','-Wasm','-Js','-Python','-PythonExe',$PythonExe) }
        $log = Join-Path $configDir ($kind + '.log')
        $code = -1
        try {
            $savedPreference = $ErrorActionPreference
            try {
                $ErrorActionPreference = 'Continue'
                & powershell @arguments *> $log
                $code = $LASTEXITCODE
            } finally { $ErrorActionPreference = $savedPreference }
        } finally {
            # Preserve transient generated evidence, restore only exact pre-audit copies.
            # Never restore an unrelated path or permit an unbacked generated change.
            foreach ($item in $before.tracked) {
                $current = (Get-FileHash -LiteralPath $item.path -Algorithm SHA256).Hash.ToLowerInvariant()
                if ($current -eq $item.sha256) { continue }
                if ($restoreAllowed -notcontains $item.path) { throw ('Unexpected tracked change: ' + $item.path) }
                $original = Join-Path $root ('TestResults/04A-01/protected/bytes/' + $item.path)
                if ((Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash.ToLowerInvariant() -ne $item.sha256) { throw 'Protected backup mismatch.' }
                $generated = Join-Path $configDir ('generated-by-' + $kind + '/' + $item.path)
                New-Item -ItemType Directory -Force (Split-Path $generated) | Out-Null
                Copy-Item -LiteralPath $item.path -Destination $generated
                Copy-Item -LiteralPath $original -Destination $item.path
            }
        }
        $trxRoot = if ($kind -eq 'full') {'TestResults/Codex'} else {'TestResults/04-01'}
        $trxFilter = if ($kind -eq 'full') {'*.trx'} else {'OfficeAddin-' + $configuration + '.trx'}
        if (Test-Path -LiteralPath $trxRoot) { Get-ChildItem -LiteralPath $trxRoot -Filter $trxFilter | Copy-Item -Destination $configDir }
        $results += [ordered]@{command=('powershell ' + ($arguments -join ' ')); exit_code=$code; log=($EvidencePath.TrimEnd('/','\') + '/' + $configuration + '/' + $kind + '.log'); offline_sources='existing local packages and global cache; NuGetAudit=false'; completed_utc=[DateTime]::UtcNow.ToString('o')}
        ConvertTo-Json -Depth 6 -InputObject $results | Set-Content -Encoding UTF8 (Join-Path $evidence 'validation-runs.json')
        & $PythonExe -B scripts/windows-migration-audit/amendment.py check
        if ($LASTEXITCODE -ne 0) { throw 'Post-suite source/evidence guard failed.' }
        Write-Output ($configuration + ' ' + $kind + ': exit ' + $code + '; ' + $log)
        if ($code -ne 0) { Write-Output 'Suite failed; preserved failure log. Continuing independent configuration checks.' }
    }
}
