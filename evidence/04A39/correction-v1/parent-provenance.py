from common import *
from diagnostic_producer import sanitized
import ast

old=load(PRIOR/'python-node-scanners/python-identities-v1.json')
w3=load(ROOT/'evidence/04A38/completion/w3/python-node-scanners/python-identities-v1.json')
ids=[r['id'] for r in load(PRIOR/'python-node-scanners/python-delta-v2.json')['changes'] if r['before']=='ERROR']
inventory=load(PRIOR/'python-node-scanners/campaign-v2/python-discovery-inventory-v1.json')['entries']
receipts=[]
for path in (ROOT/'.test-rationalization-04A39.tmp/commands').glob('*python-worker-v1-1-main/command.json'):
    receipts.append(dict(receipt=pin(path),raw=load(path)))
assert len(receipts)==1
save(PRIVATE/'privacy-parent-command-provenance.json',dict(command_receipts=receipts,focused_command=dict(argv=[sys.executable,'-I','-B','-c',"import sys,runpy;sys.path.insert(0,'evidence/04A39/correction-v1');runpy.run_path('evidence/04A39/correction-v1/focused-worker.py',run_name='__main__')"],exit_code=load(PRIVATE/'focused-raw.json')['exit_code'],raw_output=pin(PRIVATE/'focused-raw.json'),authority='Actual command executed in this correction tool transcript; additive retrospective command crosswalk, not an invented earlier receipt.')))
rows=[]
for identity in ids:
    row=next(r for r in inventory if r['id']==identity)
    before=next(r for r in w3['parents'] if r['id']==identity);after=next(r for r in old['parents'] if r['id']==identity)
    module=ROOT/row['module_path'];tree=ast.parse(module.read_text(encoding='utf-8-sig'))
    method=next(n for n in ast.walk(tree) if isinstance(n,ast.FunctionDef) and n.name==identity.rsplit('.',1)[1])
    source='\n'.join(module.read_text(encoding='utf-8-sig').splitlines()[method.lineno-1:method.end_lineno])
    rows.append(dict(identity=identity,test_source=pin(module),line=method.lineno,method_source=source,
        W3_outcome=before['outcome'],original39_outcome=after['outcome'],focused_outcome='FAIL',original_findings=1599,focused_findings=1941,
        scanner=pin('scripts/verify-04a13-publication.py' if '04a13' in identity else 'scripts/verify-04a14-publication.py'),
        W3_raw=next(p['raw'] for p in w3['executions'] if p['lane']==1),original39_raw=next(p['raw'] for p in old['parts'] if p['lane']==1),
        output_paths='Raw original and focused result/journal/command paths retained in protected command provenance; public fields contain pins and sanitized arguments only.',
        waiver=False))
save('privacy/parent-provenance.json',sanitized(dict(parents=rows,commands=[r['raw'] for r in receipts],protected_commands=pin(PRIVATE/'privacy-parent-command-provenance.json'),
    rules_changed=False,scope='Git changed/untracked candidate union plus phase manifest/privacy receipt; every parent assertion remains unchanged. Full original39 lane1 and focused26 campaign are distinct executions.')))
print('Both privacy parent identities, exact rules, actual original command and raw authority crosswalk retained',flush=True)
