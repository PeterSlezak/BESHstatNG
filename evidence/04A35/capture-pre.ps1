$ErrorActionPreference='Stop'
$locator35=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler35=& $locator35 -products '*' -version '[18.0,19.0)' -find 'MSBuild\Current\Bin\Roslyn\vbc.exe' | Select-Object -First 1
$facade35=Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
& $compiler35 /nologo /target:library /out:.test-rationalization-04A35.tmp/Legacy35.dll /reference:.test-rationalization-04A35.tmp/builds/entry/Debug/legacy/BESHStat.Core.dll /reference:.test-rationalization-04A35.tmp/builds/entry/Debug/legacy/BESHStat.Excel.Common.dll /reference:$([System.Linq.Enumerable].Assembly.Location) /reference:$facade35 BESHStat.Excel.Common.Tests/src/Legacy35.vb BESHStat.Excel.Common.Tests/src/LegacyLoveProjection35.vb evidence/04A35/Cases35.vb
if($LASTEXITCODE -ne 0){throw 'Original source-slice compilation failed'}
Add-Type -Path evidence/04A35/Capture35.cs
[Capture35]::Run((Get-Location).Path,'entry','Debug','evidence/04A35/pre-edit-capture.csv',$false)
