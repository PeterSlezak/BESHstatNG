"""Versioned final41 reporting and fail-closed actual-candidate/physical privacy seal."""
from common41 import *
import collections,re,stat,unicodedata,zipfile,zlib
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'))
import physical
STATUS='READY_FOR_OWNER_04A41_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
class ProbeMember:
    def __init__(self,text):self.text=text
    def is_file(self):return True
    def read_text(self,encoding):assert encoding=='utf-8-sig';return self.text
class ProbeRoot:
    parents=ROOT.parents;name=ROOT.name
    def __init__(self,text):self.text=text
    def __truediv__(self,path):return ProbeMember(self.text)
    def as_posix(self):return ROOT.as_posix()
    def __fspath__(self):return str(ROOT)
def identity_controls(phase):
    controls=[]
    for i,token in enumerate(sorted(physical.tokens())):
        variants=[token,token.swapcase(),''.join(chr(ord(c)+0xfee0) if 33<=ord(c)<=126 else c for c in token)]
        for text,mode in zip(variants,['literal','case','NFKC']):
            m=physical.scanner(phase);m.ROOT=ProbeRoot(json.dumps(dict(value=text)))
            a,ar=m.scan(['identity-control.json']);m.re=re;b,br=m.scan(['identity-control.json'])
            assert a==b and ar==br and any(r['category']=='known_private_identifier' for r in a)
            controls.append(dict(identity_ordinal=i,mode=mode,detected=True,direct_original_regex_and_guard_exact=True))
    return controls
def candidate_scan(label):
    lock();reports=[]
    for phase in ['13','14']:
        m=physical.scanner(phase);assert m.ROOT==ROOT and physical.tokens()
        paths=m.paths();inputs=[pin(p) for p in paths];found,reviewed=m.scan(paths)
        assert inputs==[pin(p) for p in paths]
        raw=PRIVATE/(label+'-'+phase+'.json');save(raw,dict(candidate_inputs=inputs,findings=found,reviewed=reviewed,scanner=pin(m.__file__)))
        assert not found,'UNRESOLVED_ACTUAL_CANDIDATE_PRIVACY_FINDING'
        reports.append(dict(scanner=phase,source=pin(m.__file__),scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),guard=pin(ROOT/'evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py'),members=inputs,candidates=len(paths),findings=0,unresolved=0,identity_controls=identity_controls(phase),raw=pin(raw),scope='Actual unchanged paths() over every changed/new Git candidate, including original phase-specific metadata. No filtered/staged/ZIP surrogate.',normalization='Original NFKC, paths/content/decodedJSON/base64 rules;Git relative-path spelling retained.'))
        print('Original repository scanner',phase,len(paths),'candidates;0 findings',flush=True)
    return reports
def check_sources():
    lock();freeze=load(HERE/'matrix/source-freeze-v1.json')
    for p,h in freeze['source_pins'].items():assert pin(p)['sha256']==h
    exact(freeze['plan']);exact(freeze['plan_snapshot'])
    allowed={r['after']['path'] for r in freeze['source_delta']}|{freeze['plan']['path']}
    assert set(git('diff','--name-only').decode().splitlines())==allowed
    assert not git('diff','--check')
    assert not any((ROOT/p).exists() for p in ['evidence/04A40/baseline/Debug-build-v1.json','evidence/04A40/entry/tool-runtime-final-v1.json'])
    return freeze
