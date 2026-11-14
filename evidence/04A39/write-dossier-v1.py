"""Single consolidated owner dossier; gates are independently recomputed, never inferred from prose."""
from common39 import *
import importlib.util

source_lock()
spec=importlib.util.spec_from_file_location('final_reader39',HERE/'verify-readiness-v1.py')
reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
facts=reader.verify(ROOT)
reg=load(HERE/'register/defects-v5.json');python=load(HERE/'python-node-scanners/python-identities-v1.json')
unknown=load(HERE/'register/unresolved-linkages-v2.json')['entries']
status='READY_FOR_OWNER_04A39_ACCEPTANCE_REVIEW' if not unknown else 'NOT_READY_04A39'
exceptions=[]
for config in ['Debug','Release']:
    for row in load(HERE/('build-and-tests/'+config+'-identities-v1.json'))['nonpasses']:
        exceptions.append(dict(id='MANAGED:'+config+':'+row['identity_sha256']+':'+str(row['occurrence']),
            identity=row['class_name']+'.'+row['method'],status='INHERITED_RESTRICTED;NOT_WAIVED',message_sha256=row['message_sha256'],
            trace_sha256=row['traceback_sha256'],delta=row['classification'],next_owner='Existing source-family maintainer; exact owner scientific/native disposition'))
for row in python['events']:
    exceptions.append(dict(id='PYTHON:'+row['identity']+':'+str(row['occurrence'])+':'+row['outcome'],identity=row['identity'],
        subtest=row.get('subtest'),status='INHERITED_RESTRICTED;NOT_WAIVED',delta=row['classification'],message_sha256=row['message_sha256'],trace_sha256=row['traceback_sha256']))
exceptions += [dict(id='STRICT:NORMALPLOT-AMBIGUITY',status='INHERITED_RESTRICTED;EXPLICIT_OWNER_DECISION_REQUIRED',exit_code=1),
    dict(id='PROFILE:SOURCE-CONTRACT-NONPASS',status='INHERITED_RESTRICTED;EXPLICIT_OWNER_DECISION_REQUIRED',exit_code=1)]
save('final/unresolved-exceptions-v1.json',dict(entries=exceptions,register_unknowns=unknown,
    inherited_Release_not_rerun='W3 full Release14 FAIL and nine trace differences remain retained/unwaived.39 Release303 retained selected identities plus33 new cases all pass; no full-suite result is claimed.',
    historical_python_drift='Accepted W2 one-message/three-trace replacement drift retained; current39 delta separately enumerated.',
    canonical_issue_register=pin(HERE/'register/current-v3.json'),no_waivers=True))
save('final/readiness.json',dict(status=status,utc=utc(),main=MAIN,branch='codex/04A-39',tip=MAIN,
    facts=facts,canonical_register=pin(HERE/'register/current-v3.json'),register_counts=reg['counts'],
    current_issue_linkage_uncertainties=unknown,exceptions=pin(HERE/'final/unresolved-exceptions-v1.json'),
    production_source_changes=0,external_signatures_changed=0,activated_defect_fixes=0,
    approval_basis='Owner current prompt accepted38W2(13Nov),W3qualified/overall(14Nov); actual merge verified; subsequent owner accepts newline-only reconciliation.',
    gate_status=GATES,owner_acceptance='NOT_GRANTED_BY_AGENT',next_batch='04A40_NOT_AUTHORIZED',
    manual='Excel/COM/UIA/RefEdit/R/scientific/OfficeMac/browser/install/sign/deploy NOT_RUN_UNAUTHORIZED',
    limitations=['Strict/profile and managed/Python nonpasses remain real failures/errors, not green.',
        '29,424/config compatibility retained with exact source/CIL/fixture proof, not fresh39 current comparator execution.',
        'Release full managed suite not rerun;336 targeted cases fresh.',
        'Private37484-file/154-archive W3 baseline inherited with qualified physical scope; incremental39 proof only.',
        'No independent scientific impact calculation or historical original precision oracle.',
        'Canonical declaration candidates establish traceability, not an adjudication of which candidate branch is scientifically/native defective.']))
