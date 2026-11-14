"""Reconcile actual identities, occurrences and first failures. Never waive failures."""
from common import *
from baseline_report import python_receipt
from collections import Counter
import re
labels=['w2c6-validation-v1-python-tools','w2c6-validation-v1-python-help','w2c6-final-python-scripts-cached-v1','w2c6-validation-v1-common-python']
pys=[python_receipt(n) for n in labels]
scanner_output=(RUNROOT/'commands'/labels[2]/'stdout.log').read_bytes()
scanner=json.loads(scanner_output.decode('utf-8-sig').splitlines()[-1])
assert scanner['scanner_sources_unchanged'] is True and scanner['per_file_cache']['fresh']>0 and scanner['per_file_cache']['reused']>0
assert scanner['scanner_ast_sha256']==json.loads((HERE/'scanner-cache-proof-v1.json').read_bytes())['scan_ast_sha256']
assert json.loads((HERE/'scanner-execution-proof-v1.json').read_bytes())==dict(result=scanner,raw_stdout_sha256=sha(scanner_output),all_original_assertions_executed=True,files_filtered=0,positive_findings_waived=0)
old=json.loads((PREVIOUS/'nonpasses-v3.json').read_bytes())['python']
key=lambda r:(r['outcome'],r['identity'],r['subtest'])
a=Counter(key(r) for g in old for r in g['nonpasses']);b=Counter(key(r) for g in pys for r in g['nonpasses'])
delta=lambda c:[dict(outcome=k[0],identity=k[1],subtest=k[2],occurrences=n) for k,n in c.items()]
change=dict(extra=delta(b-a),lost=delta(a-b))
allowed='test_all_retained_native_methods_and_appearance_remain_exact (test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact)'
pin_delta=json.loads((HERE/'python-source-pin-delta-v1.json').read_bytes())
assert pin_delta['status']=='NEW_AUTHORIZED_C6_SOURCE_PIN_FAIL_EXPLAINED_NOT_WAIVED' and pin_delta['entry_projection_equals_historical_anchors'] is True
assert change==dict(extra=[dict(outcome='FAIL',identity=pin_delta['identity'],subtest='',occurrences=1)],lost=[]),('Investigate any further Python outcome delta',change)
old_ids=Counter(i for g in old for i in g['identities']);new_ids=Counter(i for g in pys for i in g['identities'])
assert old_ids==new_ids,'Python identity/occurrence population changed'
details=[]
for receipt in pys:
 path=RUNROOT/'commands'/receipt['label']/'stderr.log';text=path.read_bytes().decode(receipt['console_decoding'].split()[0],errors='strict')
 for outcome,identity,body in re.findall(r'^(FAIL|ERROR): (test_[^\n]+)\n-+\n(.*?)(?=\n={5,}|\n-{5,}\nRan |\Z)',text,re.M|re.S):
  frames=re.findall(r'File "([^"]+)", line (\d+), in ([^\n]+)',body)
  lines=[l for l in body.strip().splitlines() if l and not l.startswith((' ', 'Traceback'))]
  details.append(dict(label=receipt['label'],outcome=outcome,identity=clean(identity),frames=clean([list(f) for f in frames]),message=clean(lines[-1] if lines else ''),raw_block_sha256=sha(body.encode()),classification='INHERITED_SOURCE_PIN_OR_INHERITED_ASSERTION;NOT_WAIVED'))
nodes=[];node_ids=[]
oldnodes={n['suite']:n for n in json.loads((PREVIOUS/'validation-v3.json').read_bytes())['Node']}
for suffix in ['office-node','resolver','wasm-node']:
 p=RUNROOT/'commands'/('w2c6-validation-v1-'+suffix);raw=(p/'stdout.log').read_bytes();text=raw.decode('utf-8-sig')
 count=lambda k:int(re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',text)[-1])
 ids=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)',text,re.M)
 complete_pattern=r'^[\u2714\u2716] (.*?) \([\d.]+ms\)\r?$'
 full=re.findall(complete_pattern,text,re.M|re.S)
 prior_raw=(RUNROOT/'commands'/('w2c5-validation-v2-'+suffix)/'stdout.log').read_bytes();prior_text=prior_raw.decode('utf-8-sig')
 prior_full=re.findall(complete_pattern,prior_text,re.M|re.S)
 assert Counter(full)==Counter(prior_full),'Full Node identity/occurrence drift including multiline labels'
 hashes=[sha(name.encode()) for name in full]
 n=dict(suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=json.loads((p/'command.json').read_bytes())['exit_code'],identity_multiset_sha256=sha(json.dumps(sorted(ids)).encode()),stdout_sha256=sha(raw))
 assert len(full)==n['total'],'Node identity parsing incomplete'
 n.update(complete_identity_multiset_sha256=sha(json.dumps(sorted(hashes)).encode()),complete_identity_delta_from_C5=0,legacy_parsed_identities=len(ids),prior_raw_stdout_sha256=sha(prior_raw))
 node_ids.append(dict(suite=suffix,identities=hashes,representation='SHA256 of every complete raw display identity including embedded newline/Unicode; occurrences retained. Raw original labels remain in private execution logs'))
 assert all(n[k]==oldnodes[suffix][k] for k in ['total','passed','failed','identity_multiset_sha256','exit_code']),suffix
 nodes.append(n)