def prepare():
    freeze=check_sources();auth=load(HERE/'entry/authentication-v1.json')
    save('entry/review-authentication-v2.json',dict(kind='SANITIZED_REVIEW_AUTHENTICATION;NOT_RAW_GIT_TRANSCRIPT',HEAD=MAIN,main=MAIN,merge_base=MAIN,branch='codex/04A-41',entry_branch='main',prior=PRIOR,index=auth['index'],preexisting_owner_edits=[],accepted40_receipt=auth['accepted40_receipt'],accepted40_archive=auth['accepted40_archive'],raw40_boundary_verified=True,plan_ids=226,current_plan='2.18',original_plan='2.17',owner_checkout_newline_only_transitions=31,original_sealed40_copies_preserved=True,complete_entry_receipt=pin(HERE/'entry/authentication-v1.json'),restriction='No continuous physical file-identity claim;accepted40 restrictions persist;41 not owner-accepted or merged.',gates=GATES))
    for config in ['Debug','Release']:
        proof=load(HERE/('abi-cil/'+config+'-proof-v1.json'));exact(proof['current_exports'])
        exports=load(ROOT/proof['current_exports']['path'])['exports'];assert len(exports)==268
        save('abi-cil/'+config+'-exports-review-v1.json',dict(kind='PUBLIC_EXPORT_REVIEW_DATA;NOT_RAW_METADATA_TRANSCRIPT',configuration=config,exports=exports,raw=proof['current_exports'],exact_accepted40=True))
    patch=git('diff','--no-ext-diff','--no-color','--',*[r['after']['path'] for r in freeze['source_delta']],freeze['plan']['path'])
    with (HERE/'source-review-v1.patch').open('xb') as f:f.write(patch)
    attempts=[]
    for p in sorted((PRIVATE/'failed-attempts').glob('*.json')):
        d=load(p);attempts.append(dict(recipe=d['recipe'],type=d['type'],raw=pin(p),disposition='RETAINED_FAILED_ATTEMPT;NOT_REPLACED_BY_SUCCESSOR'))
    raw_files=[pin(p) for p in sorted(PRIVATE.rglob('*')) if p.is_file()]
    save(PRIVATE/'new-raw-inventory-v1.json',dict(files=raw_files,scope='Existing41-owned files before candidate/physical seal. New seal receipts are separately pinned externally;no recursive historical TestResults walk.'))
    save('preservation/new-raw-review-v1.json',dict(kind='SANITIZED_RAW_PRESERVATION_INDEX;NOT_RAW_DIAGNOSTICS',inventory=pin(PRIVATE/'new-raw-inventory-v1.json'),files=len(raw_files),bytes=sum(r['bytes'] for r in raw_files),failed_attempts=attempts,initial_unwrapped_launch='A ModuleNotFoundError before the protected recipe wrapper was introduced is retained in tool history;no fabricated raw receipt.',raw_vs_review='Build/test/trace/command/cache/binary outputs stay in ignored controlled41 storage. Public documents contain explicit versioned facts, raw SHA/provenance and no raw owner identifiers.',new41_raw_path_removals=0,old40_two_raw_paths_absent=True))
    commands=[]
    for p in sorted((PRIVATE/'commands').glob('*/command.json')):
        d=load(p);commands.append(dict(label=p.parent.name,exit_code=d['exit_code'],seconds=d['seconds'],raw=pin(p),stdout_sha256=d['stdout_sha256'],stderr_sha256=d['stderr_sha256']))
    save('matrix/actual-commands-review-v1.json',dict(kind='SANITIZED_COMMAND_FACTS;NOT_LITERAL_RAW_ARGV_TRANSCRIPT',commands=commands,executable_aliases={'python_initial':'Existing managed3.13.18 for early preparation/build fixtures','python_final':'Recovered accepted3.14.7;SHA6bf492976e3a0cbb10695171d7f11897c4935ba93bc078b3bc3be52861d78ecc','root':'<REPO_ROOT>','private':'<CONTROLLED41_RAW>'},reproducible_recipes=['run41.py authenticate-v3.py','run41.py scope-v1.py','run41.py capture-fixtures-v1.py','run41.py wave1-v1.py','run41.py wave1-check-v2.py','run41.py wave2-v1.py','run41.py wave2-check-v1.py','run41.py update-plan-v1.py','run41.py final-managed-v1.py Debug','run41.py final-managed-v1.py Release','run41.py compiled-and-fixtures-v1.py Debug','run41.py compiled-and-fixtures-v1.py Release','run41.py compatibility-v1.py Debug','run41.py compatibility-v1.py Release','run41.py python-campaign-v2.py prepare','run41.py python-campaign-v2.py execute','run41.py reconcile-python-v1.py','run41.py python-dispositions-v1.py','run41.py preservation-v1.py','run41.py serial-checks-v1.py'],recipe_directory='evidence/04A41/',recipe_policy='Append-only writer recipes are execution provenance, not safe rerun instructions over existing evidence. Use standalone review_reader.py for read-only delivery verification.',full_Debug_campaigns=1,full_Python_campaigns=1,full_Release='NOT_RUN',Node='INHERITED_916_PASS;NOT_RERUN',direct_verify_script='NOT_RUN_PROTECTED_FIXED_TRX_PATH_COLLISION;UNCHANGED_ISOLATED_RUNNER_USED',native_R_Excel_WASM_manual='NOT_RUN',installed_packages=0,stage_commit_merge_push_release=False))
    print('Versioned review authentication/export data/source patch/raw preservation/command facts prepared',flush=True)
