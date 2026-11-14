from common import *
from publication_gate_v2 import *
selection=load(PRIVATE/'publication-selection-v1.json');stage=ROOT/selection['stage']
names=physical_inventory(stage,selection['files'],selection['excluded'])
findings,reviewed,details,encodings=scan(stage,names,load(HERE/'entry/source-lock.json')['files'])
save(PRIVATE/'stage-scan-probe-v2.json',dict(findings=findings,details=details,reviewed=reviewed,encodings=encodings,selection=pin(PRIVATE/'publication-selection-v1.json')))
print('Stage probe files',len(names),'legacy sources',len(encodings),'finding occurrences',len(findings),flush=True)
for name in sorted({r['path'] for r in findings}):print(name, sorted({r['category'] for r in findings if r['path']==name}),flush=True)
