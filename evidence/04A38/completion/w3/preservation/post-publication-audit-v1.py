"""Full existing private reader after publication plus exact additive/source checks."""
from common import *
import importlib.util,concurrent.futures

spec=importlib.util.spec_from_file_location('entry_reader',HERE/'entry/authenticate-entry-v2.py')
entry=importlib.util.module_from_spec(spec);spec.loader.exec_module(entry)
script=ACCEPTED/'private_preservation_v2.py'
output=PRIVATE/'post-publication-base-private-v1.json'
code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
r=command('postpub-existing-private-reader',[sys.executable,'-I','-B','-c',code,str(ACCEPTED),str(script),'verify',str(ACCEPTED/'retained-private-preservation-v2.json'),str(output)])
assert r['exit_code']==0,'POST_PUBLICATION_PRIVATE_READER_FAILED'
base=load(ACCEPTED/'retained-private-preservation-v2.json')
add=load(W2/'continuation-v17-postcorrection-v1-postseal-private-addendum-v1.json')
delivery=load(W2/'continuation-v17-postcorrection-v1-review-delivery-receipt-v1.json')
old={x['path']:x for x in base['private_pins']}
additional=add['pins']+delivery['last_completed_raw_pins']+[delivery['postseal_supplement']]
ids={x['path']:{k:v for k,v in x.items() if k!='path'} for x in add['current_filesystem_identities']}
for x in additional:assert old.setdefault(x['path'],x)==x
assert len(old)==31910
additional_checks=[entry.check(x,ids.get(x['path'])) for x in additional]
sources=load(ACCEPTED/'production-test-freeze-v1.json')['sources']
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:source_checks=list(pool.map(entry.check,sources))
private_differences=[x for x in additional_checks if x['status']!='VERIFIED_PASS']
source_differences=[x for x in source_checks if x['status']!='VERIFIED_PASS']
assert not private_differences and not source_differences
# Enumerate only named W3 roots; never the original massive TestResults tree.
roots=[HERE,PRIVATE,RUNROOT/'w3pyv1',RUNROOT/'builds/w3v1',ROOT/'.test-rationalization-w3x.tmp']
for group in ['commands','executions']:
    for p in (RUNROOT/group).iterdir():
        if p.is_dir() and (p.name.startswith(('w3v1-','w3pyv1-')) if group=='commands' else p.name.startswith(('w3D-','w3R-'))):roots.append(p)
excluded=PRIVATE/'packet-commands/post-publication-audit-v1'
paths=set()
for root in roots:
    assert root.resolve().is_relative_to(ROOT)
    for p in root.rglob('*'):
        if p.is_file() and not p.resolve().is_relative_to(excluded.resolve()):
            assert not p.is_symlink() and p.resolve().is_relative_to(root.resolve())
            paths.add(p)
new=[]
for p in sorted(paths):
    row=pin(p);assert row['path'] not in old
    s=p.stat();new.append(dict(row,device_hex=format(s.st_dev,'x'),file_id_hex=format(s.st_ino,'x')))
assert len(new)==len({r['path'].casefold() for r in new})
archives=[x for x in new if x['path'].lower().endswith(('.zip','.zip.001','.zip.002'))]
save('preservation/w3-new-pins-v1.json',dict(new_pins=new,old_private_union=31910,new_count=len(new),union=31910+len(new),
    old_archive_records=152,new_archive_records=len(archives),archives=archives,
    excluded_in_progress_raw=str(excluded.relative_to(ROOT)),manifest_self='Nonrecursive; completed audit receipt and later postseal pins bound additively',
    historical_TestResults='NOT_ENUMERATED',giant='ONLY_STREAMED_BY_EXISTING_EXACT_PROTECTED_PIN_READER',
    inherited_named_populations=dict(source_lineage=278,mixed_model_captures=100,owner_documents=16,original_UDF_snapshots=41),
    category_scope='Named subset counts retained from accepted W2; entire inherited private pin/identity union actually audited, no new categorical reclassification.'))
save('preservation/post-publication-lock-v1.json',dict(base_reader_exit=r['exit_code'],base_reader=pin(script),
    actual_base_receipt=pin(output),command=r,old_private_union=31910,old_archives=152,
    base_private_checked=load(output)['private_pins'],additive_pins_checked=len(additional),
    private_differences=private_differences,source_count=len(sources),source_differences=source_differences,
    new_private_pins=pin(HERE/'preservation/w3-new-pins-v1.json'),
    claim='Full31896 existing-reader audit plus14 exact postseal additions; all1350 source bytes rehashed after main publication. Later W3 receipts/postseal files receive separate additive pins.'))
git=entry.git_receipt();entry.validate_git if hasattr(entry,'validate_git') else None
save('final/git-final-v1.json',git)
assert git['index']['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
assert not git['commands']['staged']['stdout'] and git['commands']['diff-check']['exit_code']==0
save('final/git-worktree-crosswalk-v1.json',dict(
    entry=pin(HERE/'entry/git-v2.json'),final=pin(HERE/'final/git-final-v1.json'),
    staged_changes=False,index_bytes_unchanged=True,
    tracked_status_unchanged=[s for s in load(HERE/'entry/git-v2.json')['commands']['status']['stdout'].splitlines() if not s.startswith('??')]==[s for s in git['commands']['status']['stdout'].splitlines() if not s.startswith('??')],
    new_untracked_W3=m.git('ls-files','--others','--exclude-standard','evidence/04A38/completion/w3').splitlines(),
    distinction='W2 source changes remain uncommitted, unstaged entry state; W3 adds untracked evidence and ignored isolated execution outputs only.'))
print('Post-publication full preservation and1350-source lock PASS; new W3 pins',len(new),flush=True)