def initial_scan():
    reports=candidate_scan('repository-initial')
    save('privacy/repository-candidate-v1.json',dict(status='ZERO_FINDINGS_FOR_EXACT_RECORDED_CURRENT_INPUTS',reports=reports,rules_ignore_and_candidate_scope_unchanged=True,waivers=0,owner_tokens_and_unsalted_identifier_hashes_published=False,source_preservation='All originals/history unchanged;old40 raw exclusion is the separately authorized correction inherited from accepted main.',history=dict(accepted39_privacy='TWO_FAIL_PARENTS_1941_EACH;1599+342',W3='TWO_PRIVACY_ERROR_PARENTS_UNWAIVED',original54='RETAINED_UNWAIVED',original32='RETAINED_UNWAIVED',old40_raw='6_PYTHON_SNAPSHOT_AND8_LATER_FOCUSED_OCCURRENCES_FAIL_RETAINED'),final_metadata='Later final full actual-candidate scans include this receipt, owner review/readiness and every final metadata file. Complete immutable input set is sealed in the external receipt;no self-hash fiction.'))
def documents():
    freeze=check_sources();debug=load(HERE/'build-and-tests/Debug-identities-v1.json');release=load(HERE/'build-and-tests/Release-identities-v1.json');py=load(HERE/'python/parent-event-reconciliation-v2.json')
    assert debug['summary']['outcomes']==dict(Passed=21143,Failed=14) and all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
    assert release['summary']['outcomes']==dict(Passed=2271,Failed=1)
    assert py['summary']['parent_outcomes']==dict(PASS=832,FAIL=113,ERROR=11) and not py['summary']['unresolved_diagnostic_events']
    assert load(HERE/'python/diagnostic-dispositions-v1.json')['unresolved']==0
    assert all(r['findings']==0 for r in load(HERE/'privacy/repository-candidate-v1.json')['reports'])
    for config in ['Debug','Release']:
        assert load(HERE/('compatibility/'+config+'-v1.json'))['cases']==29424
        assert load(HERE/('abi-cil/'+config+'-proof-v1.json'))['exports']==268
    save('final/git-final-v1.json',dict(utc=utc(),branch='codex/04A-41',HEAD=MAIN,main=MAIN,merge_base=MAIN,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),untracked_at_receipt=git('ls-files','--others','--exclude-standard').decode().splitlines(),final_actual_membership='External final receipt binds all later candidate metadata and exact member hashes.',old40_raw_paths_absent=True,staging_commit_merge_rebase_push_release=False))
    state=dict(status=STATUS,kind='OWNER_ACCEPTANCE_REVIEW_READINESS_ONLY',effective_only_with_matching_external_receipt=True,base=MAIN,branch='codex/04A-41',owner_accepted=False,merged=False,phase04A42_authorized=False,scientific_corrections=0,native_activations=0,implementation=dict(production_files=10,Windows_files=5,Common_files=5,new_Common_public_methods=22,new_Common_classes=0,changed_exported_parent_bodies=28,scoped_exports=76,total_exports=268,new_test_files=2,new_passed_test_identities=18),software=dict(Debug=debug['summary'],Release=release['summary'],full_Release='NOT_RUN',Python=py['summary']['parent_outcomes'],Python_events=py['summary']['event_outcomes'],Node='INHERITED_916_PASS;NOT_RERUN',strict_profile_MkDocs=load(HERE/'matrix/serial-checks-v1.json')['exits']),compatibility=dict(sealed_per_configuration=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,new_current_main_cases=390,accepted39_cases=77,accepted40_contracts=48,GEE_elapsed_only_fields=4,original_queries=0,scientific='NOT_GOLDEN'),privacy='Both unchanged full actual-candidate scanners zero;matching external receipt must also seal final metadata and all physical ZIP members/index with zero findings.',historical_failures='14_Debug_FAIL;1_affected_Release_FAIL;113_Python_FAIL_and11_ERROR;3_historical_UDF_guards_FAIL;39privacy1941each_andW3ERROR_and54+32_records_unwaived',investigations=dict(count=17,register_entries=1169,obligations=1514,restricted_controls=802,mappings=2967,scientific_native_exits_discharged=0),preservation=dict(checkpoint=468,historical=8984,newer_raw=1906,source_history=1353,continuous_physical_identity='NOT_VERIFIED',old_ID_drifts=788,W3_37484_154='INHERITED_QUALIFIED_NOT_FRESH_AUDIT',sealed40_newline_recoveries=31),coverage_limits='Five ordinary current-main probes retain errors in unchanged parents.390 is representative, not an independent scientific oracle or exhaustive native/domain coverage. Full preserved corpus/affected tests are separately reported.',gates=GATES)
    save('final/readiness-postseal-v1.json',state)
    save('final/decision-matrix-v1.json',dict(state,readiness=pin(HERE/'final/readiness-postseal-v1.json'),gate_evidence=[pin(HERE/p) for p in ['build-and-tests/Debug-identities-v1.json','build-and-tests/Release-identities-v1.json','build-and-tests/Release-trace-disposition-v1.json','python/parent-event-reconciliation-v2.json','python/diagnostic-dispositions-v1.json','python/source-guard-dispositions-v1.json','privacy/repository-candidate-v1.json','register/issue-delta-v2.json','preservation/readback-v1.json']]))
    text='''# Phase04A.41 owner review

READY_FOR_OWNER_04A41_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS is authoritative only with the matching PASS external-delivery-receipt-v1.json. The owner has not accepted or merged04A.41.04A.42 is not authorized.

Verified base/main/merge-base: `c6c22dbc59d6dfc61ed64f6413d8c81e7102e1dc`, direct child of the previously reviewed main. Branch: `codex/04A-41`. Entry was clean. Revision2.18 records the owner's explicit40 acceptance and41 authority; exact2.17 and all226 batch IDs are preserved.31 pre-existing clean-checkout newline differences in40 evidence are documented; sealed original byte copies match their original SHA/size. No protected historical file was rewritten by41. The two excluded unsafe40 raw report paths remain absent; exact protected copies and safe review-v2 successors remain pinned.

Ten existing production files changed: five Common services and five Windows UDF files.22 public methods extend existing services, with no new41 helper class and no changes to pre-existing Common method bodies.28 exported parent bodies delegate typed execution/result work; all76 scoped functions and all268 exports retain signatures/names/order/attributes. Core owns the same numerical algorithms. Windows retains Excel-DNA arguments/errors, acquisition, conversion, labels and exception precedence; deferred option/seed readers preserve compute/constructor-before-read ordering. GUI/native code, existing tests, framework targets and32/64-bit packaging are unchanged. Contingency already used the correct shared services and received no churn. No Office/WASM UI or capability is added.

| Actual validation | Outcome |
|---|---|
| Full Debug, one frozen campaign |21143 PASS /14 inherited FAIL;all21139 accepted40 identities retained;18 new PASS;all14 failure messages/traces exact |
| Complete Release compilation / affected tests |Nine-step graph exit0;2271 PASS /1 inherited FAIL;full Release NOT_RUN |
| Release failure detail |Same retained message/test/source frames;optimized sequence point line89 versus accepted/current Debug line88. Accepted40 targeted Release did not cover this test; no resolution claim |
| Frozen compatibility |29424 exact/configuration:28966 PRE_EDIT_FROZEN plus458 approved LATE_ARCHIVED_ENTRY_SUPPLEMENT;no original query/rebaseline |
| Other fixtures |390 pre-edit41 current-main cases and77 accepted39 cases exact/configuration;48 accepted40 statistical/input/exception contracts exact, with four GEE.CompTime fields separately disclosed |
| Python, one956-parent campaign |832 PASS /113 FAIL /11 ERROR;185 FAIL +11 ERROR events;175 identical retained events,21 changed messages source/context-argument proved;two current privacy parents now PASS |
| Node |Inherited916 PASS, not rerun;JS and host-facing DTO/compiled contract dependencies unchanged;fresh managed Office tests passed |
| Strict/profile/MkDocs |1/1/0;the nonzero checks remain nonpasses |
| Privacy/publication |Both original repository scanners and every physical ZIP member/index must have zero findings in the matching external receipt;standalone reader plus12 adverse controls |

The390 new cases were materialized before production edits from the verified current-main Debug assembly. They preserve typed values/IEEE bits, array bounds, exception types/messages, culture and seed173. They are compatibility, not original38 or scientific-golden references. Five ordinary probes return retained errors in unchanged parents (TTEST_UNPAIRED_EQUIV, ROSNER, CT_MANTEL_HAENSZEL, CT_TREND, BLANDALTMAN_ALLOWABLE_BIAS); this is disclosed coverage, not invented successful or exhaustive coverage. Every changed parent has materialized success and adverse cases. The76-function source map supplies actual GUI member hashes/lines and distinguishes unlike interfaces;no native execution is implied.

Seventeen interim investigations bind canonical IDs/original dossiers/current source and materialized cases. Prose-only Shapiro, DAgostino, empty homogeneity, nested-label and Liddell candidates use existing canonical UDF38-D41-ALIGNMENT without invented IDs. C14-INPUT-FINITE-01 and all four C26 issues remain restricted. Shapiro GUI eligibility is n>3 and n<5000 versus UDF3..5000; the original composed-report DAgostino overflow can abort before any eligibility NA cells are written. Constants, moment arithmetic, bounds, model estimates, tails/CI, defaults, spinner/guard order and resampling minima are not corrected here. Current accepted05 CoreLiddellTable dispatch/numerical corrections are retained under their original exact authority; the separate2x2 Risk ratio label remains a matched-pair interpretation candidate. The versioned issue-delta-v2 corrects an overbroad reference to the earlier planned05 summary without changing runtime or old evidence.

Independent definitions were reviewed in R/SciPy/NIST documentation and Liddell1983; accepted05 integer/Decimal candidate reference/CSV are pinned, not regenerated. These references do not authorize an inferential change. Same-engine comparisons remain NOT_GOLDEN;no R/scientific/native campaign or original-control discharge occurred. Exact issue-specific decisions, frozen intended outputs, applicable independent reference and native gate are required before any future correction. No04A.48 final exit status is assigned.

All14 Debug nonpasses, the current affected Release failure, all113 Python FAIL/11 ERROR parents, three historical no-UDF-diff guard FAILs, original39 privacy1941 findings each(1599+342), W3 privacy ERRORs and54+32 diagnostic histories remain unwaived. The two fresh privacy-parent PASSes describe this new candidate only;old raw failures are not recolored. The21 changed messages are verified branch/main/plan/source-path argument transitions, not repaired assertions. Original tests and original13/14 privacy scanners/ignore rules are unchanged.

Fresh preservation reads cover14406 tracked-entry paths except the11 explicitly authorized source/plan transitions,468 checkpoint originals/copies(464 live exact plus four prior authorized newline renderings),8984 listed historical paths,1906 newer40 raw pins and1353 source history. All new41 raw diagnostics/binaries/TRX stay in controlled ignored storage, with versioned safe review facts. Continuous physical file identity remains NOT_VERIFIED;788 ID drifts remain recorded. W3's37484/154 union is inherited qualified, not a fresh union audit. Canonical register1169/1514/802/2967 and40's20 investigations remain unchanged.

Exact executed command facts/protected raw SHA provenance are in matrix/actual-commands-review-v1.json. The fixed-path verify-beshstatng.ps1 / -Legacy entrypoint was not run: it targets existing protected TestResults/Codex/Core.trx,Common.trx andLegacy.trx. Its relevant compilation/full Debug/affected Release work was performed with the unchanged accepted isolated runner, retaining every original test identity and failure. Early3.13 missing-PyYAML discovery and all failed attempts remain;the full Python campaign used the exact recovered accepted3.14.7 interpreter and existing packages,no installation. Do not rerun append-only recipe writers over existing records;review_reader.py is the standalone read-only archive command.

Manual smoke checks NOT_RUN: existing ANOVA/paired-t worksheet spill comparison and agreement/sample-size worksheet report comparison. Live Excel/COM/UIA, R, Office/Mac/browser/WASM UI, signing, installer, release, staging, commit, merge, rebase and push were not performed.

P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A42_NOT_AUTHORIZED
'''
    with (HERE/'final/owner-review-v1.md').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
    destination=ROOT/'docs/architecture/batch-04A-41-owner-review.md'
    with destination.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
    save('final/physical-gate-policy-v1.json',dict(kind='EXTERNAL_PHYSICAL_SEAL_REQUIRED;NOT_SELF_ATTESTED_ZIP_PASS',external_receipt='.test-rationalization-04A41.tmp/publication/external-delivery-receipt-v1.json',index='.review-index.json',scope='Every physical member/index and final metadata under both unchanged original scanners;no allowances.',index_self_hash='Excluded from its own internal map;included in external SHA/CRC/size member inventory and physical privacy scope.',owner_accepted=False,gates=GATES))
    print('Final restricted owner-review/readiness documents prepared;external actual seal required',flush=True)
