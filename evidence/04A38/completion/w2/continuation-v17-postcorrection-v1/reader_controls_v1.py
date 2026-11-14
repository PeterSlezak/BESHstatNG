from common import *
import copy
from verify_postcorrection_v1 import exact_bytes,canonical
from verify_transport_v1 import safe_path,validate_manifest
load=lambda p:json.loads(Path(p).read_bytes())
controls=[]
def reject(name,fn):
 try:fn()
 except (AssertionError,ValueError,KeyError):controls.append(dict(name=name,status='REJECTED'))
 else:raise AssertionError('Negative control accepted '+name)
row=load(HERE/'production-test-freeze-v1.json')['sources'][0];data=(ROOT/row['path']).read_bytes();exact_bytes(row,data)
reject('missing production source',lambda:exact_bytes(row,None));reject('modified production source',lambda:exact_bytes(row,data+b' '))
d=load(PREVIOUS/'canonical-identity-status-v4.json');canonical(d)
x=copy.deepcopy(d);x['entries'][-1]['supported']=False;reject('unsupported closure',lambda:canonical(x))
x=copy.deepcopy(d);x['entries'][0]['identity']+='-invented';reject('renumbered original identity',lambda:canonical(x))
x=copy.deepcopy(d);x['entries'].pop();reject('missing supported identity',lambda:canonical(x))
for name in ['../escape','/absolute','C:/absolute','folder/../escape','a\\b','.git/config','a/CON.txt','folder/trailing.']:
 reject('unsafe transport '+name,lambda name=name:safe_path(name))
safe=dict(scope='GIT_FREE_REVIEW_DERIVATIVE;NOT_A_LIVE_CHECKOUT',private_raw_included=False,files=[dict(path='a.txt',bytes=1,sha256='0'*64,crc32='0'*8)])
validate_manifest(safe)
x=copy.deepcopy(safe);x['files'].append(copy.deepcopy(x['files'][0]));reject('duplicate logical payload',lambda:validate_manifest(x))
x=copy.deepcopy(safe);x['files'].append(dict(x['files'][0],path='A.txt'));reject('casefold collision',lambda:validate_manifest(x))
private=pin(RUNROOT/'commands/w2c17pc1-entry-git-HEAD/command.json');b=(ROOT/private['path']).read_bytes();exact_bytes(private,b);reject('private raw tamper',lambda:exact_bytes(private,b+b' '))
comp=load(HERE/'compatibility-replacement-v1.json')
rows=[dict(corpus=r['corpus'],cases=r['cases'],provenance='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in list('ABCDEFG') else 'PRE_EDIT_FROZEN') for r in comp['results']]
def provenance(rows):
 assert sum(r['cases'] for r in rows if r['provenance']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT')==458
 for r in rows:assert r['provenance']==('LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in list('ABCDEFG') else 'PRE_EDIT_FROZEN')
provenance(rows);x=copy.deepcopy(rows);next(r for r in x if r['corpus']=='A')['provenance']='PRE_EDIT_FROZEN';reject('late-reference provenance relabelling',lambda:provenance(x))
frag=load(PREVIOUS/'original-current-fragment-transition-matrix-v2.json')
# Fragment tables are immutable byte authorities as well as complete source/CIL proofs.
r=pin(PREVIOUS/'original-current-fragment-transition-matrix-v2.json');b=(ROOT/r['path']).read_bytes();reject('missing fragment obligation',lambda:exact_bytes(r,b[:-1]))
save('adversarial-reader-controls-v1.json',dict(status='POSITIVE_AND_ADVERSE_READBACK_CONTROLS_PASS',controls=controls,repository_files_modified=0,original_queries=0,scope='Real validators with altered in-memory specimens; actual source/private/reference bytes remain unchanged. Exact prior source/CIL/whole-body fragment proofs separately replayed and pinned.'))
print('Adversarial controls',len(controls),'PASS',flush=True)
