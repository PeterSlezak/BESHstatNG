$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A34/CaptureReferences.cs
[References34Capture]::Run((Get-Location).Path)
