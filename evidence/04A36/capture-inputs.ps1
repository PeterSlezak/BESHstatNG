$ErrorActionPreference='Stop'
$captureRoot36=(Get-Location).Path
[Reflection.Assembly]::LoadFrom((Join-Path $captureRoot36 '.test-rationalization-04A36.tmp/builds/entry/Debug/legacy/BESHStat.Core.dll')) | Out-Null
$originalFixture36=[Reflection.Assembly]::LoadFrom((Join-Path $captureRoot36 '.test-rationalization-04A36.tmp/Legacy36.dll'))
$casesType36=$originalFixture36.GetType('Legacy36.Cases36',$true)
$snapshot36=$casesType36.GetMethod('Snapshot')
function Snapshot36([object]$Value){return $snapshot36.Invoke($null,[object[]]@(,$Value))}
$inputRows36=for($caseIndex36=0;$caseIndex36 -lt 152;$caseIndex36++){
    $km36=$casesType36.GetMethod('KM').Invoke($null,[object[]]@($caseIndex36))
    $kmFields36=[ordered]@{}
    if($null -ne $km36){foreach($property36 in $km36.GetType().GetProperties()){$kmFields36[$property36.Name]=Snapshot36 $property36.GetValue($km36,$null)}}
    $times36=$casesType36.GetMethod('Numbers').Invoke($null,[object[]]@($caseIndex36))
    $probabilityCase36=if($caseIndex36 % 3 -eq 0){$caseIndex36+1}else{$caseIndex36}
    $probabilities36=$casesType36.GetMethod('Numbers').Invoke($null,[object[]]@($probabilityCase36))
    $points36=$casesType36.GetMethod('Points').Invoke($null,[object[]]@($caseIndex36))
    $pointRows36=foreach($point36 in $points36){
        if($null -eq $point36){$null;continue}
        $fields36=[ordered]@{}
        foreach($property36 in $point36.GetType().GetProperties()){$fields36[$property36.Name]=Snapshot36 $property36.GetValue($point36,$null)}
        $fields36
    }
    [ordered]@{case=$caseIndex36;km_null=($null -eq $km36);km_fields=$kmFields36;cox_time=(Snapshot36 $times36);cox_probability=(Snapshot36 $probabilities36);spc_points=@($pointRows36)}
}
$record36=[ordered]@{authority='Replay of immutable PRE_EDIT input generator and entry assemblies; original Cases36.vb was frozen before production edits. This JSON expansion does not create expected outputs.';double_policy='Exact IEEE Int64 hex including NaN payload and signed zero; array rank/dimensions retained.';cases=@($inputRows36)}
$target36=Join-Path $captureRoot36 'evidence/04A36/pre-edit-inputs.json'
if(Test-Path -LiteralPath $target36){throw 'Frozen input expansion exists'}
[IO.File]::WriteAllText($target36,($record36 | ConvertTo-Json -Depth 20),(New-Object Text.UTF8Encoding($false)))
Write-Output ('Expanded '+$inputRows36.Count+' immutable original input cases; no new implementation used')
