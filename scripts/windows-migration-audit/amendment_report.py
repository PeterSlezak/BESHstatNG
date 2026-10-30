"""Render the candid 04A.01 amendment, preservation manifest and proposed staging list."""
from __future__ import annotations
import collections
import json
import re
import xml.etree.ElementTree as ET
import catalog
import semantic
from amendment import SNAPSHOT, git, read, rows, sha

ROOT, OUT = catalog.ROOT, catalog.DEFAULT


def write(name, content):
    (OUT / name).write_text(content.rstrip()+"\n", encoding="utf-8", newline="")


def log_text(path):
    data=path.read_bytes()
    return data.decode("utf-16" if data.startswith((b"\xff\xfe",b"\xfe\xff")) else "utf-8-sig", errors="replace")


def validations():
    runs_path=SNAPSHOT/"validation-runs.json"
    runs=read(runs_path) if runs_path.exists() else []
    counters,logs=[],[]
    for config in ("Debug","Release"):
        for name in ("Core.trx","Common.trx","OfficeAddin.trx","OfficeExecution.trx","Legacy.trx",f"OfficeAddin-{config}.trx"):
            path=SNAPSHOT/config/name
            if path.exists():
                counter=ET.parse(path).find(".//{*}Counters")
                counters.append({"configuration":config,"suite":name,**counter.attrib,"path":path.relative_to(ROOT).as_posix(),"sha256":sha(path),
                                 "distinct_full_suite":name!=f"OfficeAddin-{config}.trx"})
        for kind in ("full","foundation"):
            path=SNAPSHOT/config/(kind+".log")
            if path.exists():
                text=log_text(path)
                logs.append({"configuration":config,"kind":kind,"path":path.relative_to(ROOT).as_posix(),"sha256":sha(path),
                             "python_suite_counts":re.findall(r"Ran (\d+) tests? in",text),
                             "node_suite_counts":re.findall(r"(?:# |\u2139 )tests (\d+)",text),
                             "node_skip_counts":re.findall(r"(?:# |\u2139 )skipped (\d+)",text)})
    checks=read(SNAPSHOT/"final-checks.json") if (SNAPSHOT/"final-checks.json").exists() else []
    result={"scope":"Fresh amendment runs, separate from initial-audit historical results", "runs":runs,"managed":counters,"python_node":logs,"checks":checks,
            "audit_build_commands":[{"command":"dotnet build scripts/windows-migration-audit/WindowsMigrationAudit.csproj --configfile scripts/windows-migration-audit/NuGet.Config -v minimal","initial_sandbox_exit":1,"approved_rerun_exit":0,"warnings":0,"errors":0,"output_location":"conversation tool output; SDK sentinel denial then successful local SDK-only restore/build"},
                                    {"command":"dotnet build scripts/windows-migration-audit/WindowsMigrationAudit.csproj --no-restore -v minimal","exit_code":0,"warnings":0,"errors":0,"output_location":"conversation tool output; final analyzer compilation after semantic/control guard changes"}],
            "toolchain":"Installed .NET SDK 10.0.403; SDK-only analyzer restore with cleared sources, subsequent build --no-restore. Product suites use existing local packages/cache with NuGetAudit=false.",
            "initial_build_side_effect":"Initial sandboxed dotnet build failed at C:/Users/peter/.dotnet SDK toolpath sentinel. Approved rerun succeeded and .NET first-run output reported generating an ASP.NET development certificate. Later runs set DOTNET_GENERATE_ASPNET_CERTIFICATE=false and DOTNET_CLI_TELEMETRY_OPTOUT=1. No installer, package download, Excel, signing, publishing or deployment command was executed.",
            "not_run":["Native Excel/COM/XLL execution and actual Ribbon/RefEdit dispatch","Authenticated Office/browser or Mac runtime","04A.02 numerical golden capture","Installer/signing/deployment"],
            "audit_development_notes":["A local transcription asserted a wrong attribute-start line (12 versus11) and stopped before writing decisions; corrected to source span.",
                                       "Initial local wiring transcription used an unnecessarily expensive join, was stopped and replaced with an indexed join before writing decisions.",
                                       "The proposed-path ignore checker initially used Windows text-mode stdin, adding CR to Git filenames. Switched to binary NUL-delimited git check-ignore input/output; ignore policy itself was unchanged."]}
    write("amendment-validation.json",json.dumps(result,indent=2))
    return result


