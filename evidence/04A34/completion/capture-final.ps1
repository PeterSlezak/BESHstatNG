param([ValidateSet('Debug','Release')][string]$Configuration34)
$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A34/completion/PostCaptureShapes.cs
[PostCompletionShapes34Capture]::Run((Get-Location).Path,$Configuration34)
Add-Type -Path evidence/04A34/completion/PostCaptureExtras.cs
[PostCompletionExtras34Capture]::Run((Get-Location).Path,$Configuration34)
Add-Type -Path evidence/04A34/completion/PostCapturePreparation.cs
[PostCompletionPreparation34Capture]::Run((Get-Location).Path,$Configuration34)
Add-Type -Path evidence/04A34/completion/PostCaptureSeries.cs
[PostCompletionSeries34Capture]::Run((Get-Location).Path,$Configuration34)
Add-Type -Path evidence/04A34/completion/PostCapturePolicies.cs
[PostCompletionPolicies34Capture]::Run((Get-Location).Path,$Configuration34)
Add-Type -Path evidence/04A34/completion/PostCaptureOriginal.cs
[Specialty34FinalCapture]::Run((Get-Location).Path,$Configuration34)
