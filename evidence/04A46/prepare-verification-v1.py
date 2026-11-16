from common46 import *
lock();rows=[]
def adapt(name,source,edits):
 s=(ROOT/source).read_text(encoding='utf-8-sig')
 for a,b in edits:
  assert a in s,(name,a);s=s.replace(a,b)
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
 rows.append(dict(source=pin(source),driver=pin(HERE/name),edits=edits))
adapt('managed-final-v1.py','evidence/04A45/managed-final-v1.py',[('common45','common46'),('a45-','a46-'),("['WindowsHarness45'","['GuiWiring46','AssumptionReport06','PairedBlockAdapter','ContingencyAdapter','WindowsHarness45'")])
adapt('reconcile-managed-v1.py','evidence/04A45/reconcile-managed-v1.py',[('common45','common46'),('evidence/04A44/build-and-tests/Debug-identities-v1.json','evidence/04A45/build-and-tests/Debug-identities-v1.json'),('ADDED45','ADDED46'),("['WindowsHarness45Tests','WindowsHarness45FormTests']","['GuiWiring46StateTests','GuiWiring46ExecutionTests','GuiWiring46PolicyTests']")])
# Existing45 inspector driver stops after42 because compare43 needs its own import path.
# Stop deliberately at that boundary;the disjoint cumulative driver executes43/44 below.
s=(ROOT/'evidence/04A45/compiled-and-fixtures-v1.py').read_text(encoding='utf-8-sig');s=s[:s.index("compare43=module")];s=s.replace('common45','common46').replace("label='a45-final-v1'","label='a46-final-v1'").replace('evidence/04A44/abi-cil/','evidence/04A45/abi-cil/')
with (HERE/'compiled-and-fixtures-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
rows.append(dict(source=pin('evidence/04A45/compiled-and-fixtures-v1.py'),driver=pin(HERE/'compiled-and-fixtures-v1.py'),scope='Same268 exports/normalized CIL and39-42 current fixtures;43/44 handled by separate accepted cumulative driver;no case omitted.'))
for name in ['compatibility-v1.py','cumulative-v1.py','progress-stub-v1.py','serial-checks-v1.py','resource-boundary-v1.py']:
 edits=[('common45','common46')]
 if name=='compatibility-v1.py':edits.append(("stage='a45-'","stage='a46-'"))
 if name=='serial-checks-v1.py':edits.append(('a45-final-serial-v1','a46-final-serial-v1'))
 if name=='resource-boundary-v1.py':edits.extend([('builds/a45-final-v1','builds/a46-final-v1'),("HERE/'fixtures/ResourceBoundary45.cs'","ROOT/'evidence/04A45/fixtures/ResourceBoundary45.cs'"),("pin('evidence/04A45/fixtures/ResourceBoundary45.cs')","pin('evidence/04A45/fixtures/ResourceBoundary45.cs')")])
 adapt(name,'evidence/04A45/'+name,edits)
save('matrix/verification-driver-provenance-v1.json',dict(rows=rows,original_test_sources_modified=False,full_Release='NOT_RUN',Node='INHERITED_916_PASS;NOT_RERUN',native='NOT_RUN'))
