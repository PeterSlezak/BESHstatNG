param([string]$Label='final',[string]$Configuration='Debug',[string]$OutputPrefix='postedit')
$ErrorActionPreference='Stop'
$locator37=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler37=& $locator37 -products '*' -version '[18.0,19.0)' -find 'MSBuild\Current\Bin\Roslyn\vbc.exe' | Select-Object -First 1
$facade37=Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
$dependencies37='.test-rationalization-04A37.tmp/builds/'+$Label+'/'+$Configuration+'/legacy/'
$coreReference37='/reference:'+$dependencies37+'BESHStat.Core.dll'
$commonReference37='/reference:'+$dependencies37+'BESHStat.Excel.Common.dll'
& $compiler37 /nologo /target:library /out:.test-rationalization-04A37.tmp/Current37.dll $coreReference37 $commonReference37 /reference:$([System.Linq.Enumerable].Assembly.Location) /reference:$facade37 .test-rationalization-04A37.tmp/Current37.vb
if($LASTEXITCODE -ne 0){throw 'Current shipping prefix compilation failed'}
Add-Type -Path evidence/04A37/CapturePost37.cs
[CapturePost37]::Run((Get-Location).Path,$Label,$Configuration,$OutputPrefix)
