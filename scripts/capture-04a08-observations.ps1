# Managed source-oracle/controller observations. No shown forms, Excel, COM or native Excel workflow.
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$AssemblyDirectory,
      [Parameter(Mandatory=$true)][string]$EvidenceDirectory,
      [ValidateSet('Debug','Release')][string]$Configuration='Debug',
      [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Name='final')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$run=(Resolve-Path -LiteralPath $EvidenceDirectory).Path
if (-not $run.StartsWith((Join-Path $root 'TestResults\04A-08-'),[StringComparison]::OrdinalIgnoreCase)) {throw 'Use initialized 04A.08 evidence'}
$assemblyRoot=(Resolve-Path -LiteralPath $AssemblyDirectory).Path
if (-not $assemblyRoot.StartsWith($run+'\',[StringComparison]::OrdinalIgnoreCase)) {throw 'Use run-isolated assemblies'}
$target=Join-Path $run ('authoring-observations-'+$Configuration+'-'+$Name+'.json')
if (Test-Path -LiteralPath $target) {throw 'No-clobber output already exists'}
Add-Type -AssemblyName System.Windows.Forms
foreach ($file in @('BESHStat.Core.dll','BESHStat.Excel.Common.dll','BESHStatNG.dll','BESHStatNG.Test.dll')) {[void][Reflection.Assembly]::LoadFrom((Join-Path $assemblyRoot $file))}
$oldType=[Reflection.Assembly]::LoadFrom((Join-Path $assemblyRoot 'BESHStatNG.Test.dll')).GetType('BESHStatNG.Test.LegacyRegressionEffects08',$true)
$newType=[Reflection.Assembly]::LoadFrom((Join-Path $assemblyRoot 'BESHStatNG.dll')).GetType('BESHStatNG.RegressionEffectsController',$true)
function Snapshot($box,$specs) {
    $model=[BESHStatNG.ExcelCommon.RegressionEffectModel]::FromProjection([string[]]@($box.Items),$specs,$true)
    return [BESHStatNG.ExcelCommon.RegressionEffectSerialization]::Serialize($model)
}
function HashText([string]$value) {
    $sha=[Security.Cryptography.SHA256]::Create()
    try {return ([BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($value)))).Replace('-','').ToLowerInvariant()}
    finally {$sha.Dispose()}
}
$records=New-Object 'System.Collections.Generic.List[object]'
$methods=@('AddMainEffectsFromSelectedVars','AddCategoricalEffectsFromSelectedVars','AddPolynomialEffectsFromSelectedVars','AddTwoWayInteractionsFromSelectedVars','AddCustomInteractionFromSelectedVars')
foreach ($culture in @('en-US','sk-SK')) {
    [Threading.Thread]::CurrentThread.CurrentCulture=[Globalization.CultureInfo]::GetCultureInfo($culture)
    foreach ($method in $methods) {
        foreach ($seed in @($false,$true)) {
            $vars=New-Object Windows.Forms.ListBox
            $before=New-Object Windows.Forms.ListBox
            $after=New-Object Windows.Forms.ListBox
            try {
                $vars.SelectionMode='MultiExtended'
                $vars.Items.AddRange([object[]]@('Age | VarB','Group | VarC',([string][char]0x96EA)+' | VarD'))
                $old=$oldType.GetConstructors()[0].Invoke([object[]]@($vars.PSObject.BaseObject,$before.PSObject.BaseObject,$null))
                $current=$newType.GetConstructors()[0].Invoke([object[]]@($vars.PSObject.BaseObject,$after.PSObject.BaseObject,$null))
                $messages=New-Object 'System.Collections.Generic.List[string]'
                $sink=[Action[string]]{param($message) $messages.Add($message)}
                $newType.GetProperty('PresentMessage',[Reflection.BindingFlags]'Instance,NonPublic,Public').SetValue($current,$sink)
                if ($seed) {
                    $vars.SetSelected(0,$true)
                    if ($method -eq 'AddCategoricalEffectsFromSelectedVars') {$old.AddMainEffectsFromSelectedVars();$current.AddMainEffectsFromSelectedVars()}
                    else {$old.AddCategoricalEffectsFromSelectedVars();$current.AddCategoricalEffectsFromSelectedVars()}
                }
                for ($i=2;$i -ge 0;$i--) {$vars.SetSelected($i,$true)}
                $arguments=if ($method -eq 'AddPolynomialEffectsFromSelectedVars') {[object[]]@(3)} else {[object[]]@()}
                [void]$oldType.GetMethod($method).Invoke($old,$arguments)
                [void]$newType.GetMethod($method).Invoke($current,$arguments)
                $a=Snapshot $before $old.TermSpecs
                $b=Snapshot $after $current.TermSpecs
                if ($a -cne $b -or (($old.Messages -join "`n") -cne ($messages -join "`n"))) {throw 'Observation mismatch'}
                $records.Add([ordered]@{culture=$culture;method=$method;seed=$seed;before_sha256=(HashText $a);after_sha256=(HashText $b);equal=$true;
                    canonical_typed_spec=$b;messages=@($messages.ToArray());effect_keys=@($after.Items)})
            } finally {$vars.Dispose();$before.Dispose();$after.Dispose()}
        }
    }
}
[ordered]@{schema='04A08-managed-authoring-observations-v1';configuration=$Configuration;native_executed=$false;
    scope='Source-derived executable oracle vs shipping bridge; managed list controls only; NOT_GOLDEN';
    assemblies=@(Get-ChildItem -LiteralPath $assemblyRoot -Filter BESHStat*.dll | ForEach-Object {[ordered]@{name=$_.Name;sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()}});
    observations=$records.ToArray()} | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $target -Encoding UTF8
Write-Host ($records.Count.ToString()+' managed before/after authoring observations PASS; native NOT RUN')