def package_files():
    relative=['entry/review-authentication-v2.json','entry/baseline-Debug-build-review-v1.json','matrix/source-freeze-v1.json','matrix/serial-checks-v1.json','matrix/actual-commands-review-v1.json','planning/reconciliation-v1.json','scope/UDF-inventory-v1.json','scope/ownership-parity-v1.json','scope/gui-current-member-crosswalk-v1.json','scope/portable-member-lock-v1.json','register/issue-delta-v2.json','register/independent-reference-index-v1.json','build-and-tests/Debug-identities-v1.json','build-and-tests/Release-identities-v1.json','build-and-tests/Release-trace-disposition-v1.json','python/parent-event-reconciliation-v2.json','python/diagnostic-dispositions-v1.json','python/source-guard-dispositions-v1.json','privacy/repository-candidate-v1.json','preservation/readback-v1.json','preservation/new-raw-review-v1.json','fixtures/current-main-390-review-v1.json','fixtures/Observe41.cs','final/git-final-v1.json','final/readiness-postseal-v1.json','final/decision-matrix-v1.json','final/owner-review-v1.md','final/physical-gate-policy-v1.json','review_reader.py','source-review-v1.patch']
    for config in ['Debug','Release']:relative += ['abi-cil/'+config+'-proof-v1.json','abi-cil/'+config+'-exports-review-v1.json','compatibility/'+config+'-v1.json','fixtures/'+config+'-41-v1.json','fixtures/'+config+'-39-v1.json','fixtures/'+config+'-40-v1.json']
    freeze=load(HERE/'matrix/source-freeze-v1.json')
    files=[HERE/p for p in relative]+[ROOT/r['after']['path'] for r in freeze['source_delta']]+[ROOT/r['path'] for r in freeze['new_tests']]+[ROOT/freeze['plan']['path'],ROOT/freeze['plan_snapshot']['path'],ROOT/'docs/architecture/batch-04A-41-owner-review.md']
    return sorted(set(files))
