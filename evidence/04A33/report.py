"""One owner dossier; sanitized final Git/privacy receipts and deterministic publication list."""
from finalize import *
def checks():
 assert git('branch','--show-current')=='codex/04A-33'
 assert git('rev-parse','HEAD','main','HEAD^').splitlines()==[BASE,BASE,'43a752b169fb996ed65387d88d380b1386e92240']
 assert git('merge-base','HEAD','main')==BASE
 assert not git('diff','--cached','--name-only')
 assert not git('diff','--check')
 assert pin(OWNER_PLAN)['sha256']==OWNER_PLAN_SHA
 save('git-final.json',dict(schema='04A33-git-final-v1',branch=git('branch','--show-current'),head=BASE,main=BASE,merge_base=BASE,index='EMPTY',diff_check='PASS',tracked_changes=git('diff','--name-only').splitlines(),untracked=git('ls-files','--others','--exclude-standard').splitlines(),external_owner_change='planning/BESHStatNG_CrossPlatform_MasterPlan.md;preserved revision2.12 outside batch publication',publication='NO_STAGE_NO_COMMIT_NO_MERGE_NO_PUSH_NO_DEPLOY',next_batch='NOT_STARTED'))
def report():
 v=load(HERE/'validation.json');n=v['node'];p=v['python'];m=v['managed_totals'];cb=load(HERE/'compatibility-casebook.json')
 suite_rows=[]
 for suite in SUITES:
  d=next(r for r in v['managed'] if r['suite']==suite and r['configuration']=='Debug');r=next(r for r in v['managed'] if r['suite']==suite and r['configuration']=='Release')
  fmt=lambda x:str(x.get('Passed',0))+'/'+str(x.get('Failed',0))+'/0/0'
  suite_rows.append('|'+suite+'|'+fmt(d['baseline_debug'])+'|'+fmt(d['outcomes'])+'|'+fmt(r['outcomes'])+'|')
 files='\n'.join('- `'+x+'`' for x in WINDOWS+CORE+TESTS+[PROJECT])
 failures=load(HERE/'nonpasses.json')['nonpasses'];failures=[x for x in failures if x['configuration']=='Debug']
 fail_list='\n'.join('- `'+x['suite']+': '+x['class_name']+'.'+x['method']+'` — '+x['test_name'] for x in failures)
 audit=v['new_audits']
 audit_text=json.dumps(audit,sort_keys=True) if isinstance(audit,dict) else audit
 body=f'''# Batch 04A.33 owner review

**{v['decision']}** — portable geometry/software compatibility only. Blockers: {('; '.join(v.get('blockers',[])) or 'none')}.

The actual Windows XYZ, scatter-matrix and wire-object adapters consume source-backed Core coordinate payloads. No new projection convention, interpolation family, statistical formula, renderer or UI feature was introduced. Software compatibility is **NOT_GOLDEN** and does not establish scientific accuracy or native chart appearance.

Actual merged-main base and current feature HEAD: `{BASE}`. Branch: `codex/04A-33`; merge-base is the same SHA. Parent is `43a752b169fb996ed65387d88d380b1386e92240`. Main contains the accepted 04A.32 implementation and evidence. Entry index/worktree were clean after the owner merge appeared during initial inspection;branch creation followed a stable recheck. No archive/old branch supplied authority. Git HEAD has not moved during implementation. Index remains **EMPTY**.

An external writer updated the Master Plan from2.11 to2.12 during validation. Codex never wrote that path. The update now reports acceptance through32 and retains the official33 objective/gates. It is preserved as an unrelated owner change,excluded from this batch's publication manifest,and pinned in [external-owner-change.json](../../evidence/04A33/external-owner-change.json). This qualifies the plan-preservation statement;it is not a silent baseline rewrite. No other owner collision was found.

Changed/new implementation and test files:

{files}

Only the three Windows graphics files and Framework test project modify tracked batch source. Five Core files and five test/fixture files are additive. Core/Common remain netstandard2.0;Windows/Test remain net48. Common is unchanged. No UI/Designer,Ribbon,UDF,OfficeAddin,WasmSmoke,installer,target,configuration,package,signing or file-output change is included.

The [entry operation map](../../evidence/04A33/operation-map-entry.csv) and [final operation map](../../evidence/04A33/operation-map-final.csv) preserve the original04A29 member/operation IDs and04A30 links. The three files contain191 historical declaration/member rows,of which185 were proposed for33. The six other historical rows are constructor/Draw/object-list/settings boundaries;the owner's explicit33 wire-numeric scope governs the narrow Draw extraction while native tails remain host-owned. This is not a claim that185 members were independently executed or migrated. [Roslyn ownership](../../evidence/04A33/source-ownership.json) supplies actual entry/final ranges,header/body token hashes and unchanged members.

`XyzProjectionContext` freezes copied minima/ranges/directions,two rotation rows and shift/zoom scalars from the **current** adapter state at each original calculation point. RotationRows reuses StatFunc.Radians and the original sign/angle/scale order;zoom and shifts retain `(1+z/50)^2`, `x/100-.5`, `y/100+.5`. Existing XyzDataCore.NormalizeAxis remains the sole raw-axis normalization formula. Windows get_rotations,interpolation helpers and ProjectRawPoint delegate. No parallel Windows trigonometric projection remains. Draw and its chart/export/animation lifecycle are token-unchanged.

`XyzGeometryCore` consumes that context for normalized data,Z-drop,cut planes,cage,ticks and grids. Data has n X/Y/drop entries;planes six vectors of n;cage two vectors of11;ticks six vectors of3*(CInt(tx)+1);grids twelve corresponding vectors. Fixed normalized Z=-.5 is preserved for drop bases even with flipped Z. The real XYZ sentinel is **-100000000000000**,not the prompt's historical -100;point/segment order and existing native hiding are retained. TickAxis returns raw step/first/count and normalized first/step;CInt/Math.Round exceptions and per-axis Windows commits preserve characterized partial state. Tick text remains Windows CStr/current culture.

`ScatterMatrixSeriesCore` supplements the accepted unchanged ScatterMatrixDataCore.Normalize;it reuses Core CorrelationMatrixCore.Compute and StatFunc.Intercept/Slope. No normalization/regression/correlation engine is copied. Windows transformData retains original allocation and normalized/correlation assignment order,then consumes PanelCoordinates. Scaled values are n-by-p;correlation anchors p² in i-outer/j-inner order;regression vectors p*(p-1)*3 in endpoint/endpoint/**-100** triples (old zero arrays when disabled). GridPosition and PanelValues feed the real compute renderer. Constructor margin is still forced to.1;defaults,labels,upper-r/lower-p format routing,series order and historical `Variabe Names` spelling remain unchanged. Optional-off CorrMatrix retains legacy state.

`WireObjectGeometryCore` owns sphere/ellipsoid raw bounds and the exact original separate parametric sampling formulas. Full widths become radii/semiaxes;latitude excludes poles,meridians retain endpoints. Cardinality is (Max(0,lat)+Max(1,lon))*(Max(12,pts)+2),including one break/ring. WireSeriesData exposes copied X/Y,structural break masks,ring and sample ordinals. Invalid<=0 dimensions preserve Windows no-draw and reversed raw-bound behavior. Each Draw reads current object properties and uses the same current projection coefficients as data/cage. Series creation,name,color,border,Excel types and point hiding remain native tails.

The [coordinate contract](../../evidence/04A33/coordinate-contract.json) names every public member,shape,axis policy and member-specific retained Windows boundary. Live object-bound callbacks/manual-bound validation/range fallbacks and mutable option/group orchestration remain Windows-owned to preserve existing API/error/evaluation timing;their resolved numeric state is frozen for portable geometry. Chart/Series/Points,styles,label formatting,group packing,COM ownership and UI/GIF/export stay Windows. This is an explicit state/renderer exception,not a claim those retained policies were independently ported or native-tested.

All new portable inputs are snapshotted;every mutable coordinate,vector-name,matrix,mask and ordinal getter returns an independent copy. Data SourceOrdinals are zero-based original point positions;scatter rows/variables/panel rows/columns are distinct;wire ordinals identify ring/sample geometry. Native point positions are one-based within their eventual series. **PhysicalWorksheetRow=NOT_AVAILABLE** for these values-only callers. No worksheet row is invented. Legacy Windows input/label aliases and mutable sequencing are retained intentionally.

Pre-edit authority is the actual compiled clean-main Framework adapter: [9,472-row corpus](../../evidence/04A33/pre-edit-adapter-capture.csv),[source/fixture pins](../../evidence/04A33/pre-edit-authority.json) and frozen test-only numerical slices. No Chart/COM instance was created. Those slices remove the renderer and adapt namespace/interop signatures only;the compiled fixture adjustment is independently checked. Wire Draw cannot be called without a Chart,so its original numeric sampling slice supplies compatibility reference and its native tail is source-guarded. This limitation is explicit.

The [casebook](../../evidence/04A33/compatibility-casebook.json) verifies **{cb['unique_vector_cases']:,} unique vector comparisons / {cb['executed_vector_comparisons']:,} executed across Core/Framework Debug/Release**. Full expected/actual IEEE vectors are published once;other lanes are checked and digest-linked. [Fixture specification](../../evidence/04A33/fixture-specification.json) records inputs/options,method identities,source and ordering. Actual final adapter captures in both configurations equal the pre-edit corpus byte-for-byte,including shapes,NaN bits and exceptions. Finite/infinite/signed-zero values compare bitwise;algorithmic NaNs compare by classification in new source-fixture tests. No new floating tolerance or scientific oracle was used. Additional tests cover input/getter mutation,manual/object bounds,clear/update/repeated options,ragged/missing inputs,feature toggles,group/label state and independent cardinal geometry. Existing04A30 regressions remain unchanged and pass in focused lanes.

Managed outcomes are PASS/FAIL/ERROR/SKIP;real failures remain failures:

| Suite | Fresh main Debug | Final Debug | Final Release |
|---|---:|---:|---:|
{chr(10).join(suite_rows)}
| **Total** | **20080/14/0/0** | **20145/14/0/0** | **20145/14/0/0** |

There are65 new raw passing cases:25 Core plus40 Framework (25 shared Core tests and15 adapter cases). No original identity/multiplicity or outcome was removed/changed. Entry full Release was NOT_RUN;accepted32 historical Release remains distinct;final full Release was executed. Focused Core **117/117**,Framework **79/79** in each configuration. [Managed identities](../../evidence/04A33/managed-test-identities.csv) retain raw identity SHA and occurrence ordinal for every case. All14 inherited failures,each present in both final configurations:

{fail_list}

Node: **916 PASS /0 FAIL**. Python aggregate: **{p['passed']} PASS /{p['failed']} FAIL /{p['errors']} ERROR**,956 reported tests;fresh main was833/113/10. One new failed historical subtest pins 3Dobjects.vb. [Qualification](../../evidence/04A33/source-nonpass-qualification.json) proves the exact old/new hash pair and source assertion lines25–29;the old test/expected pin is unchanged. Other nonpass identities/multiplicities are inherited,with no unclassified delta. Python totals retain unittest subtest accounting;they are not a claim of a green suite. The external plan update added no observed Python nonpass. [Source/Node receipts](../../evidence/04A33/source-and-node-validation.json) contain all primary/nonpass identities and multiplicities.

Strict/profile real exits **1/1**,with retained stale NormalPlot constructor review-decision / INTEGRITY_FAILURE diagnostics;strict MkDocs **PASS (exit0)**. New independent evidence audits: **{audit_text}**. Commands,versions,exits,source/build receipts and sanitized isolation paths are in [command-outcomes.json](../../evidence/04A33/command-outcomes.json) and [validation.json](../../evidence/04A33/validation.json). Tools: .NET SDK10.0.403,MSBuild18.11.1.51303,Framework CLR4.0.30319.42000,Node24.20.0,Python3.14.7. All build targets/configurations passed;no restore/install/network action was needed.

The unchanged verify-beshstatng.ps1 wrappers,including -Legacy,are **NOT_RUN** because they hard-code shared TestResults/Codex/bin/obj and the Legacy Build packaging hooks. The accepted isolated runner executed complete Core/Common/Framework/mitigation/Office managed lanes,Node .cjs suites,Python audits,strict/profile and MkDocs;host compile-only targets avoid packaging/signing. No claimed wrapper completion. Failed intermediate build/harness receipts remain local and are distinguished from the qualified source-unchanged runs.

[Preservation](../../evidence/04A33/source-preservation.json) authenticates4019 tracked entry files:4014 unchanged,four authorized source/project changes,one separately preserved external owner plan update. All35 other Windows graphics files and23 existing Core graphics files,old tests and04A29–32 evidence remain byte-unchanged. The278-pin [successor](../../evidence/04A33/source-transition-successor.json) retains275 predecessor pins and records three authorized Windows transitions;268 hashes still equal original04A28 pins. Accepted32's seven earlier source/project transitions remain frozen. All100 mixed-model captures match exact accepted hashes. Historical TestResults metadata matches280953 files/138180795001 bytes and the accepted membership/size/mtime digest;**payload bytes were NOT_REHASHED**. No tree metadata was intentionally changed.

All57 predecessor32 manifest payloads are authenticated by exact direct or inverse EOL/CSV digest reconstruction and equality to actual merged Git blobs. This qualifies post-merge line endings without editing the old manifest,rerunning the original29 archive qualification or asking for a ZIP. No new original-archive-byte inspection is claimed. Original scientific/native controls and deferred ledgers remain frozen.

The [publication manifest](../../evidence/04A33/publication-manifest.json) is the exact intended publication list:the source/tests/project above,this dossier,and scoped evidence/04A33 files. Every external payload has relative path,byte length and SHA256;the sole self-entry has null hash/size. The independent verifier imports no producer and rejects traversal,duplicates/case aliases,missing files,hash/size tampering and recursive self-entries. Additional audits reject source-transition tampering,source identity reassignment,stale gate states and false EOL qualification. Privacy checks exclude private home/machine/SID/workbook/credential data;raw logs and isolated build products are not publication artifacts. The external owner plan file is excluded.

[Deferred issues](../../evidence/04A33/deferred-issues.json) retain G29-SCATTER-MARGIN-IGNORED (existing04A42 product/04A47 native disposition),sentinel collisions,extreme tick digits,group latch,invalid wire bounds/no-draw inconsistency and degenerate scatter policies. Source/reproducers/evidence and candidate dispositions are explicit. Nothing is corrected or discharged. Independent scientific/state investigations stay04A39–41,applicable existing product/native decisions04A42/45–48. Specialty charts remain04A34–37;no04A34 work began.

**P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED. Zero native releases;zero original-obligation discharges.** Native Excel/COM/UIA/RefEdit/chart smoke,R/scientific oracle,manual Office/Mac/browser/WASM UI,installer/packaging/signing/deploy are NOT_RUN. [Synthetic smoke proposal](../../evidence/04A33/native-smoke-proposal.md) supplies exact fixtures,series/axes/labels/breaks and later source/build/HWND/PID/loaded-XLL/session attribution requirements;it is not an executed smoke or Office/Mac feature activation.

Owner checklist:

- Review portable coordinate ownership,fixture authority,retained host exceptions and unchanged native/source boundaries.
- Review raw test identities,real inherited/source-pin nonpasses,deferred candidates and gates.
- Run the independent manifest verifier and inspect exact publication paths/privacy/preservation receipts.
- Keep the unrelated revision2.12 owner plan update separate as appropriate;owner alone stages/commits/merges/pushes.

Nothing is staged,committed,merged,pushed or deployed by Codex. The index is empty;HEAD/main/merge-base remain the verified entry SHA. No04A34 authorization is requested.
'''
 (ROOT/DOSSIER).write_text(body,encoding='utf-8',newline='\n')
def privacy():
 from verify_manifest import expected_scope
 issues=[]
 for p in sorted(expected_scope()):
  if p.endswith('publication-manifest.json'):continue
  s=(ROOT/p).read_text(encoding='utf-8-sig')
  for pattern in [r'(?i)[A-Z]:[\\/]+Users[\\/]+[^\\/\s"\']+',r'S-1-5-\d+(?:-\d+)+',r'(?i)\b(?:DESKTOP|LAPTOP)-[A-Z0-9]+',r'https?://[^\s/"<>:@]+:[^\s/"<>@]+@']:
   if re.search(pattern,s):issues.append(dict(path=p,pattern=pattern))
 assert not issues,issues
 save('privacy.json',dict(schema='04A33-privacy-v1',status='PASS',scope='All intended externally published UTF8 source/tests/evidence/dossier payloads',private_paths_sids_machine_names_credentials='No matching private home/SID/machine/credential URI payloads',raw_logs='NOT_PUBLISHED',workbook_data='Synthetic source-backed fixtures only;no private workbook acquired',notes='Private test displays redacted with raw identity hashes retained;tracked private-path aliases are hash-addressed',issues=[]))
if __name__=='__main__':checks();report();privacy()
