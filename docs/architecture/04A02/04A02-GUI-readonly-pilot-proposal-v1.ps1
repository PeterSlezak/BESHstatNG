# SOURCE PROPOSAL ONLY. NOT EXECUTED / NOT NATIVE QUALIFIED IN C06.
# Reads an already owner-launched disposable process only. Never launches Excel,
# loads an XLL, invokes a button, changes settings, closes workbooks or kills Excel.
param(
    [switch]$ExecuteApprovedReadOnlyPilot,
    [string]$CaseId,
    [string]$DecisionFile,
    [string]$TrustedOwnerDecisionSha256,
    [string]$OwnerProcessReceipt,
    [string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'
if (-not $ExecuteApprovedReadOnlyPilot) { throw 'SOURCE_ONLY_NOT_AUTHORIZED: separate N and read-only native pilot decision required.' }
if (-not $DecisionFile -or -not $TrustedOwnerDecisionSha256 -or -not $OwnerProcessReceipt -or -not $OutputDirectory) { throw 'Explicit reviewed decision/hash/process receipt/output required.' }
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
function Read-ReviewedFile([string]$relative) {
    if ([IO.Path]::IsPathRooted($relative)) { throw 'Relative repository evidence path required.' }
    $path = [IO.Path]::GetFullPath((Join-Path $repo $relative))
    if (-not $path.StartsWith($repo + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'Evidence path escapes repository.' }
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw 'Required reviewed evidence absent.' }
    $cursor = Get-Item -LiteralPath $path
    while ($cursor.FullName -ne $repo) {
        if ($cursor.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Reparse evidence path refused.' }
        $cursor = if ($cursor.PSIsContainer) { $cursor.Parent } else { $cursor.Directory }
    }
    return $path
}
$decisionPath = Read-ReviewedFile $DecisionFile
if ((Get-FileHash -LiteralPath $decisionPath -Algorithm SHA256).Hash -ne $TrustedOwnerDecisionSha256) { throw 'Decision hash differs from independently supplied owner hash.' }
$decision = Get-Content -LiteralPath $decisionPath -Raw | ConvertFrom-Json
if ($decision.status -ne 'NEW_OWNER_APPROVED_NATIVE_PILOT' -or $decision.N -ne $true -or
    $decision.case_id -ne $CaseId -or -not $decision.owner -or -not $decision.date -or -not $decision.decision_source -or
    $decision.disposable_execution_authorized -ne $true -or $decision.global_refedit_released -ne $true -or
    $decision.readonly_ui_inspection_authorized -ne $true -or $decision.readonly_com_context_check_authorized -ne $true) { throw 'Unsigned/pending/incomplete decision is not native authority.' }
# Authenticity is external: a human must supply the hash from the separately
# attributable owner decision. This script cannot verify a human signature itself.
$selfHash = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash
if ($decision.pilot_script_sha256 -ne $selfHash) { throw 'Pilot source not the approved reviewed version.' }
$plan = Get-Content -LiteralPath (Join-Path $repo 'docs\architecture\04A02\04A02-G4-native-capture-ready-v1.json') -Raw | ConvertFrom-Json
$case = @($plan.cases | Where-Object case_id -eq $CaseId)
if ($case.Count -ne 1) { throw 'Unknown or duplicate native case.' }
$case = $case[0]
$expected = @($case.deferred_obligation_ids | Sort-Object -Unique)
$actual = @($decision.obligation_ids | Sort-Object -Unique)
if (@($decision.obligation_ids).Count -ne $actual.Count -or @(Compare-Object $expected $actual).Count -ne 0) { throw 'Exact deferred-obligation release set incomplete/duplicated.' }
if (@($decision.regression_receipts).Count -eq 0 -or -not $decision.shared_edge_regression_receipt) { throw 'Executed mitigation/regression proof required, including shared edge.' }
foreach ($pin in @($decision.regression_receipts) + @($decision.shared_edge_regression_receipt)) {
    $path = Read-ReviewedFile $pin.path
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $pin.sha256) { throw 'Regression receipt hash mismatch.' }
}
$receipt = Get-Content -LiteralPath (Read-ReviewedFile $OwnerProcessReceipt) -Raw | ConvertFrom-Json
if ($receipt.case_id -ne $CaseId -or $receipt.tool_owned -ne $true -or $receipt.disposable -ne $true -or
    $receipt.no_other_workbooks_or_unapproved_addins -ne $true -or $receipt.refedit_lifetime_reviewed -ne $true -or
    $receipt.macros_and_external_prompts_absent -ne $true -or $receipt.output_empty_verified -ne $true -or
    $receipt.workbook_path -ne $decision.workbook_path) { throw 'Contaminated/unknown native process or workbook context.' }
$workbook = Read-ReviewedFile $receipt.workbook_path
if ((Get-FileHash -LiteralPath $workbook -Algorithm SHA256).Hash -ne $receipt.workbook_sha256) { throw 'Disposable workbook hash mismatch.' }
$excelProcess = Get-Process -Id ([int]$receipt.process_id)
$currentSession = (Get-Process -Id $PID).SessionId
if (-not [Environment]::UserInteractive -or $excelProcess.ProcessName -ne 'EXCEL' -or $excelProcess.SessionId -ne $currentSession -or
    $excelProcess.StartTime.ToUniversalTime().Ticks -ne [long]$receipt.process_start_utc_ticks) { throw 'Wrong session/PID ownership or reused PID.' }
if (@(Get-Process EXCEL | Where-Object Id -ne $excelProcess.Id).Count -ne 0) { throw 'Other Excel process exists; do not interact or terminate it.' }
if ((Get-FileHash -LiteralPath $excelProcess.Path -Algorithm SHA256).Hash -ne $receipt.excel_exe_sha256) { throw 'Unapproved Excel binary.' }
$xll = Read-ReviewedFile $receipt.xll_path
if ((Get-FileHash -LiteralPath $xll -Algorithm SHA256).Hash -ne $decision.xll_sha256) { throw 'Unapproved XLL.' }
if ($excelProcess.MainWindowHandle -eq 0) { throw 'No interactive Excel window.' }
$output = [IO.Path]::GetFullPath((Join-Path $repo $OutputDirectory))
if (-not $output.StartsWith((Join-Path $repo 'TestResults')+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'New protected TestResults output required.' }
if (Test-Path -LiteralPath $output) { throw 'Output directory already exists; never overwrite or reuse.' }
$ancestor = Get-Item -LiteralPath ([IO.Path]::GetDirectoryName($output))
while ($ancestor.FullName -ne $repo) {
    if ($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Reparse output ancestor.' }
    $ancestor = $ancestor.Parent
}
[IO.Directory]::CreateDirectory($output) | Out-Null
# Native API boundary starts ONLY after all explicit prerequisites above.
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
# Read-only COM context verification is a separate explicit part of the future N
# decision. GetActiveObject never launches Excel; validate the returned PID/context.
# Do not ReleaseComObject a borrowed application, Quit it, or change a setting.
$app = [Runtime.InteropServices.Marshal]::GetActiveObject('Excel.Application')
$appRoot = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$app.Hwnd)
if ($appRoot.Current.ProcessId -ne $excelProcess.Id -or $app.Workbooks.Count -ne 1) { throw 'Unexpected Excel process/workbook contamination.' }
$activeBook = $app.ActiveWorkbook
if ($null -eq $activeBook -or [IO.Path]::GetFullPath([string]$activeBook.FullName) -ne [IO.Path]::GetFullPath($workbook)) { throw 'Active workbook is not the approved disposable workbook.' }
if (-not $activeBook.Saved) { throw 'Unreviewed unsaved workbook edits; make a new owner-reviewed snapshot/receipt.' }
# Modeless WinForms may be a top-level owned window, not an Excel-main-window
# descendant. Restrict top-level enumeration to the exact reviewed process ID.
$processCondition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty,[int]$excelProcess.Id)
$windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children,$processCondition)
if ($windows.Count -eq 0 -or $windows.Count -gt 12) { throw 'Missing/unexpected owned windows.' }
$rows = @()
foreach ($window in $windows) {
    if ($window.Current.ClassName -eq '#32770') { throw 'Unexpected modal dialog; do not dismiss or retry automatically.' }
    $elements = $window.FindAll([System.Windows.Automation.TreeScope]::Subtree,[System.Windows.Automation.Condition]::TrueCondition)
    if ($elements.Count -gt 1500) { throw 'Unexpected UI tree size; retain partial evidence, do not retry same output.' }
    for ($i=0; $i -lt $elements.Count; $i++) {
        $element = $elements.Item($i)
        $state = $element.Current
        $toggle = $null; $selected = $null; $value = $null; $pattern = $null
        if ($element.TryGetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern,[ref]$pattern)) { $toggle = $pattern.Current.ToggleState.ToString() }
        $pattern = $null
        if ($element.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern,[ref]$pattern)) { $selected = $pattern.Current.IsSelected }
        $pattern = $null
        if ($element.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern,[ref]$pattern)) { $value = $pattern.Current.Value }
        $rows += [pscustomobject]@{Window=$window.Current.Name;AutomationId=$state.AutomationId;Name=$state.Name;ControlType=$state.ControlType.ProgrammaticName;Enabled=$state.IsEnabled;Offscreen=$state.IsOffscreen;Bounds=$state.BoundingRectangle.ToString();Toggle=$toggle;Selected=$selected;Value=$value}
    }
}
$record = [pscustomobject]@{Schema='04A02-native-readonly-ui-candidate-v1';CaseId=$CaseId;Status='CAPTURED_NOT_VERIFIED_NOT_GOLDEN';ProcessId=$excelProcess.Id;DecisionSha256=$TrustedOwnerDecisionSha256;Rows=@($rows);NoButtonInvoked=$true;NoWorkbookWrite=$true}
$file = [IO.File]::Open((Join-Path $output 'readonly-ui.json'),[IO.FileMode]::CreateNew)
$writer = New-Object IO.StreamWriter($file,(New-Object Text.UTF8Encoding($false)))
try { $writer.Write(($record | ConvertTo-Json -Depth 6)) } finally { $writer.Dispose() }
# UIA provider calls may hang: use a separately approved external supervisor with
# a short deadline which terminates ONLY its own PowerShell helper PID. Never kill
# arbitrary Excel; operator closes only the owned disposable Excel after review.
