from common46 import *
import runpy
lock();runpy.run_path(str(HERE/'preservation-base-v1.py'),run_name='__main__')
base=load(HERE/'preservation/readback-base-v1.json')
old=ROOT/'.test-rationalization-04A45.tmp';files=[pin(p) for p in sorted(old.rglob('*')) if p.is_file()]
save(PRIVATE/'accepted45-whole-store-readback-v1.json',dict(files=files,scope='Fresh full old45 store readback;no preexisting45 whole-store inventory was asserted. Previously sealed artifacts rechecked against accepted45 individual receipt pins below.'))
receipt=load(old/'publication/external-delivery-receipt-v1.json');exact(receipt['archive'])
def pins(x):
 if isinstance(x,dict):
  if all(k in x for k in ['path','bytes','sha256']):yield {k:x[k] for k in ['path','bytes','sha256']}
  for v in x.values():yield from pins(v)
 elif isinstance(x,list):
  for v in x:yield from pins(v)
checked={}
for report in [receipt,load(ROOT/'evidence/04A45/matrix/actual-commands-review-v1.json'),load(ROOT/'evidence/04A45/build-and-tests/Debug-identities-v1.json'),load(ROOT/'evidence/04A45/build-and-tests/Release-identities-v1.json')]:
 for r in pins(report):
  if r['path'].startswith('.test-rationalization-04A45.tmp/'):
   exact(r);checked[r['path']]=r
save('preservation/readback-v1.json',dict(base,accepted45_raw_current_count=len(files),accepted45_prior_individual_receipt_pins_exact=len(checked),accepted45_whole_store=pin(PRIVATE/'accepted45-whole-store-readback-v1.json'),accepted45_receipt=pin(old/'publication/external-delivery-receipt-v1.json'),accepted45_source_freeze=pin('evidence/04A45/matrix/source-freeze-v1.json'),accepted45_legacy_inventory_note='No historical45 full raw population count fabricated;fresh whole-store readback plus individual external/test/command pins. Earlier468/8984/raw40-44 inventories remain exact.',new46_runtime_changes=0,new46_test_files=8,new46_test_Compile_entries=8,accepted41_Release_FAIL='RETAINED_AND_FRESHLY_REPRODUCED_IN_SELECTED46_SET;NOT_NEW_HARNESS_FAIL'))
print('Accepted45 complete raw store readback',len(files),';individually sealed45 raw pins',len(checked),flush=True)
