from common45 import *
lock();provenance=[]
def adapted(name,source,edits):
 text=(ROOT/source).read_text(encoding='utf-8-sig')
 for a,b in edits:
  assert a in text,(name,a);text=text.replace(a,b)
 target=HERE/name
 with target.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
 provenance.append(dict(source=pin(source),driver=pin(target),substitutions=edits))
adapted('reconcile-managed-v1.py','evidence/04A44/reconcile-managed-v1.py', [('common44','common45'),('evidence/04A43/build-and-tests/Debug-identities-v1.json','evidence/04A44/build-and-tests/Debug-identities-v1.json'),('ADDED44','ADDED45'),("['InfrastructurePolicy44Tests','InfrastructurePolicy44AdapterTests']","['WindowsHarness45Tests','WindowsHarness45FormTests']")])
adapted('compiled-and-fixtures-v1.py','evidence/04A44/compiled-and-fixtures-v1.py',[('common44','common45'),("label='a44-wave-b-v3' if config=='Debug' else 'a44-final-v1'","label='a45-final-v1'"),('evidence/04A43/abi-cil/','evidence/04A44/abi-cil/'),("PRIVATE/'fixtures-before-v1/", "ROOT/'.test-rationalization-04A44.tmp/fixtures-before-v1/"),("assert not unexpected,'UNEXPECTED_COMPILED_METHOD_CHANGE'","assert not changes,'UNEXPECTED_PRODUCTION_COMPILED_METHOD_CHANGE'")])
adapted('compatibility-v1.py','evidence/04A44/compatibility-v1.py',[('common44','common45'),("stage='a44-'","stage='a45-'")])
adapted('cumulative-v1.py','evidence/04A44/cumulative43-44-v2.py',[('common44','common45'),("PRIVATE/'fixtures-before-v1/", "ROOT/'.test-rationalization-04A44.tmp/fixtures-before-v1/")])
adapted('progress-stub-v1.py','evidence/04A44/progress-stub-v1.py',[('common44','common45'),("next(r for r in load(HERE/'entry/source-preimages-v1.json')['files'] if r['path']==path)","pin(PRIVATE/'source-preimages'/path)")])
adapted('serial-checks-v1.py','evidence/04A44/serial-checks-v1.py',[('common44','common45'),('a44-final-serial-v1','a45-final-serial-v1')])
save('matrix/verification-driver-provenance-v1.json',dict(rows=provenance,original_test_sources_modified=False,original_queries=0,scope='Versioned current-build paths and accepted44 baseline;all original assertions retained'))
