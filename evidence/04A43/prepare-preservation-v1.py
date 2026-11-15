from common43 import *
s=(ROOT/'evidence/04A42/preservation-v2.py').read_text(encoding='utf-8-sig').replace('from common42','from common43').replace('matrix/source-freeze-v2.json','matrix/source-freeze-v1.json').replace('len(tracked)==14493','len(tracked)==14634')
a=s.index("old_history=load(");b=s.index("for r in load(ROOT/'evidence/04A41/entry/authentication-v1.json')",a)
s=s[:a]+'''old_history=load(ROOT/'evidence/04A40/entry/source-preimages-v3.json')['files'];assert len(old_history)==1353
transitions={r['before']['path']:r for r in historical['authorized_source_transitions']};assert len(transitions)==16
initial={r['path']:r for r in tracked}
freeze41=load(ROOT/'evidence/04A41/matrix/source-freeze-v1.json');freeze42=load(ROOT/'evidence/04A42/matrix/source-freeze-v2.json')
after41={r['before']['path']:r['after'] for r in freeze41['source_delta']};after41[freeze41['plan']['path']]=freeze41['plan']
after42={r['before']['path']:r['after'] for r in freeze42['source_delta']};after42[freeze42['plan']['path']]=freeze42['plan']
owner41={r['sealed']['path']:r for r in load(ROOT/'evidence/04A42/entry/authentication-v1.json')['merged41_newline_transitions']}
owner42={r['sealed']['path']:r for r in load(HERE/'entry/authentication-v1.json')['merged42_newline_transitions']}
for r in old_history:
    p=r['path'];expected=transitions[p]['after'] if p in transitions else r
    if p in after41:expected=after41[p]
    if p in owner41:
        t=owner41[p];assert expected==t['sealed'];exact(t['protected_exact']);expected=t['current']
    if p in after42:expected=after42[p]
    if p in owner42:
        t=owner42[p];assert expected==t['sealed'];exact(t['protected_exact']);expected=t['current']
    assert initial[p]==expected,'UNAUTHENTICATED_SOURCE_HISTORY_ENTRY'
    if p in transitions:exact(transitions[p]['preserved_preimage'])
'''+s[b:]
a=s.index("for r in load(HERE/'entry/authentication-v1.json')");b=s.index('register=load(',a)
s=s[:a]+'''for r in load(ROOT/'evidence/04A42/entry/authentication-v1.json')['merged41_newline_transitions']:
    assert initial[r['current']['path']]==r['current'];exact(r['protected_exact'])
for r in load(HERE/'entry/authentication-v1.json')['merged42_newline_transitions']:
    assert initial[r['current']['path']]==r['current'];exact(r['protected_exact'])
raw42=load(ROOT/'.test-rationalization-04A42.tmp/new-raw-inventory-v1.json')
for r in raw42['files']:exact(r)
old42_files=[pin(p) for p in sorted((ROOT/'.test-rationalization-04A42.tmp').rglob('*')) if p.is_file()]
save(PRIVATE/'inherited42-current-raw-inventory-v1.json',dict(files=old42_files,count=len(old42_files),prior_pins_exact=len(raw42['files']),scope='Fresh42-store enumeration,not a claim all later additions had an original pre43 ledger. Prior11457 and accepted42 final/postdelivery receipts independently authenticated.'))
'''+s[b:]
s=s.replace('accepted41_runtime_source_transitions=10,new42_runtime_source_transitions=8,new42_plan_transition=1,new42_tests=2','accepted41_runtime_source_transitions=10,accepted42_runtime_source_transitions=8,new43_existing_runtime_transitions=3,new43_TestProject_Compile_registration=1,new43_Common_source_files=1,new43_tests=2,new43_plan_transition=1')
s=s.replace("save('preservation/readback-v1.json',dict(","save('preservation/readback-v1.json',dict(raw42_prior_inventory=pin(ROOT/'.test-rationalization-04A42.tmp/new-raw-inventory-v1.json'),raw42_prior_pins_exact=len(raw42['files']),current42_raw_recount=pin(PRIVATE/'inherited42-current-raw-inventory-v1.json'),current42_raw_count=len(old42_files),current41_raw_count=len(old41_files),sealed42_newline_recoveries=len(owner42),")
s=s.replace('len(load(HERE/\'entry/authentication-v1.json\')[\'merged41_newline_transitions\'])','len(owner41)')
s=s.replace("accepted41_delivery=pin(ROOT/'.test-rationalization-04A41.tmp/publication/external-delivery-receipt-v1.json')","accepted42_delivery=pin(ROOT/'.test-rationalization-04A42.tmp/publication/external-delivery-receipt-v1.json')")
s=s.replace("print('Preservation PASS:14493 tracked entry,468 checkpoint,8984 historical,1906 raw40,6810 raw41 prior pins plus complete41 recount;ID continuity NOT_VERIFIED',flush=True)","print('Preservation PASS:14634 tracked entry,468 checkpoint,8984 protected,1906 raw40,6810 raw41,11457 raw42 and complete41/42 recount;ID continuity NOT_VERIFIED',flush=True)")
with (HERE/'preservation-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Exact40/41/42/source/raw lineage plus43 permitted transitions wired into additive preservation verifier',flush=True)
