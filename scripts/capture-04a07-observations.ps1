# Pure managed before/after report capture. Shipping adapters, frozen source oracle, no form/Excel/writer calls.
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$AssemblyDirectory,
      [Parameter(Mandatory=$true)][string]$EvidenceDirectory,
      [ValidateSet('Debug','Release')][string]$Configuration='Debug',
      [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Name='v2')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$run=(Resolve-Path -LiteralPath $EvidenceDirectory).Path
if (-not $run.StartsWith((Join-Path $root 'TestResults\04A-07-'),[StringComparison]::OrdinalIgnoreCase)) {throw 'Use initialized 04A.07 evidence'}
$target=Join-Path $run ('observations-'+$Configuration+'-'+$Name+'.json')
if (Test-Path -LiteralPath $target) {throw 'No-clobber output already exists'}
$assemblyRoot=(Resolve-Path -LiteralPath $AssemblyDirectory).Path
if (-not $assemblyRoot.StartsWith($run,[StringComparison]::OrdinalIgnoreCase)) {throw 'Use this run isolated assemblies'}
foreach ($file in @('BESHStat.Core.dll','BESHStat.Excel.Common.dll','BESHStatNG.dll','BESHStatNG.Test.dll')) {[void][Reflection.Assembly]::LoadFrom((Join-Path $assemblyRoot $file))}
$testAssembly=[Reflection.Assembly]::LoadFrom((Join-Path $assemblyRoot 'BESHStatNG.Test.dll'))
$legacy=$testAssembly.GetType('BESHStatNG.Test.LegacyAnovaWorkflow07',$true)
$assumptions=$testAssembly.GetType('BESHStatNG.Test.LegacyAssumption06Characterization',$true)
function Snapshot($table) {
    $model=if ($table -is [BESHStatNG.ResultTableOutputModel]) {$table} else {$table.ToOutputModel()}
    $rows=New-Object 'System.Collections.Generic.List[object]'
    for ($r=0;$r -lt $model.RowCount;$r++) {
        $cells=New-Object 'System.Collections.Generic.List[object]'
        for ($c=0;$c -lt $model.ColumnCount;$c++) {
            $value=$model.Values[$r,$c];$type=if ($null -eq $value) {'Nothing'} else {$value.GetType().FullName}
            if ($value -is [double] -and ([double]::IsNaN($value) -or [double]::IsInfinity($value))) {$value=$value.ToString([Globalization.CultureInfo]::InvariantCulture)}
            $cells.Add([ordered]@{type=$type;value=$value})
        }
        $rows.Add($cells.ToArray())
    }
    return [ordered]@{rows=$rows.ToArray();headerTopRows=$model.HeaderTopRows;headerLeftColumns=$model.HeaderLeftColumns;titleRows=$model.TitleRows;
        footerRows=$model.FooterRows;pvalueColumns=@($model.PvalueColumns);pvalueCells=@($model.PvalueCells|ForEach-Object {,@($_.BodyRow,$_.BodyColumn)})}
}
function New-Groups {
    $d=New-Object BESHStatNG.MultiGroupsUnpairedData
    $d.X=[double[][]]@([double[]]@(7,1,3,4),[double[]]@(9,2,6,8,3),[double[]]@(11,4,5));$d.varNames=[string[]]@("Z'eta",'0','alpha');return $d
}
function New-Repeated {
    $d=New-Object BESHStatNG.MultiGroupsPairedData
    $d.X=[double[,]]::new(6,3);$rows=@(@(1,1.2,1.1),@(0.9,1.1,1),@(1.1,1.3,1.25),@(1.05,1.15,1.2),@(0.95,1.05,1),@(1.2,1.25,1.3))
    for ($r=0;$r -lt 6;$r++) {for ($c=0;$c -lt 3;$c++) {$d.X[$r,$c]=$rows[$r][$c]}}
    $d.varNames=[string[]]@('A','B','C');return $d
}
function New-Nested([bool]$unbalanced) {
    $d=New-Object BESHStatNG.MultiGroupsPairedDataObj
    $rows=@(@('B','b2',10.0),@('A','a1',1.0),@('A','a2',7.0),@('B','b1',2.0),@('A','a1',3.0),@('B','b2',8.0),@('A','a2',5.0),@('B','b1',4.0))
    if ($unbalanced) {$rows+=,@('A','a1',2.0)}
    $d.X=[object[,]]::new($rows.Count,3)
    for ($r=0;$r -lt $rows.Count;$r++) {for ($c=0;$c -lt 3;$c++) {$d.X[$r,$c]=$rows[$r][$c]}}
    $d.varNames=[string[]]@('Parent','Child','Response');return $d
}
function Box-Table($data) {
    $box=[Activator]::CreateInstance([BESHStatNG.graphics.BoxPlot],[object[]]@($data.X,$data.varNames,$null,''))
    $box.Calculate();$box.CalcForPlotting();return $box.wrapResults()
}
function Assert-Equal($before,$after,[string]$context) {
    if ((ConvertTo-Json -InputObject $before -Depth 18 -Compress) -cne (ConvertTo-Json -InputObject $after -Depth 18 -Compress)) {throw ('Before/after mismatch '+$context)}
}
$observations=New-Object 'System.Collections.Generic.List[object]'
$saved=[Threading.Thread]::CurrentThread.CurrentCulture
try {
 foreach ($culture in @('','sk-SK')) {
  [Threading.Thread]::CurrentThread.CurrentCulture=[Globalization.CultureInfo]::GetCultureInfo($culture)
  foreach ($scenario in @('anova-base','anova-all','kw-base','kw-dunn','rm-base','rm-all','nested-balanced','nested-unbalanced')) {
   $expected=New-Object 'System.Collections.Generic.List[object]';$actual=New-Object 'System.Collections.Generic.List[object]'
   $metadata=[ordered]@{source_coordinates='Unavailable for these hand-supplied arrays; real producer provenance is covered by MSTest';native_charts_rendered=$false}
   if ($scenario.StartsWith('anova') -or $scenario.StartsWith('kw')) {
    $before=New-Groups;$after=New-Groups
    $anova=$scenario.StartsWith('anova');$all=$scenario -eq 'anova-all' -or $scenario -eq 'kw-dunn';$dunn=$scenario -ne 'kw-base'
    $kind=if ($anova) {[BESHStatNG.ExcelCommon.SpreadsheetAnalysisKind]::OneWayAnova} else {[BESHStatNG.ExcelCommon.SpreadsheetAnalysisKind]::KruskalWallis}
    if ($anova) {
        $oldOptions=New-Object BESHStatNG.ExcelCommon.OneWayAnovaAnalysisOptions
        $oldOptions.Alpha=0.05;$oldOptions.IncludeWelch=$all;$oldOptions.IncludeFisherLsd=$all;$oldOptions.IncludeBonferroni=$all;$oldOptions.IncludeTukeyKramer=$all;$oldOptions.IncludeGamesHowell=$all
    } else {$oldOptions=New-Object BESHStatNG.ExcelCommon.KruskalWallisAnalysisOptions;$oldOptions.Alpha=0.05;$oldOptions.IncludeDunnComparisons=$dunn}
    $captured=[BESHStatNG.ExcelDnaSharedApplicationAdapter]::ToGroupedNumericData($before)
    $request=[BESHStatNG.ExcelCommon.SpreadsheetAnalysisRequest]::new($kind,$captured,$oldOptions)
    $oldPrimary=[BESHStatNG.ExcelCommon.SharedAnalysisApplicationService]::Run($request)
    foreach ($table in $oldPrimary.Tables) {$expected.Add((Snapshot $table))}
    if ($anova -and $all) {$expected.Add((Snapshot ($assumptions.GetMethod('ComputeVarianceHomogeneity').Invoke($null,[object[]]@($before.PSObject.BaseObject,$true,$true,$true,$true)))))}
    if ($all) {$expected.Add((Snapshot ($assumptions.GetMethod('ComputeDescriptiveStats').Invoke($null,[object[]]@($before.PSObject.BaseObject,$null)))))}
    if (-not $anova -and $all) {$expected.Add((Snapshot ($assumptions.GetMethod('ComputeVarianceHomogeneity').Invoke($null,[object[]]@($before.PSObject.BaseObject,$true,$true,$true,$true)))))}
    if ($all) {$expected.Add((Snapshot (Box-Table $before)))}
    $homogeneity=if ($all) {[BESHStatNG.ExcelCommon.HomogeneityMethods]::All} else {[BESHStatNG.ExcelCommon.HomogeneityMethods]::None}
    $options=[BESHStatNG.ExcelCommon.AnovaGroupedWorkflowOptions]::new($kind,0.05,($anova -and $all),($anova -and $all),($anova -and $all),($anova -and $all),($anova -and $all),$dunn,$homogeneity,$all,$all)
    $workflow=[BESHStatNG.WindowsAnovaWorkflowAdapter]::AnalyzeGrouped($after,$options,[Threading.CancellationToken]::None)
    $supplements=[BESHStatNG.WindowsAnovaWorkflowAdapter]::PrepareSupplements($after,$options,[Threading.CancellationToken]::None)
    foreach ($table in $workflow.LegacyAnalysis.Tables) {$actual.Add((Snapshot $table))}
    foreach ($table in $supplements.Tables) {$actual.Add((Snapshot $table))}
    $metadata.stages=@($workflow.Plan.Stages | ForEach-Object {$_.ToString()})
    $metadata.comparison_plots=@($workflow.ComparisonPlots | ForEach-Object {[ordered]@{procedure=$_.ProcedureName;alpha=$_.Alpha;labels=$_.Labels;estimates=$_.Estimates;lower=$_.LowerLimits;upper=$_.UpperLimits}})
    $metadata.metrics=$workflow.LegacyAnalysis.Metrics
   } elseif ($scenario.StartsWith('rm')) {
    $before=New-Repeated;$after=New-Repeated;$all=$scenario -eq 'rm-all'
    $old=$legacy.GetMethod('Repeated').Invoke($null,[object[]]@($before.PSObject.BaseObject,[double]0.05,$all,$all,$all,$all,$all))
    foreach ($table in $old) {$expected.Add((Snapshot $table))}
    if ($all) {$expected.Add((Snapshot (Box-Table $before)))}
    $options=[BESHStatNG.ExcelCommon.RepeatedAnovaWorkflowOptions]::new(0.05,$all,$all,$all,$all,$all,$all)
    $prepared=[BESHStatNG.WindowsAnovaWorkflowAdapter]::Repeated($after,$options,[Threading.CancellationToken]::None)
    foreach ($table in $prepared.Tables) {$actual.Add((Snapshot $table))}
    $metadata.calls=@($prepared.Report.Calls)
   } else {
    $unbalanced=$scenario -eq 'nested-unbalanced';$before=New-Nested $unbalanced;$after=New-Nested $unbalanced
    $old=$legacy.GetMethod('Nested').Invoke($null,[object[]]@($before.PSObject.BaseObject))
    $prepared=[BESHStatNG.WindowsAnovaWorkflowAdapter]::Nested($after,[Threading.CancellationToken]::None)
    foreach ($table in $old.Item1) {$expected.Add((Snapshot $table))}
    foreach ($table in $prepared.ToLegacyTables()) {$actual.Add((Snapshot $table))}
    Assert-Equal $old.Item2 ([bool]$prepared.BalancedDesign) $scenario
    $metadata.balanced=[bool]$prepared.BalancedDesign;$metadata.banner=@($prepared.Banner[0,0],$prepared.Banner[1,0]);$metadata.separator=@(@('',''),@($prepared.Separators[0][1,0],''))
   }
   Assert-Equal ($expected.ToArray()) ($actual.ToArray()) ($scenario+'/'+$culture)
   $observations.Add([ordered]@{scenario=$scenario;culture=$culture;equal=$true;before=$expected.ToArray();after=$actual.ToArray();metadata=$metadata})
  }
 }
} finally {[Threading.Thread]::CurrentThread.CurrentCulture=$saved}
$record=[ordered]@{schema='04A07-managed-report-observations-v1';configuration=$Configuration;provenance='Current Core plus frozen pre-edit source oracle; NOT native; NOT independent new scientific references; NOT_GOLDEN';native_executed=$false;observations=$observations.ToArray()}
$bytes=[Text.UTF8Encoding]::new($false).GetBytes((ConvertTo-Json -InputObject $record -Depth 24))
$stream=[IO.File]::Open($target,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try {$stream.Write($bytes,0,$bytes.Length)} finally {$stream.Dispose()}
Write-Host ('Passed '+$observations.Count+' before/after managed workflow observations; '+$target)
