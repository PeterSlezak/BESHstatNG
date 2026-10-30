[CmdletBinding()]
param([string]$PythonExe = $env:PythonExe)
$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (-not $PythonExe) { $PythonExe = & scripts/resolve-beshstatng-python.ps1 }
$evidence = 'TestResults/04A-01'
New-Item -ItemType Directory -Force $evidence | Out-Null
$dll = 'scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll'
$gitPrefix = @('-c', ('safe.directory=' + (Get-Location).Path.Replace('\','/')))
$checks = @(
    @{id='syntax-fixtures';exe='dotnet';arguments=@($dll,'--self-test');expected=0},
    @{id='source-check';exe='dotnet';arguments=@($dll,'--check');expected=0},
    @{id='strict-acceptance';exe='dotnet';arguments=@($dll,'--check','--strict');expected=2},
    @{id='catalog-check';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/catalog.py','--check');expected=0},
    @{id='negative-reconciliation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/test_reconciliation.py');expected=0},
    @{id='protected-bytes';exe='powershell';arguments=@('-NoProfile','-ExecutionPolicy','Bypass','-File','scripts/windows-migration-audit/protect.ps1','-Mode','Check');expected=0},
    @{id='diff-check';exe='git';arguments=($gitPrefix+@('diff','--check'));expected=0},
    @{id='index-unchanged';exe='git';arguments=($gitPrefix+@('diff','--cached','--exit-code'));expected=0},
    @{id='status-short';exe='git';arguments=($gitPrefix+@('status','--short'));expected=0}
)
$records = @()
foreach($check in $checks) {
    $log = Join-Path $evidence ($check.id+'.log')
    $savedPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        & $check.exe @($check.arguments) *> $log
        $code = $LASTEXITCODE
    } finally { $ErrorActionPreference = $savedPreference }
    $records += [ordered]@{id=$check.id;exe=$check.exe;arguments=$check.arguments;exit_code=$code;expected_exit_code=$check.expected;status=if($code -eq $check.expected){'PASS (expected open blockers)' }else{'FAIL'};log=$log.Replace('\','/');sha256=(Get-FileHash -LiteralPath $log -Algorithm SHA256).Hash.ToLowerInvariant()}
    if ($check.id -ne 'strict-acceptance' -and $code -eq 0) { $records[-1].status='PASS' }
    ConvertTo-Json -Depth 6 -InputObject $records | Set-Content -Encoding UTF8 (Join-Path $evidence 'final-checks.json')
    Write-Output ($check.id+': '+$code+' (expected '+$check.expected+')')
    if ($code -ne $check.expected) { throw ('Audit check failed: '+$check.id+'; inspect '+$log) }
}
