[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location $root
$evidence = Join-Path $root 'TestResults/04A-01'
$env:RestoreSources = (Join-Path $root 'packages')
$env:NuGetAudit = 'false'
$env:PythonExe = 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe'
if (-not (Test-Path -LiteralPath $env:PythonExe)) { throw 'Previously installed Python interpreter absent; do not install implicitly.' }
$before = Get-Content -Raw (Join-Path $evidence 'protected/before.json') | ConvertFrom-Json
$restoreAllowed = @('BESHstatNG/Generated/VersionAttributes.vb','BESHstatNG/src/Help/HelpLinks.vb','BESHstatNG/src/Help/HelpContext.vb')
$results = @()
foreach ($configuration in @('Debug','Release')) {
    $configDir = Join-Path $evidence $configuration
    New-Item -ItemType Directory -Force $configDir | Out-Null
    foreach ($kind in @('full','foundation')) {
        & (Join-Path $PSScriptRoot 'protect.ps1') -Mode Check
        $script = if($kind -eq 'full'){'scripts/verify-beshstatng.ps1'}else{'scripts/verify-office-foundation.ps1'}
        $arguments = @('-NoProfile','-ExecutionPolicy','Bypass','-File',$script,'-Configuration',$configuration)
        if ($kind -eq 'full') { $arguments += @('-VisualStudio','2026','-Legacy','-Wasm','-Js','-Python','-PythonExe',$env:PythonExe) }
        $log = Join-Path $configDir ($kind + '.log')
        $savedPreference = $ErrorActionPreference
        try {
            # Python unittest progress is stderr even on success in Windows PowerShell 5.1.
            $ErrorActionPreference = 'Continue'
            & powershell @arguments *> $log
            $code = $LASTEXITCODE
        } finally { $ErrorActionPreference = $savedPreference }
        $results += [ordered]@{command=('powershell '+($arguments -join ' ')); exit_code=$code; log=('TestResults/04A-01/'+$configuration+'/'+$kind+'.log'); offline_sources='existing local packages plus existing global cache; NuGetAudit=false'}
        $changed = @($before | Where-Object { (Get-FileHash -LiteralPath $_.path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $_.sha256 })
        foreach ($item in $changed) {
            if ($restoreAllowed -notcontains $item.path) { throw ('Unexpected baseline change: '+$item.path) }
            $generated = Join-Path $configDir ('generated-by-'+$kind+'/'+$item.path)
            New-Item -ItemType Directory -Force (Split-Path $generated) | Out-Null
            Copy-Item -LiteralPath $item.path -Destination $generated
            Copy-Item -LiteralPath (Join-Path $evidence ('protected/bytes/'+$item.path)) -Destination $item.path
            $results += [ordered]@{event='restored exact pre-run protected bytes'; path=$item.path; generated_copy=$generated.Substring($root.Length+1).Replace('\','/')}
        }
        & (Join-Path $PSScriptRoot 'protect.ps1') -Mode Check
        $trxRoot = if($kind -eq 'full'){'TestResults/Codex'}else{'TestResults/04-01'}
        $trxFilter = if($kind -eq 'full'){'*.trx'}else{'OfficeAddin-'+$configuration+'.trx'}
        if (Test-Path $trxRoot) { Get-ChildItem -LiteralPath $trxRoot -Filter $trxFilter | Copy-Item -Destination $configDir }
        ConvertTo-Json -Depth 5 -InputObject $results | Set-Content -Encoding UTF8 (Join-Path $evidence 'validation-runs.json')
        Write-Output ($configuration+' '+$kind+': exit '+$code+'; '+$log)
    }
}