def main():
    before=read(SNAPSHOT/"before.json")
    original=SNAPSHOT/"original/docs/architecture/WindowsMigrationLedger"
    old_review=read(original/"review-coverage.json")
    old_symbols=rows(original/"symbols.csv")
    coverage=read(OUT/"coverage.json")
    review=read(OUT/"review-coverage.json")
    sem=read(OUT/"semantic-review-coverage.json")
    symbols=catalog.load(OUT,"symbols.csv")
    edges=catalog.load(OUT,"ui-wiring.csv")
    validation=validations()
    table=[]
    for family in sem["families"]:
        previous=[s for s in old_symbols if s["family"]==family["family"]]
        table.append("| "+" | ".join(map(str,(family["family"],family["parsed_records"],sum(s["review_status"]=="manually-reviewed-static" for s in previous),
                    family["pinned_reviews"],family["qualified_source_reviews"],family["unreviewed_for_acceptance"],sum(s['disposition']=='needs-investigation' for s in previous),family["needs_investigation"])))+" |")
    candidates=[s for s in symbols if semantic.qualified(s) and s["disposition"].startswith("extract-to-")]
    write("inspected-candidates.csv",catalog.render([{k:s.get(k,"") for k in ("symbol_id","path","start_line","end_line","signature","containing_type","body_sha256","family","disposition","reusable_operation","destination","future_batch","core_common_equivalent","equivalence_scope","equivalent_evidence","separable_responsibilities","risk","exception_tags","exception_applicability","proposed_test","owner_role") } for s in candidates]))
    reason_counts=collections.Counter()
    for s in symbols:
        state='detailed source review' if semantic.qualified(s) else 'legacy pin needs detailed review' if s['review_status']=='manually-reviewed-static' else 'no pinned source review'
        reason_counts[s['family'],state,s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01']+=1
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=b,allocation_evidence=a,records=n) for (f,r,d,b,a),n in sorted(reason_counts.items())]))
    old_edges=rows(original/'ui-wiring.csv')
    delta_families=[]
    for f in sem['families']:
        old=[s for s in old_symbols if s['family']==f['family']]
        delta_families.append(dict(family=f['family'],old_pinned=sum(s['review_status']=='manually-reviewed-static' for s in old),new_pinned=f['pinned_reviews'],new_detailed=f['qualified_source_reviews'],
                                   old_investigations=sum(s['disposition']=='needs-investigation' for s in old),new_investigations=f['needs_investigation']))
    write('amendment-count-deltas.json',json.dumps(dict(families=delta_families,
          unpinned=dict(before=3812,after=review['unreviewed_static_records']),
          detailed_review_gaps=dict(unpinned=review['unreviewed_static_records'],legacy_detail_gaps=sem['legacy_pinned_reviews_requiring_details'],total=sem['unreviewed_for_acceptance']),
          member_investigations=dict(before=3245,after=review['member_investigations']),
          unresolved_wiring=dict(before=1710,after=review['unresolved_edge_rows'],before_by_kind=dict(collections.Counter(e['kind'] for e in old_edges if 'unresolved' in e['target_status'])),after_by_kind=sem['remaining_unresolved_edges_by_kind']),
          inspected_candidate_records=len(candidates),candidate_freeze='NOT FROZEN; incomplete audit'),indent=2))
    batch_table="\n".join(f"| {f} | {d} | {b} | {n} |" for (f,d,b),n in sorted(collections.Counter((s["family"],s["disposition"],s["future_batch"]) for s in candidates).items()))
    numeric_table="\n".join(f"| {r['configuration']} | {r['suite']} | {r.get('passed',0)} | {r.get('failed',0)} | {r.get('notExecuted',0)} |" for r in validation['managed'])
    native_table="\n".join(f"| {e['edge_id']} | {e['owner_role']} / {e['future_batch']} | {e['static_boundary']} |" for e in edges if e['target_status']=='reviewed-runtime-supplied-delegate; runtime deferred')
    distinct={config:sum(int(r.get("passed",0)) for r in validation['managed'] if r['configuration']==config and r['distinct_full_suite']) for config in ('Debug','Release')}
    log_table="\n".join(f"| {r['configuration']} {r['kind']} | {', '.join(r['python_suite_counts']) or 'none'} | {', '.join(r['node_suite_counts']) or 'see log'} | {', '.join(r['node_skip_counts']) or 'none reported'} |" for r in validation['python_node'])
    outputs=("completion-amendment.md","amendment-validation.json","amendment-artifacts.csv","inspected-candidates.csv","proposed-tracked-files.json","future-staging.txt","amendment-count-deltas.json","counts-by-reason-and-batch.csv")
    files={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    files|={(OUT/name).relative_to(ROOT).as_posix() for name in outputs}
    files|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}
    files.add("docs/architecture/WindowsMigrationLedger.md")
    files.add("docs/architecture/WindowsMigrationLedger/index.json")
    write("proposed-tracked-files.json",json.dumps({"status":"PROPOSED ONLY; no staging or commit performed","paths":sorted(files),
          "comparison":"Must equal index.json artifacts paths plus index.json itself. Existing .gitignore preserved."},indent=2))
    force=[p for p in sorted(files) if p.startswith('docs/architecture/WindowsMigrationLedger/')]
    normal=[p for p in sorted(files) if p not in force]
    write("future-staging.txt","# Proposed future owner-reviewed staging only; do not execute during this amendment.\n# Minimal force inclusion is per individually reviewed ledger file; .gitignore is unchanged.\n"+
          "\n".join('git add -f -- "'+p+'"' for p in force)+"\n# Visible audit tooling and top-level entry:\n"+"\n".join('git add -- "'+p+'"' for p in normal))
    old_index=read(original/"index.json")
    old_hashes={a['path']:a['sha256'] for a in old_index['artifacts']}
    artifact_rows=[]
    for path in sorted(files):
        existing=ROOT/path
        change='modified' if path in old_hashes and (not existing.exists() or sha(existing)!=old_hashes[path]) else 'unchanged' if path in old_hashes else 'added'
        if path=='docs/architecture/WindowsMigrationLedger/index.json':
            change='modified (self excluded from its own hash index)'
        artifact_rows.append({"path":path,"change_from_original_audit":change,"original_sha256":old_hashes.get(path,''),"current_hash_authority":"index.json (except index itself)"})
    write("amendment-artifacts.csv",catalog.render(artifact_rows))
    write("completion-amendment.md",f"""# 04A.01 completion amendment — partial, same branch

**NOT READY FOR ACCEPTANCE. No 04A.02 work or acceptance request.** The available execution/review window did not permit source-wide completion. Detailed review is {sem['qualified_source_reviews']}/{sem['denominator']}; the remaining queue must be completed in 04A.01 before owner acceptance. Candidate counts remain provisional.

Branch `{git('branch','--show-current')}`, HEAD `{git('rev-parse','HEAD')}`, main `{before['main']}`. Index and tracked worktree were clean at preflight; only prior audit tooling/top-level entry were untracked and the ledger was ignored. All work remains unstaged/uncommitted. Revision 2.1 and its exact 04A.01 contract were reread: no unclassified reusable method and a frozen candidate count at acceptance. Gate C owner GO with E1–E7 open and GC3–GC8 deferred remains unchanged; 40 accepted /183 outstanding. No product source or tracked README/project/Ribbon/catalog/MSI/manifest/framework target changed.

Original evidence: all **51** indexed artifacts and **171** source-file hashes matched. No ZIP exists under the checkout, so a fresh byte comparison against the earlier ZIP is unavailable. Actual Git HEAD is the pre-audit Core-forwarding cleanup, with **19 Windows VB paths plus one test file** changed from its parent; there are no corresponding unreported tracked worktree changes. See `source-provenance.json` and retained Git preflight in TestResults/04A-01-amendment/before.json. `baseline-manifest.json` retains all 51 original indexed artifacts plus their original index (52 files), and the separate local snapshot preserves 109 original audit/run files. Neither historical source classification nor test evidence has been discarded.

## Source review progress

The syntax denominator is unchanged: **171 compiled files, 3,866 records, {coverage['actual_profile_records']} active /{coverage['alternative_only_records']} alternative-only**, with generated and excluded-source provenance intact. There are zero parser errors. Property containers/accessors and lambdas/initializers remain separate; no scope reduction was made.

| Family | Records | Original pins | Current pins | Detailed member-v1 | Incomplete for acceptance | Original investigations | Current investigations |
|---|---:|---:|---:|---:|---:|---:|---:|
{chr(10).join(table)}

The original **3,812 unpinned** records are now **{review['unreviewed_static_records']}** ({3812-review['unreviewed_static_records']} newly pinned). Total pins:54 -> {review['manually_reviewed_static_records']}. The stronger, separately enforced denominator requires **{sem['unreviewed_for_acceptance']}** further reviews, including **{sem['legacy_pinned_reviews_requiring_details']}** older decisions needing detailed responsibility/caller/equivalence/risk/fixture evidence. **{len(sem['files_without_qualified_review'])} files have no detailed review**; generated-rule-verified count is **0**. `remaining-member-review.csv` lists each exact ID/span, owner role, review reason and provisional follow-up. Every remaining review is owed in 04A.01, even when its eventual extraction batch is later.

Member investigations: **3,245 -> {review['member_investigations']}**. `amendment-count-deltas.json` provides exact family/edge-subtype changes; `counts-by-reason-and-batch.csv` separates detailed review, missing pins and legacy-detail gaps by disposition and inspected/provisional batch. Detailed reviews now cover every record in UibyID, RegressionEffectsController, RefEdit code-behind plus its small InitializeComponent, NormalPlot, ROC, UdfScalarUtilities and AssumptionsUDFs. The other Designer bodies, most model dialogs/UDF families, DM/global/lifecycle/settings/Help/updater and remaining graphics are still completion work. A syntactically identified forwarding wrapper is not promoted to independent semantic review; three complete typed Compute facades were separately reviewed.

New detailed decisions are in `review-decisions.json`, pinned to symbol/signature/body, with meaningful dependency file hashes. Prior broad pins stay visibly separate. Static target/disposition confidence is not numerical or native parity evidence. The analyzer now rejects missing detailed member/control review in strict mode independently of needs-investigation enums, and returns1 for integrity drift before returning2 for legitimate incomplete acceptance.

## Inspected candidates and equivalent reuse

`inspected-candidates.csv` contains every **{len(candidates)} detailed inspected candidate** with exact source/body identity, operation, approved destination/batch, shape/default/parity risk, E/GC applicability and named future fixture. These include property/accessor/lambda records, so they are not a frozen count of unique migrations. Old pins lacking the detailed contract and all generic family assignments are excluded from this count.

| Family | Disposition | Approved follow-up | Inspected source records |
|---|---|---|---:|
{batch_table}

- `UI/UibyID.vb`: grouping/input roles/default descriptors ->04A.03; UTT direction/margin and NI/TOST reports ->04A.04; normality/outlier/descriptive/homogeneity reports ->04A.06; ANOVA/KW/MW supplementary workflows ->04A.07; normal/ROC ->04A.31; histogram/violin/box/categorical ->04A.32; CDF parsing/specs ->04A.34. Native lifecycle/destination/COM actions stay Windows. GUI Shapiro accepts n>3 and n<5000 while UDF accepts inclusive3..5000; retain that distinction. Homogeneity table metadata only explicitly marks the first stacked p-value column; do not silently normalize it.
- `UI/RegressionEffectsController.vb`: effect-state, categorical conflicts, degree/duplicate/hierarchy/order and interaction authoring ->04A.08 Common. Exact existing helpers are `BESHStat.Core/src/RegModels/RegressionDesignCore.vb` MakeCategoricalEffectKey(String), MakePolynomialEffectKey(String,Integer), GetCoefBaseName(String), MakeInteractionEffectKey(IEnumerable(Of String)), MakeInteractionCoefName(IEnumerable(Of String)). `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb` SpreadsheetModelInputCompiler.Compile(range,options) compiles typed designs; it does not replace mutable ListBox authoring. Each mixed action keeps UI selection/messages separate from proposed typed term changes.
- `Graphics/NormalPlot.vb`: constructor sorts caller array in place; scores and SPSS/OLS/R endpoint calculations ->04A.31 Core. Exact primitive helper signatures are pinned for NormSInv, Slope, Intercept and QuartilesComp; none is a whole NormalPlot equivalent. The renderer keeps Excel series/title/line formatting.
- `Graphics/ROC.vb`: constructor/state, cutoffs/WilcoxonAUC/Hanley-McNeil, DeLongSE and MidRanks/comparer ->04A.31 Core; two-table report ->Common. Repeated compute accumulates pAUC without resetting at entry; rank/endpoint/report indexing, ties and unclipped CIs require fixtures. `BESHStat.Core/src/RegModels/BinaryClassificationCore.vb` threshold/calibration helpers are not a full ROC replacement.
- `ExcelUDFs/AssumptionsUDFs.vb`: all12 exports are mixed validation/report/marshaling wrappers, not whole Core facades; exact estimator calls/signatures are pinned in `BESHStat.Core/src/StatTests/Assumptions.vb` (ShapiroWilk, DAgostino, AndersonDarlingTEST, BoxM, FlignerKilleenTEST, LeveneTEST, BartlettTEST, SquaredRanksTestVARIANCE, MauchlyTest, SymmetryTest, Grubbs, Rosner). Proposed typed policies/reports ->04A.41. The existing spill adapter turns marked p-values into display text; keep scalar error versus array shape and diagnostic BESH.ASSUMP versus export BESH.ASM identity.
- `ExcelUDFs/UdfScalarUtilities.vb`: typed normalization ->04A.39; mixed-model aliases/default REML/KR/AI, gradient override and covariance names/clone behavior ->04A.40. ExcelArgReaders.AsString handles Excel missing/empty; it remains host conversion. Unknown covariance names pass through. Symmetry full-name aliases lose hyphens before comparison; short aliases work. Record compatibility fixtures before any correction.
- UibyID ANOVA/KW primary calculation really delegates through `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb` Run(SpreadsheetAnalysisRequest), then typed `AnalysisHandlerRegistry`; supplementary reports and native writes make these mixed methods. Three complete forwarding facades are individually verified: CategoricalHistogram.Compute(Double(),Array,Options), ViolinPlot.Compute(Double(),Array,Options), CumulativeDistributionPlot.Compute(Array,String,Options), each to the corresponding `BESHStat.Core/src/Graphics/*Core.vb` Compute with identical typed arguments/defaults/return contract and no other statements. UibyID request/appearance/COM orchestration remains mixed. Whole-method equivalent claims in those mixed rows: **0**. Exact helper signatures/paths and source hashes are in the decision store.

All47 approved rows **04A.02–04A.48** remain in `batch-map.csv`. It now separates inspected candidates from unreviewed provisional rows. No histogram PrettyBreaks/04A.75, extra UDF, graph feature, extraction or Office registration was implemented.

## UI wiring and intent

All **2,056 UI WithEvents declarations and 19,701 wiring/configuration rows** remain, including 16,863 configuration assignments. Configuration is not presented as callable event proof. The original control census remains separately visible in `baseline_control_states`: **389 direct Handles /864 indirect-review candidates /803 decorative-container candidates**. There are **{sem['independently_checked_control_intents']}** independently checked indirect/dynamic controls (26 Help buttons,28 PSM/SPC role buttons); **{sem['unreviewed_control_intents']}** controls still lack an independent intent pin. Full control contracts also require default, enabled/reset, validation/output and option interpretation evidence. **{sem['qualified_full_control_contracts']}/2,056** currently satisfy that stronger contract; the54 routing pins alone do not satisfy it. This distinction is independently enforced by strict acceptance and a negative fixture; per-control option_contract states the limited review scope.

**1,710 -> {review['unresolved_edge_rows']} unresolved wiring edges.** All80 original dynamic/external edges now have individual source/dependency pins:26 WireHelp extension call sites,34 inline AddHandler lambdas,8 typed Excel lifecycle attach/detach edges and12 other typed invocation sites. Of the latter,7 are precise caller-supplied delegate boundaries; the other5 have exact local/AddressOf targets. The26 Help WithEvents declaration edges changed from unknown to indirectly handled. The28 role-button declaration edges already had broad-prefix matches and therefore add review evidence without reducing the old unknown count again. In total134 pinned edge decisions;106 previously unresolved rows resolved. The remaining **{review['unresolved_edge_rows']}** are WithEvents intent edges; none of the original80 remains a blanket unknown.

The tables' original no-Handles counts differ because **1,667 UI controls without direct Handles -37 UI controls with other prefix-matching edges =1,630 no-Handles-found edges**. Those37 are28 role buttons,5 PSM option combos,3 SPC tab/split-container dispatch sites and RefEdit.xlSheet; the actual IDs are in semantic-review-coverage.json. The broad matched-edge count429 is389 direct-Handles UI controls +37 other UI prefix matches +3 non-UI pChart fields (BulletChartExcel, Histogram and SankeyPlotExcel). All three non-UI fields already match handlers and therefore do not add unknown edges. Prefix matching is not an independent intentionality review.

RefEdit SelectionChange attach sites pair with Leave, sheet-change, stale-reference and Disposed detaches; repeated Enter, connector cleared before disposal and queued WriteData need named later lifecycle fixtures. RefEdit.xlApp has no live Handles clause (the workbook activation code is commented); its unused-versus-future ownership still needs its own control intent decision. ImageMaximized Nothing falls back to imgMinimized as written. WindowsDocumentLifecycle has four paired Add/Remove delegate sites, a listening guard, per-detach catches and partial-attach unwind. Help closures have no explicit detach and share form/control lifetime; source proof does not certify runtime delivery.

Precise runtime-only boundaries (source-classified; no native/parity claim):

| Source edge ID | Responsible owner role / batch | Required fixture / static boundary |
|---|---|---|
{native_table}

## Four authorities and acceptance blockers

Authored methods.yml:76 methods/5 commands/2 pages. Ribbon XML:84 button IDs/81 distinct action declarations, with reused callbacks retained in ribbon-crosswalk.csv. Inherited LoadImage remains separately bound to the installed Excel-DNA assembly hash and declaration, not live proof. ExcelFunction declarations:268 exports in23 exporting plus18 helper-only files. Registration/capability checks still permit Office ANOVA/KW only; metadata/buttons add no execution. `catalog-coverage.json` explicitly lists missing/orphan/unbound cases; the source crosswalks and all public signatures are unchanged.

Acceptance blockers are unreviewed substantive ownership, legacy pins missing details, remaining control/default/validation intent, and provisional batch allocations. These must be resolved in04A.01; the actionable source-ID queue is `remaining-member-review.csv` and the residual edge IDs are in coverage.json/investigations.csv. Appropriate runtime-only boundaries above defer named tests; they do not themselves demand native Excel execution to classify source. No source-wide completion, candidate freeze or owner acceptance is claimed. The next review pass should finish UibyID control contracts/remaining28 Designer bodies, model dialogs and their Core/Common request comparisons, then all remaining UDF/graphics/DM/global families and the legacy36 detailed-review gaps. Do not begin04A.02.

## Fresh validation and preservation

Exact commands, outcomes, log/TRX hashes and skips: `amendment-validation.json`. Full independent suite commands ran via `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/windows-migration-audit/run-amendment-validation.ps1`; for each Debug/Release it invokes verify-beshstatng.ps1 with `-VisualStudio2026 -Legacy -Wasm -Js -Python -PythonExe <installed interpreter>` and verify-office-foundation.ps1. All four script invocations exited0. Distinct full managed passed counts: **Debug {distinct['Debug']}; Release {distinct['Release']}**, excluding duplicate foundation suite reruns. Original pass results remain under baseline/ and TestResults/04A-01, not used as a substitute for these runs.

| Configuration | Suite | Passed | Failed | Not executed |
|---|---|---:|---:|---:|
{numeric_table}

| Command | Python suite counts | Node suite counts | Node skips |
|---|---|---|---|
{log_table}

Audit source --check, catalog --check, parser fixtures, negative tests and --check --strict run via check-amendment.ps1. Strict must exit2/NOT READY; data corruption exits1. Fixtures cover removal/body-hash drift of pinned decisions, generic substantive re-triage, missing source/member/export/actual UI handler/Ribbon/owner, reverted dynamic resolution and control-source pin drift. Report checks use report.py --check. The original ten parser assertions are preserved with four semantic guard assertions; no denominator change or parser scope reduction occurred.

Protected hashes: **2,109/2,109 tracked paths** and **1,543/1,543 Gate C files** unchanged at preflight and after every suite; retained109-file original local snapshot also matches. Generated VersionAttributes changes were preserved and restored only from hash-verified originals. Product source, Directory.Build.props, RibbonXML, methods.yml, generated Help/assembly metadata, MSI/manifests and tracked READMEs remain byte-identical. Initial dotnet build hit a sandbox SDK sentinel denial; the approved rerun passed but reported a first-run ASP.NET development certificate generation. That side effect is disclosed separately from product validation; later runs disabled it. No live Excel, installer, publishing/deployment/signing, package download or04A.02 golden capture was run.

Future native smoke proposals only: open UibyID ANOVA/KW, toggle supplementary reports/default alpha and each output destination, cancel/reopen and switch/delete source sheets; separately sample assumptions UDF error/spill contracts and ROC/normal plot input-order/series against owner-pinned settings. WASM smoke: confirm only ANOVA/KW remain executable and capability-disabled methods remain disabled. None was executed in this amendment.

## Git visibility and review artifacts

`amendment-artifacts.csv` lists every audit artifact/tooling path with added/modified/unchanged status against the original51-entry index. `proposed-tracked-files.json` must exactly equal index.json's artifact paths plus index.json itself; it includes all retained baseline files. `future-staging.txt` lists the exact future `git add -f -- <individual ledger file>` commands and normal staging for visible tooling/top-level entry. These are review instructions only: **nothing was staged or committed**, and .gitignore is unchanged. Do not omit the ignored ledger from a future PR. Owner review and separate authorization are required before any next batch.
""")
    print(json.dumps({"status":"NOT READY FOR ACCEPTANCE","pinned":review['manually_reviewed_static_records'],"detailed":sem['qualified_source_reviews'],
                      "member_investigations":review['member_investigations'],"unresolved_edges":review['unresolved_edge_rows'],"managed_distinct":distinct,"proposed_paths":len(files)},indent=2))


if __name__=='__main__':
    main()