save('node-identities-v1.json',dict(suites=node_ids,scope='Every complete Node raw display-identity hash including multiline labels, preserving occurrences. Historical line-only parser omitted one identity but not the pass count; its original hash remains separately. Raw stdout hashes in validation.'))
managed=json.loads((HERE/'managed-validation-v1.json').read_bytes())
serial={s:json.loads((RUNROOT/'commands'/('w2c6-validation-v1-'+s)/'command.json').read_bytes())['exit_code'] for s in ['strict','profile','docs']}
serial_details=[]
for label,exit_code in serial.items():
 streams={}
 for name in ['stdout','stderr']:
  data=(RUNROOT/'commands'/('w2c6-validation-v1-'+label)/(name+'.log')).read_bytes()
  try:value=json.loads(data)
  except (ValueError,UnicodeError):value=data.decode('utf-8-sig',errors='replace')
  streams[name]=dict(raw_sha256=sha(data),sanitized_value=clean(value))
 serial_details.append(dict(label=label,exit_code=exit_code,streams=streams))
save('serial-findings-v1.json',dict(results=serial_details,classification='Actual strict/profile findings retained; exit1 is not a pass or a scientific/native gate waiver'))
save('python-first-failures-v1.json',dict(records=details,raw_receipts_retained=True,reason='All original traceback blocks hashed; sanitized exact exception text and complete test stack locations retained'))
save('nonpasses-v1.json',dict(managed=json.loads((HERE/'managed-Debug-identities-v1.json').read_bytes())['nonpasses'],python=pys,python_occurrence_delta_from_C5=change,python_identity_occurrence_delta=0,
 prior_receipts=dict(C4=dict(passed=765,failed=181,errors=10),C5=dict(passed=765,failed=180,errors=11)),
 changed_python_classification='See python-nonpass-investigation.json and python-first-failures-v1.json. Inherited unordered-SPC set chooses the first existing bad member. ERROR remains ERROR; no test or production policy changed to reconcile totals.',
 interrupted_attempt=dict(label='w2c6-validation-v1-python-scripts',exit_code=json.loads((RUNROOT/'commands/w2c6-validation-v1-python-scripts/command.json').read_bytes())['exit_code'],reason='Own exact PID/path/start-time checked then stopped after2751sec in inherited publication scans; incomplete raw run retained; excluded from complete suite totals. Complete unchanged tests rerun with a separately proven necessary-condition regex guard.'),
 qualification='No inherited source-hash assertion waived. Every identity, subtest, occurrence and raw exception remains; failure details and new source transitions recorded separately.',original_queries=0))
validation=dict(status='C6_FRESH_PARTIAL_W2_VALIDATION_NOT_BATCH_READY',managed_Debug=managed['Debug'],selected_Release=managed['selected_Release'],Node=nodes,Node_total=sum(n['passed'] for n in nodes),Python={k:sum(r[k] for r in pys) for k in ['total','passed','failed','errors']},Python_occurrence_delta=change,Python_identity_occurrence_delta=0,
 strict_profile_MkDocs=serial,controls=dict(new_checkpoint=41,retained_preedit=28),
 current_frozen_comparison_per_configuration=dict(preedit=28966,late_A_to_G=458,total=29424,separate_provenance=True),compiled_exports=dict(Debug=268,Release=268),new_original_queries=0,
 text_domain_policy='UNCHANGED;1746360 source-domain observations/config,0 differences;8 synthetic out-of-domain differences/config retained',
 Python_execution='All unchanged tests/assertions. Tools traversal excludes only previously approved ignored components before descent. Scripts scanner regex necessary-condition guard proved equivalent on12068 samples; positive cases use original regex. Identical 04A13/14 scan/git AST permits per-file result reuse keyed by ROOT, environment, path and actual bytes after original scan. Sources authenticated unchanged. Both prior incomplete scripts attempts retained separately.',
 NOT_RUN=['Full managed Release:NOT_RUN_OWNER_PREFERENCE','W3:W2_NOT_COMPLETE','Canonical portable/Legacy wrappers:implicit restore/shared outputs/XLL packaging; safe isolated offline equivalents used','Native Excel/COM/UIA/RefEdit/workbook smoke','Independent scientific/R validation','Manual Office/Mac/UI','Network/package restore/install/installer/signing/deployment','Phase04A39'],
 gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'))
save('validation-v1.json',validation)
summary=json.loads((HERE/'ownership-summary-v1.json').read_bytes())
save('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',mandatory_blockers=dict(historical_OPEN=summary['historical']['OPEN'],current_OPEN=summary['current']['OPEN'],distinct_support_OPEN=summary['support']['distinct_OPEN']),
 validation=validation,new_original_queries=0,owner_decision_needed=False,readiness_verifier='NONE: partial fail-closed reader never claims complete W2 or READY'))
print(json.dumps(dict(Python=validation['Python'],delta=change,Node=validation['Node_total'])))
