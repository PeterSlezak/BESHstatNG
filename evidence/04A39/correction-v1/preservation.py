"""Bounded final readback of exact protected paths; never walks historical TestResults."""
from common import *
import os

def verify_checkpoint():
    result=[]
    for row in load(PRIVATE/'checkpoint-files.json')['files']:
        now=pin(row['path']);assert now=={k:row[k] for k in ('path','bytes','sha256')},'IMMUTABLE_CHECKPOINT_CHANGED'
        stat=(ROOT/row['path']).stat()
        assert stat.st_dev==row['device'] and stat.st_ino==row['file_id'],'IMMUTABLE_FILE_ID_CHANGED'
        copied=PRIVATE/'checkpoint'/row['path'];assert digest(copied)==row['sha256'] and copied.stat().st_size==row['bytes']
        result.append(now)
    lock()
    return result

def snapshot(label):
    originals=verify_checkpoint()
    # Only this task's new private directory is inventoried, not other ignored trees.
    private=[]
    for path in sorted(PRIVATE.rglob('*')):
        if not path.is_file() or any(p.startswith('publication-stage') for p in path.relative_to(PRIVATE).parts):continue
        private.append(pin(path))
    save(PRIVATE/('private-pins-'+label+'.json'),dict(files=private,source_lock=pin(HERE/'entry/source-lock.json'),checkpoint=originals,
        scope='Incremental exact468 original39 dossier readback plus new task-private files only; stage separately inventoried; no historical TestResults traversal; no repeated37484/154 full audit.',
        historical_W3_union=dict(files=37484,archive_declarations=154,classification='INHERITED_QUALIFIED_PHYSICAL_SCOPE;NOT_FRESH_FULL_UNION')))
    save('preservation/'+label+'.json',dict(original39_files=len(originals),original39_bytes=sum(r['bytes'] for r in originals),
        original39_unchanged=True,physical_file_identities_unchanged=True,frozen_source_paths=len(load(HERE/'entry/source-lock.json')['files']),index_sha256=digest(ROOT/'.git/index'),
        new_private_files=len(private),protected_inventory=pin(PRIVATE/('private-pins-'+label+'.json')),historical_union=dict(files=37484,archive_declarations=154,scope='INHERITED_NOT_REAUDITED'),
        original_raw_diagnostics_retained=True,no_historical_TestResults_walk=True))
    print('Exact original checkpoint preserved:',len(originals),'files; new protected pins',len(private),flush=True)

if __name__=='__main__':snapshot(sys.argv[1])
