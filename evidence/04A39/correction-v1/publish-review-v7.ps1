[CmdletBinding()]
param(
    [ValidateSet('Stage', 'Publish')][string]$Action = 'Stage',
    [string]$Python = (Join-Path $env:USERPROFILE 'Desktop\WPy64-31470\python\python.exe')
)
$ErrorActionPreference = 'Stop'
$taskRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
$taskScript = if ($Action -eq 'Stage') { 'create-final-stage.py' } else { 'finalize-publication.py' }
Push-Location -LiteralPath $taskRoot
try {
    $taskInvocation = "import sys,runpy;sys.path.insert(0,'evidence/04A39/correction-v1');runpy.run_path('evidence/04A39/correction-v1/$taskScript',run_name='__main__')"
    & $Python -I -B -c $taskInvocation
    if ($LASTEXITCODE -ne 0) { throw "Publication process failed with exit $LASTEXITCODE; artifacts retained; no retry or cleanup performed." }
}
finally { Pop-Location }
