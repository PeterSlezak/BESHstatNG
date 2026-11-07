$ErrorActionPreference='Stop'
$compilerLocator34=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler34=& $compilerLocator34 -products '*' -version '[18.0,19.0)' -find 'MSBuild\Current\Bin\Roslyn\vbc.exe' | Select-Object -First 1
$systemCore34=[System.Linq.Enumerable].Assembly.Location
$netstandard34=Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
& $compiler34 /nologo /target:library /out:.test-rationalization-04A34.tmp/completion-legacy-policies.dll /reference:.test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHStat.Core.dll "/reference:$systemCore34" "/reference:$netstandard34" BESHStat.Core.Tests/src/Graphics/LegacyCompletionPolicies34.vb BESHStat.Core.Tests/src/Graphics/CompletionPolicyCases34.vb BESHStat.Core.Tests/src/Graphics/Completion34Cases.vb
if($LASTEXITCODE -ne 0){throw 'Legacy policy slice compilation failed'}
Add-Type -Path evidence/04A34/completion/CapturePolicies.cs
[CompletionPolicies34Capture]::Run((Get-Location).Path)
