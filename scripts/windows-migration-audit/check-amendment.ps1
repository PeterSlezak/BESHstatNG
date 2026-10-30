[CmdletBinding()]
param([string]$PythonExe = 'C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe',
      [string]$EvidencePath = 'TestResults/04A-01-amendment')
$ErrorActionPreference = 'Stop'
Set-Location (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$evidence = $EvidencePath
$dll = 'scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll'
$gitPrefix = @('-c', ('safe.directory=' + (Get-Location).Path.Replace('\','/')))
$checks = @(
    @{id='syntax-fixtures';exe='dotnet';arguments=@($dll,'--self-test');expected=0},
    @{id='source-check';exe='dotnet';arguments=@($dll,'--check');expected=0},
    @{id='strict-acceptance';exe='dotnet';arguments=@($dll,'--check','--strict');expected=2},
    @{id='catalog-check';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/catalog.py','--check');expected=0},
    @{id='negative-reconciliation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/test_reconciliation.py');expected=0},
    @{id='protected-and-original-evidence';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/amendment.py','check');expected=0},
    @{id='diff-check';exe='git';arguments=($gitPrefix+@('diff','--check'));expected=0},
    @{id='index-unchanged';exe='git';arguments=($gitPrefix+@('diff','--cached','--exit-code'));expected=0},
    @{id='status-short';exe='git';arguments=($gitPrefix+@('status','--short','--untracked-files=all'));expected=0},
    @{id='status-ignored';exe='git';arguments=($gitPrefix+@('status','--short','--untracked-files=all','--ignored'));expected=0}
)
$records=@()
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-03') {
    $checks += @{id='continuation03-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation03_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-04') {
    $checks += @{id='continuation04-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation04_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-05') {
    $checks += @{id='continuation05-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation05_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-06') {
    $checks += @{id='continuation06-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation06_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-07') {
    $checks += @{id='continuation07-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation07_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-08') {
    $checks += @{id='continuation08-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation08_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-09') {
    $checks += @{id='continuation09-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation09_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-10') {
    $checks += @{id='continuation10-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation10_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-11') {
    $checks += @{id='continuation11-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation11_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-12') {
    $checks += @{id='continuation12-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation12_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-13') {
    $checks += @{id='continuation13-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation13_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-14') {
    $checks += @{id='continuation14-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation14_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-15') {
    $checks += @{id='continuation15-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation15_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-16') {
    $checks += @{id='continuation16-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation16_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-17') {
    $checks += @{id='continuation17-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation17_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-18') {
    $checks += @{id='continuation18-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation18_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-19') {
    $checks += @{id='continuation19-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation19_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-20') {
    $checks += @{id='continuation20-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation20_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-21') {
    $checks += @{id='continuation21-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation21_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-22') {
    $checks += @{id='continuation22-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation22_report.py','--guard');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-23') {
    $checks += @{id='continuation23-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation23_report.py','--guard','--history');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-24') {
    $checks += @{id='continuation24-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation24_report.py','--guard','--history');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-25') {
    $checks += @{id='continuation25-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation25_report.py','--guard','--history');expected=0}
}
if ((Split-Path -Leaf $evidence) -eq '04A-01-continuation-26') {
    $checks += @{id='continuation26-preservation';exe=$PythonExe;arguments=@('-B','scripts/windows-migration-audit/continuation26_report.py','--guard');expected=0}
}
foreach($check in $checks) {
    $log = Join-Path $evidence ($check.id+'.log')
    $saved = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        & $check.exe @($check.arguments) *> $log
        $code = $LASTEXITCODE
    } finally { $ErrorActionPreference = $saved }
    $records += [ordered]@{id=$check.id;exe=$check.exe;arguments=$check.arguments;exit_code=$code;expected_exit_code=$check.expected;status=if($code -eq $check.expected){if($code -eq 2){'EXPECTED ACCEPTANCE FAILURE; NOT READY'}else{'PASS'}}else{'FAIL'};log=$log.Replace('\','/');sha256=(Get-FileHash -LiteralPath $log -Algorithm SHA256).Hash.ToLowerInvariant()}
    ConvertTo-Json -Depth 6 -InputObject $records | Set-Content -Encoding UTF8 (Join-Path $evidence 'final-checks.json')
    Write-Output ($check.id+': '+$code+' (expected '+$check.expected+')')
    if ($code -ne $check.expected) { throw ('Audit amendment check failed: '+$check.id+'; inspect '+$log) }
}
