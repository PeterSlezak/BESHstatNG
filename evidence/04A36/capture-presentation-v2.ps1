param([string]$Label='entry',[string]$Configuration='Debug',[string]$Output='pre-edit-presentation-v2.csv',[switch]$Post)
$ErrorActionPreference='Stop'
if(-not $Post){
$locator36=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler36=& $locator36 -products '*' -version '[18.0,19.0)' -find 'MSBuild\Current\Bin\Roslyn\vbc.exe' | Select-Object -First 1
$facade36=Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
& $compiler36 /nologo /target:library /optioncompare:text /out:.test-rationalization-04A36.tmp/LegacyPresentation36V2.dll /reference:.test-rationalization-04A36.tmp/Legacy36.dll /reference:.test-rationalization-04A36.tmp/builds/entry/Debug/legacy/BESHStat.Core.dll /reference:.test-rationalization-04A36.tmp/builds/entry/Debug/legacy/BESHStat.Excel.Common.dll /reference:$([System.Linq.Enumerable].Assembly.Location) /reference:$facade36 evidence/04A36/LegacyPresentation36V2.vb
if($LASTEXITCODE -ne 0){throw 'Original presentation-slice compilation failed'}
}
Add-Type -Path evidence/04A36/CapturePresentation36V2.cs
[CapturePresentation36V2]::Run((Get-Location).Path,$Label,$Configuration,('evidence/04A36/'+$Output),[bool]$Post)
