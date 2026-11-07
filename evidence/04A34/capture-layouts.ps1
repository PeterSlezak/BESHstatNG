$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A34/CaptureLayouts.cs
[Layouts34Capture]::Run((Get-Location).Path)
