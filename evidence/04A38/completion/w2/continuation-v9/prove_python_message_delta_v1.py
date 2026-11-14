"""Investigate changed historical assertion messages from exact C8->C9 source views."""
from common_v2 import *
import ast,importlib.util,re
from collections import Counter
old=json.loads((PREVIOUS/'python-first-failures-v1.json').read_bytes())['records'];now=json.loads((HERE/'python-first-failures-v2.json').read_bytes())['records'];trans=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'];by={t['path']:t for t in trans}
def key(r):return r['identity'],r['outcome'],r.get('subtest'),r['occurrence']
before={key(r):r for r in old};changed={r['identity']:(before[key(r)],r) for r in now if key(r) in before and r['message']!=before[key(r)]['message']};proofs={}
def module(path):
    spec=importlib.util.spec_from_file_location('c9_message_'+Path(path).stem.replace('-','_'),ROOT/path);obj=importlib.util.module_from_spec(spec);spec.loader.exec_module(obj);return obj
def viewed(path,previous=False):return (ROOT/by[path]['preimage']['path']).read_bytes() if previous and path in by and by[path]['before'] else (ROOT/path).read_bytes()
def digest(path,previous=False):return sha(viewed(path,previous))
newdiff={'BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb','BESHstatNG/src/RegModels/BinaryClassificationReporting.vb'}
for p in newdiff:assert viewed(p,True).replace(b'\r\n',b'\n')==git('show',BASE+':'+p).replace(b'\r\n',b'\n') and digest(p)!=digest(p,True)
for identity,contractpath,exportonly in [
 ('test_04a27_geometry_completion.Geometry27CompletionTests.test_real_consumer_routes_and_udf_exports_preserved','scripts/verify-04a27-geometry-contract.py',False),
 ('test_04a27_transport_successor.Transport27SuccessorTests.test_udf_exports_signatures_and_packing_are_preserved','scripts/verify-04a27-contract.py',True)]:
    c=module(contractpath);baselinepath=c.RUN/'baseline.json';pins=json.loads(baselinepath.read_bytes())['source_pins'];files=list((ROOT/'BESHstatNG/src/ExcelUDFs').rglob('*.vb'))
    paths=[p.relative_to(ROOT).as_posix() for p in files if not exportonly or re.search(r'<ExcelFunction\b',p.read_text(encoding='utf-8-sig'),re.I)]
    frontiers=[]
    for previous in [True,False]:
        p=next(p for p in paths if pins[p]!=digest(p,previous));frontiers.append(dict(path=p,expected=pins[p],actual=digest(p,previous)))
    a,b=changed[identity]
    for r,f in zip([a,b],frontiers):assert f['expected'] in r['message'] and f['actual'] in r['message']
    assert frontiers[1]['path'] in newdiff
    proofs[identity]=dict(classification='RETAINED_SOURCE_PIN_FAIL_FIRST_FAILING_FILE_CHANGED_IN_UNCHANGED_FILESYSTEM_ENUMERATION',contract=pin(ROOT/contractpath),baseline=pin(baselinepath),original_file_enumeration=paths,old_first=frontiers[0],new_first=frontiers[1],transition=by[frontiers[1]['path']],ABI='Compiled268-export metadata separately exact; assertion compares entire source file bytes.')
for identity,contractpath,owner in [
 ('test_04a13_survival_workflow.Survival13SourceContracts.test_all_prechange_protected_bytes_except_exact_approved_scope','scripts/capture-04a13-contract.py',False),
 ('test_04a14_mixed_workflow.Mixed14SourceContracts.test_exact_scope_and_external_owner_plan_are_separate','scripts/capture-04a14-contract.py',True)]:
    c=module(contractpath);baselinepath=c.RUN/'baseline.json';pins=json.loads(baselinepath.read_bytes())['source_pins'];allowed=c.CHANGED|({c.OWNER_PLAN} if owner else set());errors=[[p for p,h in pins.items() if p not in allowed and digest(p,previous)!=h] for previous in [True,False]]
    assert set(errors[1])-set(errors[0])==newdiff and not set(errors[0])-set(errors[1])
    for event,paths in zip(changed[identity],errors):assert int(re.search(r'Second list contains (\d+) additional elements',event['message'])[1])==len(paths)
    proofs[identity]=dict(classification='RETAINED_HISTORICAL_SCOPE_FAIL_TWO_AUTHORIZED_SOURCE_TRANSITIONS',contract=pin(ROOT/contractpath),baseline=pin(baselinepath),old_errors=errors[0],new_errors=errors[1],additional=sorted(newdiff),unrelated_or_protected_change=False)
for identity in ['test_04a22_spc.Spc22Contracts.test_publication_membership_hashes_empty_index_and_privacy','test_04a24_multivariate_payload.MultivariatePayload24.test_publication_exact_membership_hashes_and_empty_index']:
    a,b=changed[identity];sets=[ast.literal_eval(r['message']) for r in [a,b]];assert all(isinstance(s,set) for s in sets) and sets[1]-sets[0]==newdiff and not sets[0]-sets[1]
    proofs[identity]=dict(classification='RETAINED_HISTORICAL_PUBLICATION_SCOPE_FAIL_TWO_AUTHORIZED_SOURCE_TRANSITIONS',old_paths=sorted(sets[0]),new_paths=sorted(sets[1]),additional=sorted(newdiff),transitions=[by[p] for p in sorted(newdiff)])
identity='test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change';a,b=changed[identity]
def paths_from_false(message):
    value=ast.literal_eval(message.removesuffix(' is not false'));assert isinstance(value,bytes);return value.decode().splitlines()
pa,pb=paths_from_false(a['message']),paths_from_false(b['message']);assert set(pb)-set(pa)=={'BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb'} and not set(pa)-set(pb)
proofs[identity]=dict(classification='RETAINED_HISTORICAL_NO_UDF_EDIT_FAIL_AUTHORIZED_AGREEMENT_EXTRACTION',old_paths=pa,new_paths=pb,transition=by['BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb'],designers_importer_targets_signing_changed=False)
save('python-additional-message-source-proof-v1.json',dict(proofs=proofs,original_queries=0,scope='Read exact historical baseline files only, never traverse TestResults. Contracts imported for declarations/constants only; no check/original/fit/query function invoked. All old/new assertion messages remain FAIL.'))
source=(HERE/'report_nonpass_investigation_v2.py').read_text(encoding='utf-8');source=source.replace('extra=[];changes=[];same=0',"extra=[];changes=[];same=0\nadditional=json.loads((HERE/'python-additional-message-source-proof-v1.json').read_bytes())['proofs']")
source=source.replace("    else:\n        need(r['identity'] in", "    elif r['identity'] in additional:\n        row.update(additional[r['identity']])\n    else:\n        need(r['identity'] in")
source=source.replace("category='FIXED_PUBLIC_REFERENCE_PATH_SOURCE' if", "category='SERIALIZED_CRLF_AFTER_SOURCE_LABEL_COLON_NOT_A_DRIVE_PATH' if r['path'].endswith('dependency-source-universe-v1.json') else 'SYNTHETIC_SCANNER_SID_EMAIL_HOST_CONNECTION_CONTROLS' if r['path'].endswith('prove_parallel_scan_v1.py') else 'FIXED_PUBLIC_REFERENCE_PATH_SOURCE' if")
with (HERE/'report_nonpass_investigation_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
print('Additional exact source/message proofs',len(proofs))
