$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
if ((Get-Location).Path -ne $repoRoot) { throw 'Run inside repository root' }
$utf8 = New-Object System.Text.UTF8Encoding($false)
function Pin([string]$p) {
    $f = Get-Item -LiteralPath (Join-Path $repoRoot $p)
    [ordered]@{ path=$p; bytes=$f.Length; sha256=(Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
}
function Read-Json([string]$p) { Get-Content -LiteralPath (Join-Path $repoRoot $p) -Raw | ConvertFrom-Json }
function Assert-Pin($r) {
    $a = Pin $r.path
    if ($a.bytes -ne $r.bytes -or $a.sha256 -cne $r.sha256) { throw ('Pin mismatch: '+$r.path) }
}
function Git-Read([string[]]$a) {
    $out = & git -c ('safe.directory='+$repoRoot.Replace('\','/')) --no-optional-locks @a
    if ($LASTEXITCODE -ne 0) { throw 'Read-only Git command failed' }
    $out
}
function Save-New([string]$name,$doc) {
    $path = Join-Path $PSScriptRoot $name
    $bytes = $utf8.GetBytes(($doc | ConvertTo-Json -Depth 50)+'`n'.Replace('`n',"`n"))
    $stream = [System.IO.File]::Open($path,[System.IO.FileMode]::CreateNew,[System.IO.FileAccess]::Write)
    try { $stream.Write($bytes,0,$bytes.Length) } finally { $stream.Dispose() }
}
function Changed-Fields($a,$b,[string]$p) {
    if ($null -eq $a -or $null -eq $b) { if ($a -ne $b) { $p }; return }
    if ($a -is [System.Management.Automation.PSCustomObject]) {
        if (($a.PSObject.Properties.Name -join '|') -cne ($b.PSObject.Properties.Name -join '|')) { throw 'Changed object shape' }
        foreach ($n in $a.PSObject.Properties.Name) { Changed-Fields $a.$n $b.$n ($p+'/'+$n) }; return
    }
    if ($a -is [System.Array]) {
        if ($a.Count -ne $b.Count) { throw 'Changed array shape' }
        for ($i=0;$i -lt $a.Count;$i++) { Changed-Fields $a[$i] $b[$i] ($p+'/'+$i) }; return
    }
    if ($a -cne $b) { $p }
}
$expectedMain = 'bba848305c2c46ff3fc835a1132522ebfec59514'
$expectedIndex = '1ba1e0ac644200d867cb5b171cbc9c883193780f0be5dae687a681c30208d700'
$branch = Git-Read @('branch','--show-current')
foreach ($ref in @('HEAD','main','codex/04A-40')) {
    if ((Git-Read @('rev-parse',$ref)) -cne $expectedMain) { throw 'Git reference mismatch' }
}
if ($branch -cne 'codex/04A-40' -or (Git-Read @('merge-base','main','codex/04A-40')) -cne $expectedMain) { throw 'Branch/merge-base mismatch' }
if (@(Git-Read @('diff','--cached','--name-only')).Count -ne 0) { throw 'Index staged' }
if ((Pin '.git/index').sha256 -cne $expectedIndex) { throw 'Index byte mismatch' }
Git-Read @('diff','--check')
$modified = @(Git-Read @('diff','--name-only'))
$untrackedEntry = @(Git-Read @('ls-files','--others','--exclude-standard'))
$beforeStatus = @(Git-Read @('status','--short','--untracked-files=all'))
$pairs = @(
    [ordered]@{raw='evidence/04A40/baseline/Debug-build-v1.json';size=172325;hash='c2fe841a71b4b859358a5aced18ec71aedc03c07d6dc23aced051f9e8e75835f';safe='evidence/04A40/baseline/Debug-build-review-v2.json';safeSize=172293;safeHash='4f04ce1e02d3e0241f15460c88ec4c4f41d67b102ddd744efef1212ae83eacea';field='/output';categories=@('known_private_identifier','profile_or_home','appdata_or_drive_path')},
    [ordered]@{raw='evidence/04A40/entry/tool-runtime-final-v1.json';size=6401;hash='4ef9143dc7241ce08da3322607ba30e6c0b4581eb643194ec0b7c2d065d67f54';safe='evidence/04A40/entry/tool-runtime-review-v2.json';safeSize=6364;safeHash='84dc88b1a9fdb13a7f87a49226a5d7fa779027b12aaf45ba6840f09e8842478c';field='/commands/0/stdout';categories=@('unc_share')}
)
$crosswalk = @()
foreach ($pair in $pairs) {
    Assert-Pin ([pscustomobject]@{path=$pair.raw;bytes=$pair.size;sha256=$pair.hash})
    Assert-Pin ([pscustomobject]@{path=$pair.safe;bytes=$pair.safeSize;sha256=$pair.safeHash})
    if ($untrackedEntry -cnotcontains $pair.raw -or $untrackedEntry -cnotcontains $pair.safe) { throw 'Actual candidate membership differs' }
    $raw = Read-Json $pair.raw; $safe = Read-Json $pair.safe
    $fields = @(Changed-Fields $raw $safe '')
    if ($fields.Count -ne 1 -or $fields[0] -cne $pair.field) { throw 'Unexpected representation change' }
    $copyPath = '.test-rationalization-04A40.tmp/protected-public-raw/'+$pair.raw.Substring('evidence/04A40/'.Length)
    $copy = $null
    if (Test-Path -LiteralPath (Join-Path $repoRoot $copyPath)) {
        Assert-Pin ([pscustomobject]@{path=$copyPath;bytes=$pair.size;sha256=$pair.hash}); $copy=Pin $copyPath
    }
    $crosswalk += [ordered]@{
        original=Pin $pair.raw; original_mtime_utc=(Get-Item -LiteralPath $pair.raw).LastWriteTimeUtc.ToString('o')
        original_status='PROTECTED_RAW_UNTRACKED_NONIGNORED;UNCHANGED;HISTORICAL_PRIVACY_FAIL_RETAINED'
        existing_protected_copy=$copy; separate_copy_verification= $(if ($null -ne $copy) {'BYTE_EXACT_COPY_AUTHENTICATED'} else {'NOT_VERIFIED_AT_THIS_EXPECTED_PATH;NO_MISSING_GLOBAL_PROTECTION_CLAIM'})
        representation=Pin $pair.safe; kind='SANITIZED_REVIEW_CANDIDATE_REPRESENTATION;NOT_ORIGINAL_EXECUTION_TRANSCRIPT'
        changed_json_fields=$fields; command_arguments_and_other_json_values_exact=$true
        transformation= $(if ($pair.field -ceq '/output') {'Build output-directory metadata rendered for review; raw build stdout and command arguments unchanged.'} else {'Only dotnet-info rendered stdout differs; tool paths use review aliases/forward separators. Command stdout_sha256 still identifies protected raw stdout, not the rendered string.'})
        rule_categories_from_retained_original_scan=$pair.categories; fresh_scanner_status='NOT_RUN_PYTHON_UNAVAILABLE'
        authority='Owner authorization quoted in decision-matrix-v1.json; only these two report representations and minimal necessary references.'
        candidate_remedy='STOPPED_BEFORE_CHANGE;RAW_PATH_STILL_SELECTED_BY_UNCHANGED_SCANNER'
    }
}
$freeze = Read-Json 'evidence/04A40/matrix/source-freeze-v2.json'
$sourceCount=0
foreach ($p in $freeze.source_pins.PSObject.Properties) {
    if ((Pin $p.Name).sha256 -cne $p.Value) { throw ('Source mismatch: '+$p.Name) }; $sourceCount++
}
foreach ($r in $freeze.new_tests) { Assert-Pin $r }
$scanners=@()
foreach ($s in @(@('13',9176,'b26596e5bf9644e81a479fad4f28d3db8d8af23611175b8e582c759007009a03'),@('14',9320,'b4b4fd3567947d807c73e63022a0128ded37199248d0067f786cb472811d5a25'))) {
    $path='scripts/verify-04a'+$s[0]+'-publication.py'
    Assert-Pin ([pscustomobject]@{path=$path;bytes=$s[1];sha256=$s[2]})
    $scanners += [ordered]@{phase=$s[0];source=Pin $path;fresh_scan='NOT_RUN';reason='Python app alias cannot execute in sandbox; escalated diagnostic confirms Python not found. No installation attempted.'}
}
$diagnostics=Read-Json 'evidence/04A40/python/diagnostic-dispositions-v3.json'
$parent=Read-Json 'evidence/04A40/python/parent-event-reconciliation-v2.json'
$guards=@()
foreach ($r in $parent.summary.parent_changes) {
    Assert-Pin $r.source.source
    $event=@($diagnostics.rows | Where-Object { $_.identity -ceq $r.id })
    if ($event.Count -ne 1 -or $r.outcome -cne 'FAIL') { throw 'Retained guard mismatch' }
    $paths=@($modified | Where-Object { $_ -clike 'BESHstatNG/src/ExcelUDFs/*' })
    if ($r.id -clike 'test_04a04_alignment.*') { $paths=@($modified | Where-Object { $_.ToUpperInvariant().Contains('UDF') -or $_.EndsWith('.dna') }) }
    $guards += [ordered]@{identity=$r.id;retained_current_status='FAIL';fresh_execution='NOT_RUN_PYTHON_UNAVAILABLE';source=Pin $r.source.source.path;method_lines=$r.source.lines;current_diagnostics=$event[0].current;earlier_passing_snapshot_diagnostics=$event[0].before;triggering_actual_git_diff_paths=$paths;applicability='Historical batch no-UDF-change assertion is violated by explicitly authorized 04A.40 UDF edits. This scope explanation does not change the current FAIL, waive it, or establish scientific/export/native readiness.';waived=$false}
}
$historyPaths=@('planning/BESHStatNG_CrossPlatform_MasterPlan.md','evidence/04A40/final/owner-review-v2.md','evidence/04A40/final/readiness-postseal-v3.json','evidence/04A40/python/repository-privacy-witness-v2.json','evidence/04A40/register/issue-delta-v4.json','evidence/04A40/preservation/historical-readback-v4.json','evidence/04A40/publication/physical-readback-v1.json','evidence/04A40/final/git-final-v3.json','evidence/04A40/baseline/Debug-build-review-v2-provenance.json','evidence/04A40/publication/narrow-renderings-v1.json','evidence/04A40/python/diagnostic-dispositions-v3.json','evidence/04A40/python/parent-event-reconciliation-v2.json')
$historyPins=@($historyPaths | ForEach-Object { Pin $_ })
Save-New 'raw-presentation-crosswalk-v1.json' ([ordered]@{status='AUTHENTICATED_BYTES_AND_JSON_FIELD_SCOPE;REMEDY_STOPPED';artifacts=$crosswalk;no_original_mutated=$true;new_protected_copies_created=0;historical_provenance=$historyPins})
Save-New 'scope-guard-dispositions-v1.json' ([ordered]@{status='RETAINED_FAILS;STATIC_SOURCE_AND_LIVE_DIFF_REVIEW_ONLY';guards=$guards;current_differences_retained=32;historical_differences_retained=54;Python_parents=$parent.summary.parent_outcomes;Python_events=$parent.summary.event_outcomes;no_assertions_changed=$true})
Save-New 'git-source-readback-v1.json' ([ordered]@{utc=[DateTime]::UtcNow.ToString('o');branch=$branch;HEAD=$expectedMain;main=$expectedMain;merge_base=$expectedMain;accepted39='f2eb060c9c422c1fec4e82989d07907dbd4b9337';index=Pin '.git/index';staged_empty=$true;diff_check_exit=0;source_pins_verified=$sourceCount;production_transitions_retained=15;new_tests=@($freeze.new_tests);modified=$modified;status_before_checkpoint=$beforeStatus;source_freeze=Pin 'evidence/04A40/matrix/source-freeze-v2.json';preservation_scope='468/8984/1353 historical counts retained from pinned receipt; full protected-path rehash NOT_RUN after stop boundary. 788 recorded ID drifts and NOT_VERIFIED continuity remain. W3 37484/154 inherited qualified.';historical_pins=$historyPins})
Save-New 'decision-matrix-v1.json' ([ordered]@{
    status='NOT_READY_04A40';kind='ADDITIVE_ADVERSE_BOUNDARY_CHECKPOINT;NOT_FINAL_ACCEPTANCE_PACKAGE'
    authorization_exact='I authorize versioned, sanitized **Git-candidate representations** of the two newly created reporting artifacts.'
    authorization_scope='Only Debug-build-v1.json and tool-runtime-final-v1.json representations, minimal necessary references and additive verification/provenance. No separate longer draft authority asserted.'
    authorized_operations_performed=@('Read-only Git/index/source/scanner-pin authentication','Authenticated both existing raw reports and both prepared sanitized review representations','Compared JSON fields without publishing raw values','Added this adverse checkpoint and provenance reports')
    representation_change='STOPPED_AT_EXACT_ADDITIONAL_BOUNDARY;NO_EXISTING_REPORT_OR_REFERENCE_CHANGED'
    blocker='Both protected raw originals are untracked, nonignored, selected by both original paths() functions. Adding sanitized counterparts/references cannot remove raw findings. Clearing candidate membership requires an additional exact raw-path operation, prohibited by current authority.'
    decision_requested='Authorize removal of only the two named raw files from their repository working-tree paths after new immutable, byte-exact separately pinned copies are verified in the existing controlled owner-local store, retaining original-path provenance and all failed snapshots. No scanner or ignore rule changes.'
    environment='Fresh original scanner, identity controls and Python guard execution NOT_RUN: Python unavailable. This is an execution limitation, not a passing privacy result.'
    retained_privacy='Both current repository snapshots FAIL: eight unique occurrences (six build receipt, two tool metadata); repeated views/scanners not added. Earlier full Python snapshot six each. Historical39 parents FAIL 1941 each (1599+342); W3 two ERROR parents unwaived.'
    physical_zip_provenance='Previous physical ZIP uses review-rendered bytes at indexed raw-looking paths, explicitly recorded by physical-readback-v1.json. Its PASS does not scan or replace repository raw bytes. No fresh physical scan or ZIP rehash claimed here.'
    pending=@('Actual candidate privacy remedy and two fresh unchanged-scanner runs with identity controls','Full preservation readback and retained ABI/compatibility authentication','Targeted reader checks','New small final physical witness and external delivery receipt')
    retained_campaigns='Full Debug 21125 PASS/14 FAIL; targeted Release 3108 PASS/2 FAIL; full Release NOT_RUN; Python 830 PASS/115 FAIL/11 ERROR parents; Node 916 PASS; 268 exports and 29424 cases/config retained, not rerun here.'
    gates='P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A41_NOT_AUTHORIZED'
    no_stage_commit_merge_rebase_push_release_native_scientific_changes=$true
})
$reports=@('record-boundary-v1.ps1','raw-presentation-crosswalk-v1.json','scope-guard-dispositions-v1.json','git-source-readback-v1.json','decision-matrix-v1.json','candidate-scope-v1.json')
$candidateAddition=@($reports | ForEach-Object { 'evidence/04A40/continuation-owner-v2-boundary-v1/'+$_ })
$nowUntracked=@(Git-Read @('ls-files','--others','--exclude-standard'))
$common=@(@($modified+$nowUntracked+$candidateAddition) | Sort-Object -Unique)
$candidateScopes=@()
foreach ($phase in @('13','14')) {
    $members=@(@($common+@('evidence/04A'+$phase+'/publication-manifest.json','evidence/04A'+$phase+'/privacy.json')) | Sort-Object -Unique)
    if ($phase -ceq '14') { $members=@($members | Where-Object { $_ -cne 'planning/BESHStatNG_CrossPlatform_MasterPlan.md' }) }
    $candidateScopes += [ordered]@{scanner=$phase;members=$members;candidate_count=$members.Count;membership_derivation='Exact unchanged paths() set expression evaluated using read-only Git output; six checkpoint paths enumerated before final write and checked again afterwards. No candidate filtered for privacy.';fresh_scan='NOT_RUN';retained_snapshot='FAIL;8_UNIQUE_RULE_OCCURRENCES';normalization='Git normalized relative paths; JSON/Unicode/base64 handling remains original scanner behavior, NOT_EXECUTED_NOW'}
}
Save-New 'candidate-scope-v1.json' ([ordered]@{status='MEMBERSHIP_AUTHENTICATED;PRIVACY_EXECUTION_NOT_RUN';scanners=$scanners;scopes=$candidateScopes;identity_controls='NOT_RUN';raw_paths_still_in_both_scopes=$true;findings_source=Pin 'evidence/04A40/python/repository-privacy-witness-v2.json';new_allowances=0;scanner_ignore_scope_changes=0})
$actual=@(@($modified+@(Git-Read @('ls-files','--others','--exclude-standard'))) | Sort-Object -Unique)
if (($actual -join "`n") -cne ($common -join "`n")) { throw 'Final candidate membership drift' }
foreach ($pair in $pairs) { Assert-Pin ([pscustomobject]@{path=$pair.raw;bytes=$pair.size;sha256=$pair.hash}) }
foreach ($r in $historyPins) { Assert-Pin ([pscustomobject]$r) }
if ((Pin '.git/index').sha256 -cne $expectedIndex) { throw 'Index changed' }
Git-Read @('diff','--check')
foreach ($report in $reports) { Pin ('evidence/04A40/continuation-owner-v2-boundary-v1/'+$report) | ConvertTo-Json -Compress }
