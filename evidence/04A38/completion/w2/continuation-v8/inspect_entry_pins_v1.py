"""Diagnose entry pin-record shape without changing source or prior evidence."""
from common import *
refs=[]
def walk(x):
    if isinstance(x,dict):
        if all(k in x for k in ['path','bytes','sha256']) and isinstance(x['path'],str) and x['sha256'] is not None:refs.append(x)
        for v in x.values():walk(v)
    elif isinstance(x,list):
        for v in x:walk(v)
for name in ['new-closures-v1.json','whole-body-contracts-v1.json','proof-authority-crosswalk-v1.json','original-fragment-parent-crosswalk-v1.json','current-ownership-v1.json','historical-ownership-v1.json','source-transitions-final-v1.json','readiness-v1.json','review-seal-v1.json','independent-controls-v1.json','command-receipts-v1.json']:
    walk(json.loads((PREVIOUS/name).read_bytes()))
bad=[];extended=[]
for row in refs:
    actual=pin(ROOT/row['path'])
    if any(actual[k]!=row[k] for k in actual):bad.append(dict(expected=row,actual=actual))
    if actual!=row:extended.append(dict(path=row['path'],keys=list(row)))
value=dict(pins=len(refs),bad=bad,extended_rows=extended,reader=pin(HERE/'inspect_entry_pins_v1.py'))
save('entry-pin-shape-diagnostic-v1.json',value)
print(json.dumps(dict(pins=len(refs),bad=bad,extended_rows=extended[:4])))
assert not bad,'Genuine protected pin mismatch; stop for owner decision'
