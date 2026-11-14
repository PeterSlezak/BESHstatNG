"""Explicit current synthetic control phase; never an original/pre-edit capture."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v11'))
from common import *
import ast

proof=json.loads((HERE/'parallel-scanner-base-equivalence-v1.json').read_bytes())
generator=ROOT/proof['proof']['path'];assert pin(generator)==proof['proof']
tree=ast.parse(generator.read_text(encoding='utf-8'))
assignment=next(n for n in tree.body if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='texts' for t in n.targets))
assert isinstance(assignment.value,ast.List) and isinstance(assignment.value.elts[0],ast.Constant)
recovered=assignment.value.elts[0].value.encode('utf-8')
work=PRIVATE/'parallel-scan-proof-v3';observed=[]
for path in sorted((work/'execution').glob('*/input.json')):
    for row in json.loads(path.read_bytes())['rows']:
        if row['index']==0:observed.append((path,{k:row[k] for k in ['path','bytes','sha256']}))
assert len(observed)==1
before_path,before=observed[0]
assert (len(recovered),sha(recovered))==(before['bytes'],before['sha256'])
after_path=work/'invalidated-execution/1/input.json';afterrow=json.loads(after_path.read_bytes())['rows'][0]
after={k:afterrow[k] for k in ['path','bytes','sha256']}
assert before['path']==after['path'] and before['sha256']!=after['sha256'] and pin(ROOT/after['path'])==after
snapshot=PRIVATE/'synthetic-control-preimages-v1/base-fixture0-recovered-v1.txt';snapshot.parent.mkdir(exist_ok=False)
with snapshot.open('xb') as stream:stream.write(recovered)
phase=dict(before=before,after=after,recovered_before=pin(snapshot),generator=pin(generator),
    observed_before=pin(before_path),observed_after=pin(after_path),proof=pin(HERE/'parallel-scanner-base-equivalence-v1.json'),
    classification='DELIBERATE_CURRENT_SYNTHETIC_CONTROL_PRESTATE_RECOVERED_FROM_GENERATOR_AND_OBSERVED_HASH_NOT_PRE_EDIT',
    limitation='The fixture was deliberately changed by the stale-cache rejection control. Its before bytes were not physically copied at that instant. This exact17-byte reconstruction is honestly synthetic recovery, not an original statistical/source preimage or a new oracle capture. Initial and changed input plans/results/streams remain unchanged.')
phasepath=W2/'continuation-v11-private-proof-transition-v1.json'
with phasepath.open('x',encoding='utf-8',newline='\n') as stream:json.dump(dict(rows=[phase],original_queries=0),stream,ensure_ascii=False,indent=2);stream.write('\n')
source=(W2/'continuation-v11-retained-private-reader-v1.py').read_text(encoding='utf-8')
source=source.replace("TARGET=W2/'continuation-v11-retained-private-preservation-v1.json'", "TARGET=W2/'continuation-v11-retained-private-preservation-v2.json'")
helper='''
def validate_current_phase():
    path=W2/'continuation-v11-private-proof-transition-v1.json'
    doc=json.loads(path.read_bytes())
    assert len(doc['rows'])==1 and doc['original_queries']==0
    for row in doc['rows']:
        for key in ['after','recovered_before','generator','observed_before','observed_after','proof']:check(row[key])
        assert (row['recovered_before']['bytes'],row['recovered_before']['sha256'])==(row['before']['bytes'],row['before']['sha256'])
        observed=next(r for r in json.loads((ROOT/row['observed_before']['path']).read_bytes())['rows'] if r['index']==0)
        assert {k:observed[k] for k in ['path','bytes','sha256']}==row['before']
        changed=json.loads((ROOT/row['observed_after']['path']).read_bytes())['rows'][0]
        assert {k:changed[k] for k in ['path','bytes','sha256']}==row['after']
        assert row['classification']=='DELIBERATE_CURRENT_SYNTHETIC_CONTROL_PRESTATE_RECOVERED_FROM_GENERATOR_AND_OBSERVED_HASH_NOT_PRE_EDIT'
    return pin(path)

'''
assert 'def generate():' in source;source=source.replace('def generate():',helper+'def generate():')
source=source.replace("priorpath,prior=previous();rows=", "phase=validate_current_phase();priorpath,prior=previous();rows=")
source=source.replace("previous=pin(priorpath),prior_reader=prior['reader'],", "previous=pin(priorpath),prior_reader=prior['reader'],current_synthetic_phase=phase,common_source=pin(HERE/'common.py'),")
source=source.replace("check(doc['parts_index']);check(doc['previous']);check(doc['prior_reader'])", "check(doc['parts_index']);check(doc['previous']);check(doc['prior_reader']);check(doc['common_source']);assert validate_current_phase()==doc['current_synthetic_phase']")
reader=W2/'continuation-v11-retained-private-reader-v2.py'
with reader.open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
source=(HERE/'finalize_live_v2.py').read_text(encoding='utf-8')
source=source.replace('continuation-v11-retained-private-preservation-v1.json','continuation-v11-retained-private-preservation-v2.json').replace('continuation-v11-retained-private-reader-v1.py','continuation-v11-retained-private-reader-v2.py')
source=source.replace("authoritative_private_reader=pin(reader),authoritative_private_preservation=pin(privatepath),", "authoritative_private_reader=pin(reader),authoritative_private_preservation=pin(privatepath),current_private_phase=pin(W2/'continuation-v11-private-proof-transition-v1.json'),")
point="'continuation-v11-live-preservation-v1.json independently verifies"
replacement="'One current synthetic stale-cache fixture was deliberately mutated. Its exact17-byte before state is recovered from its authenticated generator and independently observed before-hash, honestly labelled SYNTHETIC_RECOVERY rather than pre-edit or original statistical capture. The before/after plans, current bytes and recovered bytes remain; continuation-v11-private-proof-transition-v1.json and the authoritative private reader-v2 validate this phase. The earlier private reader-v1 is retained as a draft source and has no claimed result.','',\n        "+point
assert point in source;source=source.replace(point,replacement)
bootstrap="import sys\nfrom pathlib import Path\nsys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v11'))\n"
tool=W2/'continuation-v11-finalize-live-v3.py'
with tool.open('x',encoding='utf-8',newline='\n') as stream:stream.write(bootstrap+source)
path=W2/'continuation-v11-private-successor-source-crosswalk-v2.json'
doc=dict(reader_before=pin(W2/'continuation-v11-retained-private-reader-v1.py'),reader_after=pin(reader),
    finalizer_before=pin(HERE/'finalize_live_v2.py'),finalizer_after=pin(tool),phase=pin(phasepath),
    scope='Additive post-publication private authority. Exactly one current deliberate synthetic mutation is made explicit with honest recovered prestate; no protected/production/original source is rewritten or captured anew. Prior v10 reader-v2/2545 pins remain exact.',original_queries=0)
with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
print(json.dumps(dict(reader=pin(reader),phase=pin(phasepath),finalizer=pin(tool))),flush=True)
