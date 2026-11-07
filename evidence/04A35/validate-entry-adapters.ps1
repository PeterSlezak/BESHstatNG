$ErrorActionPreference='Stop'
$folder35=Join-Path (Get-Location) '.test-rationalization-04A35.tmp/builds/entry/Debug/legacy'
foreach($name35 in @('BESHStat.Core.dll','BESHStat.Excel.Common.dll')){[Reflection.Assembly]::LoadFrom((Join-Path $folder35 $name35)) | Out-Null}
$host35=[Reflection.Assembly]::LoadFrom((Join-Path $folder35 'BESHstatNG.dll'))
$fixture35=[Reflection.Assembly]::LoadFrom((Join-Path (Get-Location) '.test-rationalization-04A35.tmp/Legacy35V2.dll'))
$cases35=$fixture35.GetType('Legacy35.Cases35');$original35=$fixture35.GetType('Legacy35.Original35');$adapter35=$host35.GetType('BESHStatNG.graphics.MultipleComparisonPlotExcel')
function Execute35($method35,$data35){
    $args35=New-Object object[] 1;$args35[0]=$data35
    try{return 'VALUE:'+ [Convert]::ToString($method35.Invoke($null,$args35),[Globalization.CultureInfo]::InvariantCulture)}
    catch{ $inner35=$_.Exception.InnerException;return 'ERROR:'+$inner35.GetType().FullName+':'+$(if($inner35 -is [ArgumentException]){$inner35.ParamName}else{''}) }
}
$rows35=New-Object 'Collections.Generic.List[object]'
foreach($culture35 in @('','sk-SK')){
    [Globalization.CultureInfo]::CurrentCulture=[Globalization.CultureInfo]::GetCultureInfo($culture35)
    foreach($name35 in @('BuildTitle','ValidatePlotData')){
        for($index35=0;$index35 -lt 280;$index35++){
            $data35=$cases35.GetMethod('Data').Invoke($null,@($index35))
            $expected35=Execute35 ($original35.GetMethod($name35)) $data35
            $actual35=Execute35 ($adapter35.GetMethod($name35,[Reflection.BindingFlags]'Static,NonPublic')) $data35
            if($expected35 -cne $actual35){throw ('Original adapter/source-slice mismatch: '+$name35+' case '+$index35)}
            $rows35.Add(@{culture=$culture35;member=$name35;case=$index35;expected=$expected35;actual=$actual35})
        }
    }
}
$output35=Join-Path (Get-Location) 'evidence/04A35/pre-edit-actual-adapter.json'
if(Test-Path -LiteralPath $output35){throw 'Immutable original adapter capture exists'}
[IO.File]::WriteAllText($output35,($rows35 | ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
Write-Output 'PASS1120 original compiled adapter/independent source-slice comparisons, invariant/sk-SK'
