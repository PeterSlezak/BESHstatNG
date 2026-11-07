$ErrorActionPreference='Stop'
$compilerLocator34=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler34=& $compilerLocator34 -products '*' -version '[18.0,19.0)' -find 'MSBuild\Current\Bin\Roslyn\vbc.exe' | Select-Object -First 1
& $compiler34 /nologo /target:library /out:.test-rationalization-04A34.tmp/completion-legacy-series.dll /reference:.test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHStat.Core.dll /reference:.test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHstatNG.dll /reference:$([System.Linq.Enumerable].Assembly.Location) /reference:$(Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll') BESHStatNG.Test/src/Charts/LegacyCompletionSeries34.vb BESHStatNG.Test/src/Charts/CompletionSeriesCases34.vb BESHStat.Core.Tests/src/Graphics/Completion34Cases.vb
if($LASTEXITCODE -ne 0){throw 'Legacy series slice compilation failed'}
Add-Type -Path evidence/04A34/completion/CaptureSeries.cs
[CompletionSeries34Capture]::Run((Get-Location).Path)
