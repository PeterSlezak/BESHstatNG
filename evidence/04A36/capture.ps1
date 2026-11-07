param([string]$Label='final',[string]$Configuration='Debug',[string]$Output='post-edit-Debug.csv',[switch]$Pre)
$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A36/Capture36.cs
[Capture36]::Run((Get-Location).Path,$Label,$Configuration,('evidence/04A36/'+$Output),(-not $Pre))