def seal():
    check_sources();reports=candidate_scan('repository-final-full')
    sources=load(HERE/'matrix/source-freeze-v1.json');files=package_files();rows=[];out=PRIVATE/'publication';out.mkdir()
    archive=out/'BESHStatNG_04A41_OWNER_REVIEW_v1.zip'
    with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for p in files:
            n=p.relative_to(ROOT).as_posix();physical.safe_name(n);data=p.read_bytes();rows.append(dict(path=n,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data)&0xffffffff:08x}'));z.writestr(n,data)
        z.writestr('.review-index.json',json.dumps(dict(kind='04A41_SMALL_OWNER_REVIEW',files=rows,self_hash_policy='Index excluded from own map,externally pinned and scanned as physical member.'),indent=2)+'\n')
    archive.chmod(stat.S_IREAD);physical_reports=[]
    with zipfile.ZipFile(archive) as z:
        members=physical.inventory(z);assert len(members)==len(rows)+1
        assert len({unicodedata.normalize('NFKC',r['path']).casefold() for r in members})==len(members)
        for phase in ['13','14']:
            m=physical.scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig');found,reviewed=m.scan(z.namelist())
            save(out/('physical-scanner-'+phase+'-v1.json'),dict(scanner=pin(m.__file__),findings=found,reviewed=reviewed,physical_members=len(members)))
            assert not found,'UNRESOLVED_PHYSICAL_PRIVACY_FINDING'
            physical_reports.append(dict(scanner=phase,source=pin(m.__file__),physical_members=len(members),index_included=True,findings=0,unresolved=0,owner_context_applied=True,scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),raw=pin(out/('physical-scanner-'+phase+'-v1.json'))))
    reader=module('independent_review41',HERE/'review_reader.py');readback=reader.verify(archive);controls=reader.adverse(archive)
    check_sources()
    for report in reports:
        m=physical.scanner(report['scanner']);assert m.paths()==[r['path'] for r in report['members']]
        for r in report['members']:exact(r)
    for r in load(PRIVATE/'new-raw-inventory-v1.json')['files']:exact(r)
    receipt=dict(status=STATUS,utc=utc(),archive=pin(archive),readiness=pin(HERE/'final/readiness-postseal-v1.json'),owner_review=pin(HERE/'final/owner-review-v1.md'),architecture_review=pin(ROOT/'docs/architecture/batch-04A-41-owner-review.md'),decision_matrix=pin(HERE/'final/decision-matrix-v1.json'),issue_delta=pin(HERE/'register/issue-delta-v2.json'),source_freeze=pin(HERE/'matrix/source-freeze-v1.json'),full_actual_candidate_scans=reports,physical_scans=physical_reports,physical_members=members,readback=readback,adverse_controls=controls,all_candidate_bytes_membership_rechecked_after_zip=True,new_raw_inventory_rechecked=True,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,main=MAIN,branch='codex/04A-41',owner_accepted=False,merged=False,phase04A42_authorized=False,excluded=['Protected raw receipts/TRX/tracebacks','Owner tokens and unsalted identifier fingerprints','Binaries,signing/install assets','Duplicated full ABI metadata indices and historical source reverse maps'],gates=GATES)
    receiptpath=out/'external-delivery-receipt-v1.json';save(receiptpath,receipt)
    for phase in ['13','14']:
        m=physical.scanner(phase);found,_=m.scan([receiptpath.relative_to(ROOT).as_posix()]);assert not found,'EXTERNAL_RECEIPT_PRIVACY_FINDING'
    receiptpath.chmod(stat.S_IREAD)
    print(json.dumps(dict(status=STATUS,archive=pin(archive),external_receipt=pin(receiptpath),members=len(members),reader_controls=len(controls))),flush=True)
if __name__=='__main__':
    try:{'prepare':prepare,'scan':initial_scan,'documents':documents,'seal':seal}[sys.argv[1]]()
    except Exception:
        if sys.argv[1]=='seal':
            save(PRIVATE/('publication-adverse-'+datetime.datetime.now().strftime('%Y%m%d%H%M%S')+'.json'),dict(status='NOT_READY_04A41',reason='Final physical/candidate/readback gate did not complete;prior candidate readiness is ineffective without matching PASS external receipt.',gates=GATES))
        raise
