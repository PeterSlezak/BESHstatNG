param([string]$Label='final',[string]$Configuration='Debug')
$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A37/CaptureSeriesPost37.cs
[CaptureSeriesPost37]::Run((Get-Location).Path,$Label,$Configuration)
