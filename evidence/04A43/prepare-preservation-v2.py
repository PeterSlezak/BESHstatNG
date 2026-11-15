from common43 import *
s=(HERE/'preservation-v1.py').read_text(encoding='utf-8-sig');a=s.index('old_history=');b=s.index('register=load(',a)
block='''old_history=load(ROOT/'evidence/04A40/entry/source-preimages-v3.json')['files'];assert len(old_history)==1353
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
for r in load(ROOT/'evidence/04A41/entry/authentication-v1.json')['newline_only_owner_checkout_transitions']:
    exact(r['current_owner_checkout']);exact(r['exact_original_copy'])
for r in historical['archives']:exact(r)
for r in load(ROOT/'.test-rationalization-04A41.tmp/new-raw-inventory-v1.json')['files']:exact(r)
old41_files=[pin(p) for p in sorted((ROOT/'.test-rationalization-04A41.tmp').rglob('*')) if p.is_file()]
save(PRIVATE/'inherited41-current-raw-inventory-v1.json',dict(files=old41_files,count=len(old41_files),prior_inventory_exact=6810,scope='Fresh41 store recount;prior6810 and later accepted receipt separately pinned;no physical continuity claim.'))
for table in [owner41,owner42]:
    for r in table.values():assert initial[r['current']['path']]==r['current'];exact(r['protected_exact'])
raw42=load(ROOT/'.test-rationalization-04A42.tmp/new-raw-inventory-v1.json')
for r in raw42['files']:exact(r)
old42_files=[pin(p) for p in sorted((ROOT/'.test-rationalization-04A42.tmp').rglob('*')) if p.is_file()]
save(PRIVATE/'inherited42-current-raw-inventory-v1.json',dict(files=old42_files,count=len(old42_files),prior_pins_exact=len(raw42['files']),scope='Fresh42 store recount,not a claim later additions had original pre43 ledger. Prior11457 and accepted42 final/postdelivery receipts separately authenticated.'))
'''
s=s[:a]+block+s[b:];compile(s,'preservation-v2.py','exec')
with (HERE/'preservation-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Additive preservation verifier fixed at generated block boundary;first syntax failure preserved without executing or altering protected files',flush=True)
