$ErrorActionPreference='Stop'
$folder35=Join-Path (Get-Location) '.test-rationalization-04A35.tmp/builds/entry/Debug/legacy'
foreach($name35 in @('BESHStat.Core.dll','BESHStat.Excel.Common.dll')){[Reflection.Assembly]::LoadFrom((Join-Path $folder35 $name35)) | Out-Null}
$assembly35=[Reflection.Assembly]::LoadFrom((Join-Path (Get-Location) '.test-rationalization-04A35.tmp/Legacy35.dll'))
$cases35=$assembly35.GetType('Legacy35.Cases35')
function Input35($value35){
    if($null -eq $value35){return $null}
    if($value35 -is [double]){return [BitConverter]::DoubleToInt64Bits($value35).ToString('X16')}
    if($value35 -is [string] -or $value35 -is [int] -or $value35 -is [bool]){return $value35}
    if($value35 -is [Collections.IEnumerable]){return ,@($value35 | ForEach-Object { Input35 $_ })}
    $result35=[ordered]@{}
    foreach($property35 in $value35.GetType().GetProperties()){$result35[$property35.Name]=Input35 ($property35.GetValue($value35,$null))}
    return $result35
}
$records35=@()
foreach($method35 in @('Data','Rows')){
    for($index35=0;$index35 -lt 280;$index35++){
        $value35=$cases35.GetMethod($method35).Invoke($null,@($index35))
        $records35+=@{factory=$method35;case=$index35;input=(Input35 $value35)}
    }
}
$output35=Join-Path (Get-Location) 'evidence/04A35/pre-edit-inputs.json'
if(Test-Path -LiteralPath $output35){throw 'Immutable input catalog already exists'}
[IO.File]::WriteAllText($output35,($records35 | ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
Write-Output 'Captured560 actual original-source input records; doubles encoded as IEEE bits'
