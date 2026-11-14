"""Source-backed next work, including all OPEN identities, not paper closure."""
from collections import Counter
from common import *
h=json.loads((HERE/'historical-ownership.json').read_bytes());n=json.loads((HERE/'current-ownership.json').read_bytes())
open_h=[r for r in h['members'] if not r['proof']]
rows=[]
for r in open_h:
 rows.append(dict(original_id=r['canonical_original_id'],path=r['path'],name=r['name'],next_proof='Whole original/current source/branches/dependencies/context plus real compiled caller; no new original query authorized'))
save('remaining-work-queue.json',dict(status='NOT_READY_04A38',historical_OPEN=len(rows),members=rows,first_action='UdfLinearAlgebra.TryInvertMatrix: establish original ProjectData/Err versus Core embedded runtime exception-state contract from source and current-only probes; leave OPEN if not proved. Then ParametricUDFs.FormatDescriptiveTable and adjacent report preparation with authentic source-operation/IL/caller proof.',no_generic_owner_approval_needed=True,new_reference_authority=False))
save('per-family-ownership.json',dict(historical=[dict(path=p,total=sum(r['path']==p for r in h['members']),closed=sum(r['path']==p and bool(r['proof']) for r in h['members']),OPEN=sum(r['path']==p for r in open_h)) for p in sorted({r['path'] for r in h['members']})],current=[dict(path=p,total=sum(r['path']==p for r in n['members']),closed=sum(r['path']==p and not r['review_status'].startswith('OPEN') for r in n['members']),OPEN=sum(r['path']==p and r['review_status'].startswith('OPEN') for r in n['members'])) for p in sorted({r['path'] for r in n['members']})],unsupported_container_closure=False))
print('First report candidates',[(r['name'],r['canonical_original_id']) for r in open_h if r['path'].endswith('ParametricUDFs.vb')][:8])
