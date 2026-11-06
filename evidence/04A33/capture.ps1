param([string]$Label='before',[string]$Configuration='Debug')
$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A33/helpers/Capture.cs
$captureBuild33=Join-Path (Get-Location) ".test-rationalization-04A33.tmp/builds/$Label/$Configuration/legacy"
$captureOutput33=Join-Path (Get-Location) ".test-rationalization-04A33.tmp/$Label-$Configuration-adapter-capture.csv"
if(Test-Path -LiteralPath $captureOutput33){throw 'Refusing to overwrite a capture'}
[Graphics33Capture]::Run($captureBuild33,$captureOutput33)
