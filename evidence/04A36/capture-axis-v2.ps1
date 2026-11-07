param([string]$Label='entry',[string]$Configuration='Debug',[string]$Output='pre-edit-axis-v2.csv',[switch]$Post)
$ErrorActionPreference='Stop'
if(-not $Post){
$locator36=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler36=& $locator36 -products '*' -version '[18.0,19.0)' -find 'MSBuild\Current\Bin\Roslyn\vbc.exe' | Select-Object -First 1
$facade36=Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
& $compiler36 /nologo /target:library /optioncompare:text /out:.test-rationalization-04A36.tmp/LegacyAxis36.dll /reference:.test-rationalization-04A36.tmp/builds/entry/Debug/legacy/BESHStat.Core.dll /reference:.test-rationalization-04A36.tmp/builds/entry/Debug/legacy/BESHStat.Excel.Common.dll /reference:$facade36 evidence/04A36/LegacyAxis36.vb BESHStat.Excel.Common.Tests/src/Spc22LegacyRenderer.vb
if($LASTEXITCODE -ne 0){throw 'Accepted legacy axis fixture compilation failed'}
}
Add-Type -Path evidence/04A36/CaptureAxis36V2.cs
[CaptureAxis36V2]::Run((Get-Location).Path,$Label,$Configuration,('evidence/04A36/'+$Output),[bool]$Post)
