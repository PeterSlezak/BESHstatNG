from common47 import *
import runpy
lock();runpy.run_path(str(HERE/'preservation-base-v1.py'),run_name='__main__');base=load(HERE/'preservation/readback-base-v1.json');populations=[]
def pins(x):
 if isinstance(x,dict):
  if all(k in x for k in ['path','bytes','sha256']):yield {k:x[k] for k in ['path','bytes','sha256']}
  for v in x.values():yield from pins(v)
 elif isinstance(x,list):
  for v in x:yield from pins(v)
for batch,publication in [('45','publication'),('46','publication-v1')]:
 old=ROOT/('.test-rationalization-04A'+batch+'.tmp');files=[pin(p) for p in sorted(old.rglob('*')) if p.is_file()];inventory=PRIVATE/('accepted'+batch+'-whole-store-readback-v1.json');save(inventory,dict(files=files))
 receipt=load(old/publication/'external-delivery-receipt-v1.json');exact(receipt['archive']);checked={}
 for report in [receipt,load(ROOT/('evidence/04A'+batch+'/matrix/actual-commands-review-v1.json')),load(ROOT/('evidence/04A'+batch+'/build-and-tests/Debug-identities-v1.json')),load(ROOT/('evidence/04A'+batch+'/build-and-tests/Release-identities-v1.json'))]:
  for r in pins(report):
   if r['path'].startswith('.test-rationalization-04A'+batch+'.tmp/'):exact(r);checked[r['path']]=r
 populations.append(dict(batch=batch,fresh_current_whole_store_files=len(files),prior_individually_sealed_pins_exact=len(checked),current_inventory=pin(inventory),receipt=pin(old/publication/'external-delivery-receipt-v1.json')))
 print('Accepted'+batch,len(files),'current protected files;',len(checked),'individually sealed pins exact',flush=True)
save('preservation/readback-v1.json',dict(base,accepted45_46_populations=populations,new47_test_files=6,new47_production_changes=0,historical_W3_union='INHERITED_QUALIFIED;NOT_FRESH_FULL_UNION',continuous_physical_identity='NOT_VERIFIED',accepted41_Release_FAIL='RETAINED_UNWAIVED',gates=GATES))
