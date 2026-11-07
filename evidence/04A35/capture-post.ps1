param([string]$Label35,[ValidateSet('Debug','Release')][string]$Configuration35)
$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A35/Capture35V2.cs
[Capture35V2]::Run((Get-Location).Path,$Label35,$Configuration35,('.test-rationalization-04A35.tmp/post-'+$Label35+'-'+$Configuration35+'.csv'),$true)
