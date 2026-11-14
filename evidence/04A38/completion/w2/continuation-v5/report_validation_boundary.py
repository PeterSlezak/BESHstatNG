from common import *
from baseline_report import python_receipt
from collections import Counter
import re,copy

managed=json.loads((HERE/'managed-validation-v3.json').read_bytes())
tools=python_receipt('w2c5-final-python-tools-pruned')
tools_raw=python_receipt('w2c5-validation-v2-python-tools')
assert Counter(tools['identities'])==Counter(tools_raw['identities']) and all(tools[k]==tools_raw[k] for k in ['total','passed','failed','errors'])
help_lane=python_receipt('w2c5-validation-v2-python-help')
scripts_raw=python_receipt('w2c5-validation-v2-python-scripts')
scripts=copy.deepcopy(scripts_raw)
target=python_receipt('w2c5-final-python-privacy13-after-encoding')
assert target['total']==1 and target['failed']==1 and target['errors']==0 and len(target['nonpasses'])==1
identity=target['nonpasses'][0]['identity']
rows=[r for r in scripts['nonpasses'] if r['identity']==identity]
assert len(rows)==1 and rows[0]['outcome']=='ERROR'
log=(RUNROOT/'commands/w2c5-validation-v2-python-scripts/stderr.log').read_text(encoding='utf-8-sig')
assert 'preservation.json' in log and ('FileNotFoundError' in log or 'UnicodeDecodeError' in log)
scripts['nonpasses']=[r for r in scripts['nonpasses'] if r['identity']!=identity]+target['nonpasses']
scripts['failed']+=1;scripts['errors']-=1
scripts['label']='CORRECTED_FINAL_SCRIPTS_IDENTITY_OCCURRENCE_VIEW'
scripts['raw_label']=scripts_raw['label'];scripts['replacement_receipt']=target
common=python_receipt('w2c5-validation-v4-common-python')
pys=[tools,help_lane,scripts,common]
old=[python_receipt('w2c4-validation-v1-'+s) for s in ['python-tools','python-help','python-scripts','common-python']]
key=lambda r:(r['outcome'],r['identity'],r['subtest'])
a=Counter(key(r) for lane in old for r in lane['nonpasses']);b=Counter(key(r) for lane in pys for r in lane['nonpasses'])
extra=[dict(outcome=k[0],identity=k[1],subtest=k[2],occurrences=n) for k,n in (b-a).items()]
lost=[dict(outcome=k[0],identity=k[1],subtest=k[2],occurrences=n) for k,n in (a-b).items()]
assert Counter((r['identity'],r['subtest']) for lane in old for r in lane['nonpasses'])==Counter((r['identity'],r['subtest']) for lane in pys for r in lane['nonpasses'])
assert all('test_all_retained_native_methods_and_appearance_remain_exact' in r['identity'] for r in extra+lost)
nodes=[]
for suffix in ['office-node','resolver','wasm-node']:
    path=RUNROOT/'commands'/('w2c5-validation-v2-'+suffix);data=(path/'stdout.log').read_bytes();text=data.decode('utf-8-sig')
    count=lambda label:int(re.findall(r'(?:# |\u2139 )'+label+r' (\d+)',text)[-1])
    ids=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)',text,re.M)
    nodes.append(dict(suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=json.loads((path/'command.json').read_bytes())['exit_code'],identity_multiset_sha256=sha(json.dumps(sorted(ids)).encode()),stdout_sha256=sha(data)))
serial={s:json.loads((RUNROOT/'commands'/('w2c5-validation-v4-'+s)/'command.json').read_bytes())['exit_code'] for s in ['strict','profile','docs']}
raw_totals={k:sum(lane[k] for lane in [tools_raw,help_lane,scripts_raw,common]) for k in ['total','passed','failed','errors']}
final_totals={k:sum(lane[k] for lane in pys) for k in ['total','passed','failed','errors']}
save('python-encoding-correction-final.json',dict(status='ONE_EXACT_IDENTITY_SETUP_ERROR_CORRECTED_TO_OBSERVED_INHERITED_FAIL',
    raw_full_source=scripts_raw,targeted_unchanged_assertion=target,corrected_scripts=scripts,raw_aggregate=raw_totals,corrected_aggregate=final_totals,
    identity=identity,occurrences=1,reason='Exact-byte relocation of new C5 UTF16 diagnostic removes cached-path/encoding setup failure. The unchanged privacy assertion still FAILS; it is not waived or promoted.',
    delta_from_C4=dict(extra=extra,lost=lost),all_other_nonpass_identities_and_occurrences_equal=True))
save('nonpasses-v3.json',dict(managed=json.loads((HERE/'managed-Debug-identities-v3.json').read_bytes())['nonpasses'],python=pys,raw_python_scripts=scripts_raw,
    python_occurrence_delta_from_C4=dict(extra=extra,lost=lost),C4_reported=dict(passed=765,failed=181,errors=10),
    qualification='One corrected C5 diagnostic setup ERROR becomes its actual observed inherited FAIL; every raw attempt remains. The only remaining outcome movement from C4 is the same inherited unordered-SPC FAIL-to-ERROR identity. No old assertion, identity or occurrence was removed.',
    failed_attempts='All earlier C5/C4/C3/C2/C1 attempts plus byref-boundary-implementation.json and original/current alias probes remain immutable',original_queries=0))
validation=dict(status='FINAL_C5_PARTIAL_W2_VALIDATION_NOT_BATCH_READY',managed_Debug=managed['Debug'],selected_Release=managed['selected_Release'],Node=nodes,
    Python=final_totals,Python_raw_before_setup_correction=raw_totals,Python_occurrence_delta=dict(extra=extra,lost=lost),strict_profile_MkDocs=serial,
    current_frozen_comparison_per_configuration=dict(preedit=28966,late_A_to_G=458,total=29424,separate_provenance=True),compiled_exports=dict(Debug=268,Release=268),
    new_original_queries=0,text_domain_policy='FRESH cells-v5:0 source-domain differences/1608882 checks/config;8 out-of-domain raw synthetic differences retained; no policy change',
    NOT_RUN=['Full managed Release:OWNER_PREFERENCE','W3 readiness verifier:W2_NOT_COMPLETE','Canonical portable/Legacy wrappers:implicit restore/shared outputs/XLL packaging',
             'Native Excel/COM/UIA/RefEdit/in-workbook smoke','Independent R/scientific original control','Office/Mac/manual GUI smoke','Network/package restore/install/MSI/signing/deployment','Phase04A39'],
    gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'))
save('validation-v3.json',validation)
print(json.dumps(dict(Debug=managed['Debug']['outcomes'],selected_Release=managed['selected_Release']['outcomes'],Node=sum(r['passed'] for r in nodes),Python=final_totals,Python_raw=raw_totals,delta=dict(extra=extra,lost=lost),serial=serial)))