text=f'''# 04A.39 owner review

{status}

Merged main `{MAIN}` is independently authenticated as the direct child of W3 baseline `ba1636f5356ac761509c2cfa16fc66b92430935d`. Branch/tip: `codex/04A-39` / `{MAIN}`. Owner W2 acceptance13November2026 and qualifiedW3/overall38 acceptance14November2026 are current owner assertions. The subsequent explicit line-ending approval covers237 committed and39 live newline-only differences; original hashes remain preserved. All1350 paths exist in the commit, including18 package/installer-support files; no generated/ignored-path omission was invented.

No production code changed. The actual101-file Roslyn census maps1861 members and192 focused import/argument members, including38 lambdas. Existing Core/Common policies are reused; Excel sentinel/reference/acquisition, callbacks and cache lifetime remain Windows-owned. Source statements/branches, ByRef assignments, source spans and caller candidates are retained; exact compiled dependencies/signatures/instructions are independently compared. No duplicate parser, estimator, public API or scientific/native behavior fix was introduced.

| Campaign | Actual outcome |
|---|---|
| Full managed Debug |21127 identities:21113PASS /14 inheritedFAIL;21094 inherited retained,33 addedPASS;14 messages/traces identicalW3|
| Targeted managed Release |336PASS:303 inherited selected+33 added;full Release NOT_RUN_NOT_REQUIRED|
| Shipping Debug/Release |268 exact exports each;all normalized per-method signatures/CIL/exception/delegate/callback/dependency metadata unchanged versus same-configurationW3|
| Frozen compatibility |29424 retained exact/config:28966PRE_EDIT_FROZEN+458LATE_ARCHIVED_ENTRY_SUPPLEMENT;12 corpora;no original or fresh39 current comparator execution|
| Current-only observation fixtures |77 exact/config;typed inputs,Double bits,cultures,source/selected coordinates,current exceptions preserved|
| Python parents |{python['summary']['parent_outcomes']} over956 unique parents|
| Python nonpass events |{python['summary']['event_outcomes']};deltas{python['summary']['event_categories']}|
| Node |916PASS,exact identity multiset|
| Strict/profile/MkDocs |1/1/0;nonzero exits remain inherited unwaived nonpasses|

The canonical register is `register/current-v3.json` â†’ `defects-v5.json`: {reg['counts']['canonical_entries']} entries,1514 immutable original obligations,802 restricted controls,{reg['counts']['report_ids']} historical report IDs and{reg['counts']['report_occurrences']} occurrences, plus three39 observations. Populations overlap; no additive obligation arithmetic or discharge is asserted. Repeated exact IDs map once; C17/C18 no-label CA reports share an explicit source-backed canonical alias while retaining distinct history. Current declaration candidates/signatures/preimages, original source/control facets, risk/owner/phase/reproducer/re-review fields and unresolved linkage reasons remain visible. No04A.48 final disposition was assigned.

Nested precision is reproduced on Framework: typed1.2345678901234567 bits`3ff3c0ca428c59fb` reach Core as`3ff3c0ca428c5a0a` after numeric-text column stringification. Near2^53 and signed-zero observations,Invariant/sk-SK text interpretation, skipped rows and extreme-finite overflow are retained. These are conversion facts, not Excel display claims or independently measured scientific impact. No precision-changing behavior was activated; original historical precision oracle is REFERENCE_UNAVAILABLE/CANDIDATE_ONLY.

Paired tests explicitly separate physical sheet/row identity, selected positions, complete-case positions and explicit subject keys. Arrays receive no fabricated worksheet coordinates. Source intersections and ordinal pairing remain different contracts; duplicate/mismatched/unknown identities reject where accepted policy requires it. Physical-import sidecars and clone limitations retain their accepted tests. New missing/error/shape/lower-bound/ByRef/callback/error-precedence/locale tests assert current behavior, including alpha-default and numeric truncation candidates.

All inherited14 managed failures,113 PythonFAIL parents,13ERROR parents,185FAIL/13ERROR events, both historical privacyUnicodeDecodeError events and unordered-SPC error remain unwaived. W3's nine Release trace variations and acceptedW2 one-message/three-trace drift remain disclosed. No repository-wide clean privacy or scientific/native success is claimed. See the exact exception/event ledgers.

Stopped attempts are retained: initial branch sandbox denial; read-only inspection path/type mistakes; v1 diagnostic helper reflection assembly mistake; initial new-test SourceAt compilation error; incorrect new alpha fallback expectation corrected to actual exception; build report name collision recovered without rebuilding; abstract-method missing rawIL key; three generated Debug all-static field reflection-order records; register subset/global802 and Counter reader errors; Python prelaunch replacement-count stop; legacy-source lexical decoder stop. These are distinct from final product behavior and no full managed/Python campaign was repeated to erase them. All33 final added tests pass.

The living Master Plan is Revision2.15, records the actual accepted38 merge and39IN_PROGRESS, and preserves exactRevision2.14 separately. Original226 batch IDs/roadmap and39â€“48 charter remain. No methods.yml,Ribbon,Help availability or Office/Mac execution claim changed. Original verify-beshstatng wrappers/-Legacy were NOT_RUN because they restore implicitly and write shared historicalTestResults; authenticated isolated offline equivalents were executed instead. The small exact protected mitigation projection was hash-checked/copied; the massive historicalTestResults tree was never recursively enumerated or cleaned.

Preservation is incremental: W3's37484 private pins/154 archive declarations are inherited with their original qualified physical scope, not newly claimed fully audited/uploaded. All18 live-only support sources and current production locks are read back; authorized newline/test-project transitions and task-local additive pins are explicit. Review ZIP/readback receipts follow underpublication; complete private rawTRX/worker traces,live index,owner documents and every historical trace/archive are not all transported. Independent ZIP review proves derivative bytes/census and source-bound evidence; it does not rerun owner-machine builds or private/native/scientific gates.

Suggested later manual checks, only after exact authorization: recalculate one scalar/mixed-cell UDF and one offset paired-range UDF in disposable Excel; inspect existing WASM smoke paired/provenance output. Neither was run here.

Owner action: independent ChatGPT review of this candidate, exact unresolved issues/nonpasses/limitations and transport receipt; then explicit39 acceptance/merge decision. No commit,stage,merge,push,installation,release or04A.40 start was performed.

P=NO;N1=NO GO;N2=NO;C2=UNMET;C3=UNAPPROVED;NOT_GOLDEN;04A39_OWNER_REVIEW_PENDING;04A40_NOT_AUTHORIZED
'''
with (HERE/'final/owner-review.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
with (HERE/'final/next-gate-handover.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write('Independent ChatGPT review, then explicit owner04A.39 acceptance and owner-authorized merge. No automatic04A.40 start. Review canonical register/current-v3.json, final/readiness.json, exact nonpass ledgers, final Git and physical ZIP receipts. Native/scientific/R/Excel/browser/OfficeMac/install/sign/deploy remain unauthorized.\n')
with (HERE/'publication/README-v1.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write('One complementary review ZIP with .review-manifest.json. Run verify-package-v1.py ZIP for exact paths/SHA/size/CRC; extract only after validation. verify-register-v4.py EXTRACTED_ROOT verifies canonical IDs/facets against included immutable authorities. Owner-machine verify-readiness-v1.py requires private raw results; derivative logical review may call verify(root,live=False), explicitly excluding private byte/Git/re-execution claims. Read privacy exclusions before interpreting unavailable raw evidence. Later .001/.002 binary volumes, if owner chooses them, must be concatenated in numerical order before extracting the outer0-store ZIP; they are not independent source packages. No binary splitting or external upload performed here.\n')
print(status,flush=True)
