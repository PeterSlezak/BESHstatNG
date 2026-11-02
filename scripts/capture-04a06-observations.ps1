# Host-free, source-characterized before/after observations. No Form, Excel or writer invocation.
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$AssemblyDirectory,
    [Parameter(Mandatory=$true)][string]$EvidenceDirectory,
    [ValidateSet('Debug','Release')][string]$Configuration = 'Debug'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Get-Location).Path
$run = (Resolve-Path -LiteralPath $EvidenceDirectory).Path
if (-not $run.StartsWith((Join-Path $root 'TestResults\04A-06-'), [StringComparison]::OrdinalIgnoreCase)) { throw 'Use a fresh initialized 04A.06 evidence directory' }
$target = Join-Path $run ('observations-' + $Configuration + '-v2.json')
if (Test-Path -LiteralPath $target) { throw 'No-clobber observations already exist' }
$assemblyRoot = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
if (-not $assemblyRoot.StartsWith($run, [StringComparison]::OrdinalIgnoreCase)) { throw 'Use an isolated assembly copy from this run' }
foreach ($name in @('BESHStat.Core.dll','BESHStat.Excel.Common.dll','BESHStatNG.dll','BESHStatNG.Test.dll')) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $assemblyRoot $name))
}
$testAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $assemblyRoot 'BESHStatNG.Test.dll'))
$legacy = $testAssembly.GetType('BESHStatNG.Test.LegacyAssumption06Characterization', $true)

