$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A34/Capture.cs
[Specialty34Capture]::Run((Get-Location).Path)
