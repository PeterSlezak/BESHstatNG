"""Additive candidate gates and physical final witness, with external seal receipt."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from remedy import *

class ProbeMember:
    def __init__(self,value):self.value=value
    def is_file(self):return True
    def read_text(self,encoding):assert encoding=='utf-8-sig';return self.value
class ProbeRoot:
    parents=ROOT.parents
    name=ROOT.name
    def __init__(self,value):self.value=value
    def __truediv__(self,path):return ProbeMember(self.value)
    def as_posix(self):return ROOT.as_posix()
    def __fspath__(self):return str(ROOT)

def controls(phase):
    result=[]
    for number,token in enumerate(sorted(physical.tokens())):
        variations=[token,token.swapcase(),''.join(chr(ord(c)+0xfee0) if 33<=ord(c)<=126 else c for c in token)]
        for v,variation in zip(variations,['literal','case_variant','NFKC_variant']):
            m=scanner(phase);m.ROOT=ProbeRoot(json.dumps(dict(value=v)))
            a,ar=m.scan(['identity-control.json']);m.re=re;b,br=m.scan(['identity-control.json'])
            assert a==b and ar==br and any(r['category']=='known_private_identifier' for r in a)
            result.append(dict(identity_index=number,variation=variation,detected=True,guard_and_direct_original_re_exact=True))
    return result

def candidates(label,public=False):
    lock();reports=[]
    for phase in ['13','14']:
        m=scanner(phase);paths=m.paths();members=[pin(p) for p in paths]
        found,reviewed=m.scan(paths)
        assert members==[pin(p) for p in paths],'CANDIDATE_CHANGED_DURING_SCAN'
        save(PRIVATE/(label+'-'+phase+'.json'),dict(scanner=pin(m.__file__),candidate_inputs=members,findings=found,reviewed=reviewed))
        assert not found,'UNRESOLVED_CURRENT_CANDIDATE_PRIVACY_FINDINGS'
        assert not any(p['raw'] in paths for p in PAIRS) and all(p['safe'] in paths for p in PAIRS)
        reports.append(dict(scanner=phase,source=pin(m.__file__),scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),guard=pin(ROOT/'evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py'),members=members,candidate_count=len(paths),findings=0,unresolved_new_04A40=0,status='PASS',identity_controls=controls(phase),raw=pin(PRIVATE/(label+'-'+phase+'.json')),normalized_path_policy='Exact unchanged paths() output from Git; no added exclusion. Original NFKC/content/JSON/base64 rules retained.',guard_skips=m.re.skips,guard_fallbacks=m.re.fallbacks))
        print('Fresh actual candidate original scanner',phase,len(paths),'members;0 findings',flush=True)
    if public:
        save('repository-privacy-witness-v3.json',dict(status='PASS_FOR_EXACT_RECORDED_CURRENT_CANDIDATE_INPUTS',reports=reports,scope='Both complete actual unchanged repository-candidate sets; no staging surrogate, ZIP or filtered manifest substituted.',rules_and_ignore_files_unchanged=True,waivers=0,token_values_and_unsalted_identifier_hashes_published=False,final_metadata_gate='Subsequent final full actual-candidate scan includes this receipt and all final review/readiness metadata; external-delivery-receipt-v3.json seals that immutable set. This initial receipt is not falsely a self-hash.',historical=dict(accepted39_privacy_parents='FAIL',findings_each=1941,earlier_each=1599,added_each=342,W3='TWO_PRIVACY_ERROR_PARENTS_UNWAIVED',original54_differences='RETAINED_UNWAIVED',current32_differences='RETAINED_UNWAIVED'),pre_removal_original_raw='FAIL_WITH_8_UNIQUE_OCCURRENCES_RETAINED',post_removal_new_findings=0))
    return reports

def label_representations():
    cross=load(HERE/'raw-presentation-crosswalk-v3.json')
    for item in cross['artifacts']:
        p=ROOT/(item['representation']['path'].removesuffix('.json')+'-candidate-provenance-v3.json')
        save(p,item)

def document_final():
    lock();freeze=source_lock();cross=load(HERE/'raw-presentation-crosswalk-v3.json')
    preservation=load(HERE/'preservation-readback-v3.json');guards=load(HERE/'scope-guard-dispositions-v3.json')
    privacy=load(HERE/'repository-privacy-witness-v3.json')
    assert len(cross['artifacts'])==2 and all(r['findings']==0 for r in privacy['reports'])
    assert [g['status'] for g in guards['guards']]==['FAIL']*3
    save('source-lock-v3.json',dict(status='LIVE_SOURCE_SHIPPING_ABI_COMPATIBILITY_PINS_AUTHENTICATED',source_pins=freeze['source_pins'],authorized_production_files=[r['after'] for r in freeze['source_delta'] if r['after']['path'].endswith('.vb')],new_tests=freeze['new_tests'],frozen_source_manifest=pin(ROOT/'evidence/04A40/matrix/source-freeze-v2.json'),preservation=pin(HERE/'preservation-readback-v3.json'),ABI_proofs=[pin(ROOT/('evidence/04A40/abi-cil/'+c+'-proof-v2.json')) for c in ['Debug','Release']],compatibility_receipts=[pin(ROOT/('evidence/04A40/compatibility/'+c+'-current-only-v4.json')) for c in ['Debug','Release']],exports_per_configuration=268,cases_per_configuration=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,runtime_source_changed=False))
    previous=load(ROOT/'evidence/04A40/publication/physical-readback-v1.json')
    exact(previous['archive'])
    with zipfile.ZipFile(ROOT/previous['archive']['path']) as z:
        for item in cross['artifacts']:
            data=z.read(item['original']['path'])
            assert len(data)==item['representation']['bytes'] and sha(data)==item['representation']['sha256']
            assert sha(data)!=item['original']['sha256']
    save('physical-readback-provenance-v3.json',dict(kind='AUTHENTICATED_SANITIZED_SUCCESSOR_SUMMARY;NOT_RAW_EXECUTION',previous_archive=previous['archive'],previous_receipt=pin(ROOT/'evidence/04A40/publication/physical-readback-v1.json'),previous_members=399,previous_indexed=398,prior_physical_scanner_results='RETAINED_PASS;NOT_FRESHLY_RERUN_ON_PREVIOUS_ZIP',fresh_prior_zip_sha_and_two_report_member_pins_verified=True,original_indexed_paths_contain='REVIEW_RENDERED_BYTES;NOT_EXACT_RAW_REPORT_BYTES',rendered_reports=[dict(indexed_path=i['original']['path'],raw=i['original'],review=i['representation']) for i in cross['artifacts']],prior_raw_scanner_results='FAIL_RETAINED',new_final_witness='Exact safe-v2 member names used; no raw-looking aliases for review bytes. Every new physical member/index is scanned; authoritative physical results are external to the immutable ZIP.'))
    save('git-final-v4.json',dict(utc=utc(),branch='codex/04A-40',HEAD=MAIN,main=MAIN,merge_base=MAIN,accepted39='f2eb060c9c422c1fec4e82989d07907dbd4b9337',index=pin('.git/index'),staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),untracked_at_receipt=git('ls-files','--others','--exclude-standard').decode().splitlines(),authorized_raw_path_removals=[p['raw'] for p in PAIRS],final_membership='Complete later actual candidate membership, file hashes and scans are sealed externally; this receipt does not claim future file self-hashes.',stage_commit_merge_rebase_push_release=False))
    conditions=[dict(gate='Two authorized review representations',outcome='AUTHENTICATED',evidence='raw-presentation-crosswalk-v3.json'),dict(gate='Current actual Git candidate privacy',outcome='ZERO_UNRESOLVED_UNDER_BOTH_ORIGINAL_SCANNERS',evidence='repository-privacy-witness-v3.json;final external seal includes later metadata'),dict(gate='Historical guard outcomes',outcome='3_FAIL;SCOPE_APPLICABILITY_DOCUMENTED;UNWAIVED',evidence='scope-guard-dispositions-v3.json'),dict(gate='Source/preservation/ABI/compatibility',outcome='AUTHENTICATED_NO_RUNTIME_CHANGE',evidence='preservation-readback-v3.json;source-lock-v3.json'),dict(gate='Final physical witness',outcome='REQUIRES_MATCHING_PASS_EXTERNAL_DELIVERY_RECEIPT',evidence='external-delivery-receipt-v3.json outside ZIP')]
    result=dict(status=STATUS,kind='ADDITIVE_OWNER_ACCEPTANCE_REVIEW_READINESS;NOT_OWNER_ACCEPTANCE',effective_only_with_matching_external_delivery_receipt=True,conditions=conditions,authorization=dict(representation_exact='I authorize versioned, sanitized **Git-candidate representations** of the two newly created reporting artifacts.',additional_exact_response='I authorize the reuquest',additional_scope='Exact preceding request: remove only the two raw reports from repository working-tree paths after independently verified immutable separately pinned copies; retain original path provenance and failures; change no scanner or ignore rules.',longer_draft_not_separately_granted=True),actual_operations=['Created read-only separately pinned exact copies and preserved timestamps/path provenance','Independently verified protected copies before removing exactly two untracked raw repository paths','Kept both safe v2 representations byte-exact and added explicit candidate provenance labels','Added scoped guard/privacy/preservation/readiness/review evidence; no historical result replaced'],retained_nonpasses=dict(managed_Debug='14_FAIL',targeted_Release='2_FAIL',Python_parents=dict(PASS=830,FAIL=115,ERROR=11),Python_events=dict(FAIL=187,ERROR=11),guard_failures=3,accepted39_privacy='2_FAIL_PARENTS_1941_EACH;1599+342',W3='2_PRIVACY_ERROR_PARENTS',historical_diagnostic_differences=54,current_diagnostic_differences=32,strict_profile_MkDocs_exit_codes=[1,1,0]),full_Release='NOT_RUN',scientific_native='NOT_RUN;NOT_GOLDEN;NO_GATE_DISCHARGED',register=dict(entries=1169,obligations=1514,restricted_controls=802,mappings=2967,investigations=20),continuous_physical_identity='NOT_VERIFIED;788_PREVIOUS_ID_DRIFTS_RETAINED',W3_37484_154='INHERITED_QUALIFIED;NOT_FRESH_AUDIT',gates=GATES,accepted=False,committed=False,merged=False,phase04A41_authorized=False)
    save('readiness-postseal-v4.json',result)
    save('final-decision-matrix-v3.json',dict(result,readiness_document=pin(HERE/'readiness-postseal-v4.json')))
    review='''# 04A.40 owner acceptance review with documented restrictions

READY_FOR_OWNER_04A40_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS becomes authoritative only with the matching PASS external-delivery-receipt-v3.json. This is review readiness, not owner acceptance, a scientific golden result, native readiness or authority for 04A.41.

The original authority is: "I authorize versioned, sanitized **Git-candidate representations** of the two newly created reporting artifacts." The owner subsequently answered "I authorize the reuquest" to the exact request to remove only the two untracked raw repository reports after independently verifying immutable separately pinned exact copies in controlled owner-local storage. No longer draft authorization is claimed.

Both raw reports were copied with original path, timestamp, size and SHA provenance. Read-only copies and an independently pinned local manifest were verified with PowerShell and Python before removal of exactly those two repository paths. Both prepared review-v2 documents remain byte-exact. Their adjacent candidate provenance documents explicitly label them sanitized review/candidate documents. Build output-directory metadata is rendered; only dotnet-info stdout path formatting differs in the other report. Command arguments, outcomes, other JSON values and raw output pins retain their original meaning. A raw stdout hash is not represented as the rendered text hash.

Both original scanners ran over the actual complete Git candidate before removal: eight unique occurrences (six build receipt, two tool metadata); FAIL remains recorded. After correction, both unchanged scanners found zero unresolved current candidate findings. Real checkout owner/host identity controls include case and NFKC variants, with no identity values or unsalted identity fingerprints published. Final full-candidate scans include this review and all new metadata and are sealed externally. No candidate is hidden by a scanner/ignore change.

The older physical review ZIP is independently hash-verified and its two report members are verified as review-rendered bytes at raw-looking indexed paths. They are not original raw transcripts. The new small witness uses explicit safe representation names. It contains the latest readiness, Git, candidate privacy, source-lock and physical provenance records. Its external receipt binds every physical member, index, CRC, SHA, size and independent readback, and scans every physical member with both original scanners.

Three unchanged historical guards were rerun individually: 3 FAIL / 0 ERROR. The no-UDF-diff assertions are literally violated by the authorized nine UDF parent edits; the alignment guard also matches UdfLmmInputPreparation38.vb. Scope explanations and export/ABI evidence do not convert any FAIL to PASS or waive it. All 32 current and 54 older diagnostic differences remain. Historical39 privacy parents remain FAIL with 1941 findings each (1599+342); W3 retains its two privacy ERROR parents.

Retained campaigns, not rerun: full Debug 21125 PASS /14 FAIL; targeted Release 3108 PASS /2 FAIL; full Release NOT_RUN; Python 830 PASS /115 FAIL /11 ERROR parents, 187 FAIL /11 ERROR events; Node 916 PASS; strict/profile/MkDocs exits 1/1/0. Shipping pins, 268 exports and 29424 compatibility cases per configuration were authenticated: 28966 PRE_EDIT_FROZEN plus 458 LATE_ARCHIVED_ENTRY_SUPPLEMENT. The targeted fresh runner uses existing Python 3.13.18; the retained campaign used Python 3.14. No installation occurred.

Fresh byte readback verifies the 468 checkpoint files/copies (464 live exact, four prior authorized newline renderings), 8984 listed protected paths, 1353 source-entry identities, 15 production transitions, the plan transition and two new tests. Continuous physical file identity remains NOT_VERIFIED; 788 prior ID drifts are retained. The W3 37484/154 union is inherited qualified, not freshly audited. The canonical register retains 1169 entries, 1514 obligations, 802 restricted controls, 2967 mappings and 20 investigations. No C13, C12, other scientific/native correction or original-obligation discharge follows.

Fresh checks: original candidate scans and identity controls, three historical guards, unchanged evidence-reader/adverse controls, exact Git/source/preservation/shipping/ABI/compatibility checks, archive scanner/readback controls. No full managed/Release/Python/Node/compatibility campaign was rerun. No live Excel, COM, UIA, R or WASM manual check was run. No runtime source or existing test changed, and no staging, commit, merge, rebase, push, release, installer or signing action occurred.

P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A41_NOT_AUTHORIZED
'''
    with (HERE/'owner-review-v3.md').open('x',encoding='utf-8',newline='\n') as f:f.write(review)
    save('physical-gate-policy-v3.json',dict(kind='EXTERNAL_SEAL_POLICY;NOT_A_FABRICATED_PHYSICAL_PASS',index='.review-index.json',scope='Every physical member, index and final metadata under both unchanged original scanners with real owner context. Zero findings required; no fixture whitelist.',external_receipt='external-delivery-receipt-v3.json',no_circular_hash='The external receipt binds ZIP SHA/size, all actual candidates, physical member results and independent readback; it is not itself a ZIP member.',required_latest_members=['readiness-postseal-v4.json','git-final-v4.json','repository-privacy-witness-v3.json','physical-readback-provenance-v3.json','source-lock-v3.json','owner-review-v3.md']))

def package():
    lock();source_lock()
    before=[pin(p) for p in HERE.glob('*') if p.is_file()]
    final_reports=candidates('final-full-candidate')
    assert before==[pin(p) for p in HERE.glob('*') if p.is_file()]
    files=[p for p in HERE.iterdir() if p.is_file() and p.suffix in ['.py','.json','.md']]
    for pair in PAIRS:
        files.extend([ROOT/pair['safe'],ROOT/(pair['safe'].removesuffix('.json')+'-candidate-provenance-v3.json')])
    for p in ['final/readiness-postseal-v3.json','final/git-final-v3.json','python/repository-privacy-witness-v2.json','publication/physical-readback-v1.json','final/owner-review-v2.md']:
        files.append(ROOT/'evidence/04A40'/p)
    files=sorted(set(files));rows=[]
    archive=PRIVATE/'BESHStatNG_04A40_FINAL_WITNESS_v4.zip'
    with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for path in files:
            name=path.relative_to(ROOT).as_posix();physical.safe_name(name)
            data=path.read_bytes();assert name not in {p['raw'] for p in PAIRS}
            rows.append(dict(path=name,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data)&0xffffffff:08x}'))
            z.writestr(name,data)
        z.writestr('.review-index.json',json.dumps(dict(kind='FINAL_SMALL_REVIEW_WITNESS',files=rows,self_hash_policy='Index excluded from own map; included in physical scan and external member manifest.'),indent=2)+'\n')
    archive.chmod(stat.S_IREAD)
    reports=[]
    with zipfile.ZipFile(archive) as z:
        inventory=physical.inventory(z);assert len(inventory)==len(rows)+1
        for phase in ['13','14']:
            m=scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig')
            found,reviewed=m.scan(z.namelist());assert not found,'FINAL_PHYSICAL_PRIVACY_FINDING'
            reports.append(dict(scanner=phase,source=pin(m.__file__),physical_members=len(inventory),index_included=True,findings=0,unresolved=0,reviewed=reviewed,owner_context_applied=True,scan_AST_sha256=sha(physical.function_ast(m,'scan').encode())))
    reader=imported('independent_final_readback',HERE/'readback.py')
    readback=reader.verify(archive)
    probes=reader.adverse(archive)
    assert pin('.git/index')['sha256']==INDEX and not git('diff','--check')
    original=load(PRIVATE/'preservation-before-removal.json')
    for item in original['originals']:exact(item['protected_copy'])
    for r in before:exact(r)
    for r in final_reports:
        m=scanner(r['scanner']);assert m.paths()==[p['path'] for p in r['members']]
        for item in r['members']:exact(item)
    delivery=dict(status=STATUS,utc=utc(),archive=pin(archive),owner_review=pin(HERE/'owner-review-v3.md'),readiness=pin(HERE/'readiness-postseal-v4.json'),decision_matrix=pin(HERE/'final-decision-matrix-v3.json'),candidate_privacy=dict(status='PASS',final_full_actual_candidate_reports=final_reports),physical_privacy=dict(status='PASS',reports=reports),physical_members=inventory,independent_readback=readback,readback_adverse_controls=probes,raw_copies_rechecked=True,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,all_public_witness_bytes_unchanged_after_final_scan=True,raw_paths_absent=[p['raw'] for p in PAIRS],provenance='No protected raw receipts, identity tokens/fingerprints, raw TRX, source-reverse mappings or binary outputs in physical witness.',gates=GATES,owner_accepted=False,no_commit_merge_or_next_phase_authority=True)
    save(PRIVATE/'external-delivery-receipt-v3.json',delivery)
    # The external delivery receipt is outside the ZIP but is itself privacy scanned.
    for phase in ['13','14']:
        m=scanner(phase);found,_=m.scan([(PRIVATE/'external-delivery-receipt-v3.json').relative_to(ROOT).as_posix()]);assert not found,'EXTERNAL_DELIVERY_PRIVACY_FINDING'
    (PRIVATE/'external-delivery-receipt-v3.json').chmod(stat.S_IREAD)
    print(json.dumps(dict(status=STATUS,archive=pin(archive),external_receipt=pin(PRIVATE/'external-delivery-receipt-v3.json'),members=len(inventory))),flush=True)

if __name__=='__main__':
    {'labels':label_representations,'scan':lambda:candidates('post-correction',public=True),'documents':document_final,'package':package}[sys.argv[1]]()