function New-Data([int]$extremes = 0) {
    $d = New-Object BESHStatNG.MultiGroupsUnpairedData
    $first = [double[]](1..30 | ForEach-Object { [Math]::Sin($_ * 1.7) + $_ * 0.11 + ($_ % 5) * 0.2 })
    $second = [double[]](1..32 | ForEach-Object { [Math]::Cos($_ * 1.9) + $_ * 0.08 + ($_ % 3) * 0.3 })
    [Array]::Reverse($first); [Array]::Reverse($second)
    for ($i=0; $i -lt $extremes; $i++) { $first[$i]=1000.0+$i; $second[$i]=-1000.0-$i }
    $d.X = [double[][]]@($first,$second)
    $d.varNames = [string[]]@("B'eta",'0')
    return $d
}
function Snapshot($table) {
    $model=$table.ToOutputModel()
    $rows=New-Object 'System.Collections.Generic.List[object]'
    for ($r=0; $r -lt $model.RowCount; $r++) {
        $cells=New-Object 'System.Collections.Generic.List[object]'
        for ($c=0; $c -lt $model.ColumnCount; $c++) {
            $value=$model.Values[$r,$c]
            $type=if ($null -eq $value) { 'Nothing' } else { $value.GetType().FullName }
            if ($value -is [double] -and ([double]::IsNaN($value) -or [double]::IsInfinity($value))) { $value=$value.ToString([Globalization.CultureInfo]::InvariantCulture) }
            $cells.Add([ordered]@{type=$type;value=$value})
        }
        $rows.Add($cells.ToArray())
    }
    return [ordered]@{rows=$rows.ToArray();headerTopRows=$model.HeaderTopRows;headerLeftColumns=$model.HeaderLeftColumns;
        titleRows=$model.TitleRows;footerRows=$model.FooterRows;leftAdjust=$table.bLeftHeaderAdjustUp;
        pvalueCells=@($model.PvalueCells | ForEach-Object { ,@($_.BodyRow,$_.BodyColumn) })}
}
$observations=New-Object 'System.Collections.Generic.List[object]'
$savedCulture=[Threading.Thread]::CurrentThread.CurrentCulture
try {
    foreach ($culture in @('','sk-SK')) {
        [Threading.Thread]::CurrentThread.CurrentCulture=[Globalization.CultureInfo]::GetCultureInfo($culture)
        foreach ($scenario in @('normality','descriptive','descriptive-empty','homogeneity','grubbs','rosner-zero','rosner-one','rosner-multiple','rosner-capacity10','symmetry-mgg','symmetry-cm')) {
            $count=if ($scenario -eq 'rosner-one') {1} elseif ($scenario -eq 'rosner-multiple') {2} elseif ($scenario -eq 'rosner-capacity10') {10} else {0}
            $before=New-Data $count; $after=New-Data $count
            $kind=[BESHStatNG.ExcelCommon.AssumptionReportKind]::Normality
            $outlier=[BESHStatNG.ExcelCommon.AssumptionOutlierMethod]::None
            $homogeneity=[BESHStatNG.ExcelCommon.HomogeneityMethods]::None
            $symmetry=[BESHStatNG.ExcelCommon.AssumptionSymmetryMethod]::MiaoGelGastwirth
            $statistics=$null
            switch ($scenario) {
                'normality' { $expected=$legacy.GetMethod('RunNormality').Invoke($null,[object[]]@($before.PSObject.BaseObject)) }
                'descriptive' { $kind='Descriptive'; $expected=$legacy.GetMethod('ComputeDescriptiveStats').Invoke($null,[object[]]@($before.PSObject.BaseObject,$null)) }
                'descriptive-empty' {
                    $kind='Descriptive';$statistics=New-Object 'System.Collections.Generic.List[string]'
                    $expected=$legacy.GetMethod('ComputeDescriptiveStats').Invoke($null,[object[]]@($before.PSObject.BaseObject,$statistics.PSObject.BaseObject))
                }
                'homogeneity' { $kind='Homogeneity';$homogeneity='All';$expected=$legacy.GetMethod('ComputeVarianceHomogeneity').Invoke($null,[object[]]@($before.PSObject.BaseObject,$true,$true,$true,$true)) }
                'grubbs' { $kind='Outliers';$outlier='Grubbs';$expected=$legacy.GetMethod('RunOutliers').Invoke($null,[object[]]@($before.PSObject.BaseObject,$true,$false,[decimal]0.05)) }
                { $_ -like 'rosner-*' } { $kind='Outliers';$outlier='Rosner';$expected=$legacy.GetMethod('RunOutliers').Invoke($null,[object[]]@($before.PSObject.BaseObject,$false,$true,[decimal]0.05)) }
                'symmetry-mgg' { $kind='Symmetry';$expected=$legacy.GetMethod('RunSymmetry').Invoke($null,[object[]]@($before.PSObject.BaseObject,$true)) }
                'symmetry-cm' { $kind='Symmetry';$symmetry='CabilioMasaro';$expected=$legacy.GetMethod('RunSymmetry').Invoke($null,[object[]]@($before.PSObject.BaseObject,$false)) }
            }
            $options=New-Object BESHStatNG.ExcelCommon.AssumptionReportOptions -ArgumentList @($kind,$outlier,[decimal]0.05,$homogeneity,$symmetry,$false,$false,$statistics)
            $prepared=[BESHStatNG.WindowsAssumptionReportAdapter]::Prepare($after,$options,[Threading.CancellationToken]::None)
            $old=Snapshot $expected; $new=Snapshot $prepared.Tables[0]
            $left=ConvertTo-Json -InputObject $old -Depth 15 -Compress
            $right=ConvertTo-Json -InputObject $new -Depth 15 -Compress
            if ($left -cne $right) { throw ('Characterization mismatch: '+$scenario+'/'+$culture) }
            $observations.Add([ordered]@{scenario=$scenario;culture=$culture;equal=$true;before=$old;after=$new})
        }
    }
} finally { [Threading.Thread]::CurrentThread.CurrentCulture=$savedCulture }
$record=[ordered]@{schema='04A06-managed-report-observations-v1';configuration=$Configuration;
    provenance='Existing Core and frozen pre-extraction source projection; NOT native; NOT independent scientific reference; NOT_GOLDEN';
    native_executed=$false;observations=$observations.ToArray()}
$json=ConvertTo-Json -InputObject $record -Depth 18
$stream=[IO.File]::Open($target,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try { $bytes=[Text.UTF8Encoding]::new($false).GetBytes($json);$stream.Write($bytes,0,$bytes.Length) } finally { $stream.Dispose() }
Write-Host ('Passed '+$observations.Count+' before/after report observations; '+$target)
