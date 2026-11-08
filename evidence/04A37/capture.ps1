param([string]$Configuration='Debug')
$ErrorActionPreference='Stop'
$locator37=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler37=& $locator37 -products '*' -version '[18.0,19.0)' -find 'MSBuild\Current\Bin\Roslyn\vbc.exe' | Select-Object -First 1
$facade37=Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
& $compiler37 /nologo /target:library /optioncompare:text /out:.test-rationalization-04A37.tmp/Original37.dll /reference:.test-rationalization-04A37.tmp/builds/entry/Debug/legacy/BESHStat.Core.dll /reference:$([System.Linq.Enumerable].Assembly.Location) /reference:$facade37 evidence/04A37/preedit/Original37.vb evidence/04A37/preedit/Cases37.vb
if($LASTEXITCODE -ne 0){throw 'Original source-slice compilation failed'}
Add-Type -Path evidence/04A37/Capture37.cs
[Capture37]::Run((Get-Location).Path,$Configuration)
