$ErrorActionPreference='Stop'
$compilerLocator34=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$compiler34=& $compilerLocator34 -products '*' -version '[18.0,19.0)' -find 'MSBuild\Current\Bin\Roslyn\vbc.exe' | Select-Object -First 1
& $compiler34 /nologo /target:library /out:.test-rationalization-04A34.tmp/completion-legacy-spc.dll /reference:.test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHStat.Core.dll /reference:$([System.Linq.Enumerable].Assembly.Location) /reference:$(Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll') BESHStat.Core.Tests/src/Graphics/LegacySpcBasis34.vb BESHStat.Core.Tests/src/Graphics/Completion34Cases.vb
if($LASTEXITCODE -ne 0){throw 'Legacy SPC basis compilation failed'}
$fixture34=[Reflection.Assembly]::LoadFrom((Join-Path (Get-Location) '.test-rationalization-04A34.tmp/completion-legacy-spc.dll'))
[Reflection.Assembly]::LoadFrom((Join-Path (Get-Location) '.test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHStat.Core.dll')) | Out-Null
$module34=$fixture34.GetType('Batch34CompletionFixtures.LegacySpcBasis34')
$snapshot34=$fixture34.GetType('Batch34CompletionFixtures.Completion34Cases').GetMethod('Snapshot')
$rows34=New-Object 'System.Collections.Generic.List[string]'
$rows34.Add('method,case,output,exception')
foreach($method34 in $module34.GetMethods([Reflection.BindingFlags]'Public,Static')){
    for($case34=0;$case34 -lt 40;$case34++){
        $args34=New-Object object[] $method34.GetParameters().Length
        for($slot34=0;$slot34 -lt $args34.Length;$slot34++){
            $parameter34=$method34.GetParameters()[$slot34]
            if($parameter34.ParameterType -eq [int]){$args34[$slot34]=[int](@(-2,-1,0,1,5)[$case34 % 5])}
            else{$args34[$slot34]=[double](@(0.0,-1.0,0.5,[double]::NaN,[double]::PositiveInfinity,[double]::NegativeInfinity)[$case34 % 6])}
            if($parameter34.ParameterType.Name.StartsWith('Nullable') -and $case34 % 7 -eq 0){$args34[$slot34]=$null}
        }
        $error34='None';$value34=$null
        try{$value34=$method34.Invoke($null,$args34)}catch{$error34=$_.Exception.InnerException.GetType().FullName}
        $snapshotArgs34=New-Object object[] 1
        $snapshotArgs34[0]=$value34
        $bits34=$snapshot34.Invoke($null,$snapshotArgs34)
        $rows34.Add($method34.Name+','+$case34+','+$bits34+','+$error34)
    }
}
$output34=Join-Path (Get-Location) 'evidence/04A34/completion/pre-edit-spc-basis.csv'
if(Test-Path -LiteralPath $output34){throw 'Frozen capture exists'}
[IO.File]::WriteAllLines($output34,$rows34,(New-Object Text.UTF8Encoding($false)))
