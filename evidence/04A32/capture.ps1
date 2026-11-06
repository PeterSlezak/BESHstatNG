param([string]$Label='before',[string]$Configuration='Debug')
$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A32/helpers/Capture.cs
$captureBuild32=Join-Path (Get-Location) ".test-rationalization-04A32.tmp/builds/$Label/$Configuration/legacy"
$captureOutput32=Join-Path (Get-Location) ".test-rationalization-04A32.tmp/$Label-$Configuration-adapter-capture.csv"
if(Test-Path -LiteralPath $captureOutput32){throw 'Refusing to overwrite a capture'}
[Graphics32Capture]::Run($captureBuild32,$captureOutput32)
Write-Output "Host-free $Label $Configuration adapter capture complete"
