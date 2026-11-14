"""Last additive pin union after all command streams have completed."""
from common import *
import importlib.util

base=load(HERE/'preservation/w3-new-pins-v1.json')
known={r['path']:{k:r[k] for k in ['path','bytes','sha256']} for r in base['new_pins']}
roots=[HERE,PRIVATE,RUNROOT/'w3pyv1',RUNROOT/'builds/w3v1',ROOT/'.test-rationalization-w3x.tmp']
for group in ['commands','executions']:
    for p in (RUNROOT/group).iterdir():
        if p.is_dir() and (p.name.startswith(('w3v1-','w3pyv1-')) if group=='commands' else p.name.startswith(('w3D-','w3R-'))):roots.append(p)
paths=set()
for root in roots:
    for p in root.rglob('*'):
        if p.is_file():
            assert not p.is_symlink() and p.resolve().is_relative_to(root.resolve())
            paths.add(p)
pins=[];new=[]
for p in sorted(paths):
    row=pin(p)
    if row['path'] in known:assert row==known[row['path']],'AUDITED_NEW_W3_PIN_CHANGED'
    else:new.append(row)
    pins.append(row)
assert len(pins)==len({r['path'].casefold() for r in pins})
archives=[r for r in pins if r['path'].lower().endswith(('.zip','.zip.001','.zip.002'))]
assert all(any(Path(r['path']).name==name for r in archives) for name in ['BESHstatNG_04A38_W3_ACTIVE_v1.zip','BESHstatNG_04A38_W3_POSTSEAL_v1.zip'])
save('preservation/final-additive-pins-v1.json',dict(base=pin(HERE/'preservation/w3-new-pins-v1.json'),
    post_audit_additions=new,additive_count=len(new),W3_pin_union=len(pins),
    total_private_pin_union=31910+len(pins),old_physical_archive_records=152,new_physical_archive_records=len(archives),
    final_physical_archive_union=152+len(archives),new_archives=archives,
    manifest_self='Final additive inventory and delivery receipt excluded from their own nonrecursive pin union; their exact hashes are final authority anchors, not invented recursive self-pins.',
    audit_scope='Original31910 full actual audit plus complete named W3 execution/output pins; post-audit additions independently hash-read back.'))
readiness=load(HERE/'final/readiness.json')
save('final/delivery-receipt-v1.json',dict(status=readiness['status'],
    owner_review=pin(HERE/'final/owner-review.md'),readiness=pin(HERE/'final/readiness.json'),
    next_gate=pin(HERE/'final/next-gate-handover.md'),common_logical_inventory=pin(HERE/'publication/final-logical-inventory-v1.json'),
    main_parts=pin(HERE/'publication/parts-index-v1.json'),postseal=pin(HERE/'publication/postseal-physical-readback-v1.json'),
    final_Git=pin(HERE/'final/git-after-postseal-v2.json'),source_lock=pin(HERE/'final/postseal-source-lock-v1.json'),
    final_private_union=pin(HERE/'preservation/final-additive-pins-v1.json'),
    private_file_union=31910+len(pins),physical_archive_union=152+len(archives),review_transport_parts=13,
    external_upload='NOT_PERFORMED',terminal=readiness['terminal']))
print('Final delivery',readiness['status'],'file union',31910+len(pins),'archive records',152+len(archives),'review parts13',flush=True)
print('Delivery SHA256',pin(HERE/'final/delivery-receipt-v1.json')['sha256'],flush=True)
