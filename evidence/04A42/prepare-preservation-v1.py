from common42 import *
s=(ROOT/'evidence/04A41/preservation-v1.py').read_text(encoding='utf-8-sig').replace('from common41','from common42').replace('matrix/source-freeze-v1.json','matrix/source-freeze-v2.json').replace('len(tracked)==14406','len(tracked)==14493')
s=s.replace("else PRIVATE/'entry-pins'/r['path']","else ROOT/freeze['plan_snapshot']['path']")
s=s.replace("transitions={r['before']['path']:r for r in historical['authorized_source_transitions']};assert len(transitions)==16","transitions={r['before']['path']:r for r in historical['authorized_source_transitions']};assert len(transitions)==16\naccepted41={r['before']['path']:r for r in load(ROOT/'evidence/04A41/matrix/source-freeze-v1.json')['source_delta']}")
s=s.replace("expected=transitions[r['path']]['after'] if r['path'] in transitions else r","expected=accepted41[r['path']]['after'] if r['path'] in accepted41 else transitions[r['path']]['after'] if r['path'] in transitions else r")
s=s.replace("load(HERE/'entry/authentication-v1.json')['newline_only_owner_checkout_transitions']","load(ROOT/'evidence/04A41/entry/authentication-v1.json')['newline_only_owner_checkout_transitions']")
s=s.replace('new41_runtime_source_transitions=10,new41_plan_transition=1,new41_tests=2','accepted41_runtime_source_transitions=10,new42_runtime_source_transitions=8,new42_plan_transition=1,new42_tests=2')
s=s.replace("save('preservation/readback-v1.json',dict(","save('preservation/readback-v1.json',dict(raw41_inventory=pin(ROOT/'.test-rationalization-04A41.tmp/new-raw-inventory-v1.json'),raw41_prior_pins_exact=6810,current41_raw_recount=pin(PRIVATE/'inherited41-current-raw-inventory-v1.json'),accepted41_delivery=pin(ROOT/'.test-rationalization-04A41.tmp/publication/external-delivery-receipt-v1.json'),accepted41_sealed_newline_recoveries=len(load(HERE/'entry/authentication-v1.json')['merged41_newline_transitions']),")
s=s.replace("print('Preservation PASS:14406 entry,468 checkpoint,8984 historical,1906 newer raw,1353 history;ID continuity NOT_VERIFIED',flush=True)","print('Preservation PASS:14493 tracked entry,468 checkpoint,8984 historical,1906 raw40,6810 raw41 prior pins plus complete41 recount;ID continuity NOT_VERIFIED',flush=True)")
insert=s.index("register=load(")
s=s[:insert]+'''for r in load(ROOT/'.test-rationalization-04A41.tmp/new-raw-inventory-v1.json')['files']:exact(r)
old41_files=[pin(p) for p in sorted((ROOT/'.test-rationalization-04A41.tmp').rglob('*')) if p.is_file()]
save(PRIVATE/'inherited41-current-raw-inventory-v1.json',dict(files=old41_files,count=len(old41_files),prior_inventory_exact=6810,scope='Fresh enumeration of41 store,not a claim all later files had an original pre42 ledger;original6810 and accepted41 final receipt separately authenticated. No physical identity continuity claim.'))
for r in load(HERE/'entry/authentication-v1.json')['merged41_newline_transitions']:
    exact(r['current']);exact(r['protected_exact'])
'''+s[insert:]
with (HERE/'preservation-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Exact tracked/historical/raw/source successor preservation recipe prepared',flush=True)
