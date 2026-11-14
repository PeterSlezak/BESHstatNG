"""Synthetic adverse inputs; no historical bytes, tests or scanners modified/run."""
from common import *
import copy
import verify_c17_checkpoint_v1 as reader
from verify_c17_transport_v1 import safe_path,validate_manifest
from private_preservation_v1 import validate_file,file_identity
controls=[]
def rejected(name,call):
 try:call()
 except (AssertionError,ValueError,FileNotFoundError,KeyError):controls.append(dict(name=name,status='REJECTED'))
 else:raise AssertionError('Adverse control accepted '+name)
path='BESHstatNG/src/ExcelUDFs/UdfScalarUtilities.vb'
rejected('missing actual source',lambda:reader.compute(overrides={path:None},proofs=False))
rejected('modified actual source',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'},proofs=False))
d=json.loads((HERE/'canonical-identity-status-v3.json').read_bytes());bad=copy.deepcopy(d);next(r for r in bad['entries'] if r['transition']=='NEW')['proof']={}
rejected('unsupported closure without whole-body proof',lambda:reader.validate_ledger(bad))
bad=copy.deepcopy(d);bad['entries'][0]['identity']='INVENTED_SOURCE_ID'
def invented():
 reader.validate_ledger(bad);assert {r['identity'] for r in bad['entries']}=={r['identity'] for r in d['entries']},'Source identity universe changed'
rejected('invented canonical source identity',invented)
f=json.loads((HERE/'compatibility-combined-v7.json').read_bytes());p=json.loads((PREVIOUS/'compatibility-combined-v3.json').read_bytes());bad=copy.deepcopy(f);next(r for r in bad['results'] if r['corpus']=='A')['corpus']='PRE_EDIT_FROZEN_A'
rejected('late-reference provenance relabel',lambda:reader.validate_frozen(bad,p))
bad=copy.deepcopy(f);bad['results'][0]['inputs']['sha256']='0'*64
rejected('frozen typed-input tamper',lambda:reader.validate_frozen(bad,p))
for name in ['../escape','/absolute','C:/drive','safe/../escape','safe\\escape']:
 rejected('unsafe path '+name,lambda name=name:safe_path(name))
entry=dict(path='one.txt',bytes=1,sha256='1'*64,crc32='12345678')
doc=dict(scope='GIT_FREE_REVIEW_DERIVATIVE;NOT_A_LIVE_CHECKOUT',files=[entry],private_raw_included=False)
validate_manifest(doc)
bad=copy.deepcopy(doc);bad['files'].append(dict(entry,path='ONE.TXT'));rejected('casefold path collision',lambda:validate_manifest(bad))
bad=copy.deepcopy(doc);bad['files'][0]['path']='evidence/04A38/completion/w2/continuation-v9/python-first-failures-v1.json';rejected('excluded giant payload injection',lambda:validate_manifest(bad))
work=PRIVATE/'private-adverse-controls';work.mkdir(exist_ok=False);original=work/'original.txt';modified=work/'modified.txt';original.write_bytes(b'protected');modified.write_bytes(b'tampered')
expected=pin(original);expected['path']=modified.relative_to(ROOT).as_posix();rejected('actual synthetic private-file byte tamper',lambda:validate_file(expected,file_identity(modified)))
save('adverse-reader-controls-v1.json',dict(status='ALL_ADVERSE_CONTROLS_REJECTED',controls=controls,real_historical_mutations=0,original_queries=0,test_or_scanner_campaigns=0,synthetic_private_original=pin(original),synthetic_private_modified=pin(modified)))
print('Adverse controls PASS',len(controls),'no historical/source mutation')
