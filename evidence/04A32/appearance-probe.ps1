param([string]$Label='before',[string]$Configuration='Debug')
$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A32/helpers/AppearanceProbe.cs
$probeBuild32=Join-Path (Get-Location) ".test-rationalization-04A32.tmp/builds/$Label/$Configuration/legacy"
$probeOutput32=Join-Path (Get-Location) ".test-rationalization-04A32.tmp/$Label-$Configuration-appearance-probe.csv"
if(Test-Path -LiteralPath $probeOutput32){throw 'Refusing to overwrite a probe'}
[Graphics32AppearanceProbe]::Run($probeBuild32,$probeOutput32)
Write-Output "Host-free $Label $Configuration appearance/axis probe complete"
