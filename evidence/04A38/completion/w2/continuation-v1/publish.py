"""Versioned manifest; never regenerate a predecessor or claim full readiness."""
from common import *
def manifest(version,readiness):
 name='publication-manifest-'+version+'.json';inventory='file-inventory-'+version+'.json';self_path=(HERE/name).relative_to(ROOT).as_posix();inventory_path=(HERE/inventory).relative_to(ROOT).as_posix()
 old=json.loads((W2/'publication-manifest-closeout-v2.json').read_bytes());paths={r['path'] for r in old['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts)
 for row in json.loads((HERE/'source-transitions-p1-v5.json').read_bytes())['files']:paths.add(row['after']['path'])
 for row in json.loads((HERE/'source-transitions-p0-v2.json').read_bytes())['files']:paths.add(row['after']['path'])
 for field in ['Core_formatter','existing_Core_mask']:paths.add(json.loads((HERE/'source-transitions-p0-v2.json').read_bytes())[field]['path'])
 paths.update([self_path,inventory_path]);paths=sorted(paths);assert len(paths)==len({p.casefold() for p in paths})
 save(inventory,dict(paths=paths,scope='COMPLETE_INTENDED_CONTINUATION_PUBLICATION_PLUS_PROTECTED_PREDECESSORS;DERIVATIVE_NOT_ALL_TRACKED_PATHS',omissions=['Live Git/index','Owner documents not in predecessor publication','Private loaded binaries/package cache/raw machine logs','Ignored historical TestResults','Other tracked/older help/evidence outside this manifest'],not_proven_from_copy='4695 live paths/16 owner docs/ignored history remain local verified receipts; omitted payloads cannot be reverified from this derivative'))
 rows=[dict(path=p,bytes=None,sha256=None) if p==self_path else pin(ROOT/p) for p in paths]
 save(name,dict(status='NOT_READY_04A38',inventory=inventory,readiness=readiness,entries=rows,nonrecursive_self='Exactly this one null entry; older manifest files are ordinary hashed payloads',version=version,source_transition_reader='verify_preservation_v4.py',scope='Compatibility/migration checkpoint, not scientific/native/owner acceptance'))
 print(name,len(rows)-1,'hashed payloads plus one null self')
if __name__=='__main__':
 version=sys.argv[1];ready=sys.argv[2]
 if version=='v1':
  summary=json.loads((HERE/'ownership-summary.json').read_bytes())
  save('readiness.json',dict(status='NOT_READY_04A38',W1_G_GATE='APPROVAL_G_COMBINED_GATE_PASS_PREVIOUSLY_COMPLETED',W2='IN_PROGRESS',W3='NOT_STARTED',historical_proof_supported=summary['historical']['closed'],historical_OPEN=summary['historical']['OPEN'],current=summary['current'],support=summary['support'],fragments=summary['fragments'],owner_decision_needed=False,new_original_queries=0,test_state='Fresh full Debug20626PASS/14 inheritedFAIL; selectedRelease119PASS; final Python source audit still running when this v1 manifest is sealed. Later closeout must disclose its actual raw outcomes.',remaining='644 historical bodies/654 current bodies and303 support declarations; full statement/inline ownership, remaining real preparation/report consumers and W3 acceptance proof',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN')))
 manifest(version,ready)
