from common import *
from publication_gate import *
selection=load(PRIVATE/'publication-selection-v1.json')
stage=ROOT/selection['stage']
names=physical_inventory(stage,selection['files'],selection['excluded'])
findings,reviewed,details=scan(stage,names)
save(PRIVATE/'stage-scan-probe-v1.json',dict(findings=findings,details=details,reviewed=reviewed,selection=pin(PRIVATE/'publication-selection-v1.json')))
print('Physical stage probe: files',len(names),'findings',len(findings),'files with findings',len({r['path'] for r in findings}),flush=True)
for name in sorted({r['path'] for r in findings}):
    print(name, sorted({r['category'] for r in findings if r['path']==name}),flush=True)
