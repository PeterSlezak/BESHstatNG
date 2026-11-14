"""Append-only live entry inspection. Never executes a UDF or changes Git."""
import hashlib, json, subprocess
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
BASE = 'ba1636f5356ac761509c2cfa16fc66b92430935d'
CLASS = 'LATE_ARCHIVED_ENTRY_SUPPLEMENT'

def sha(data): return hashlib.sha256(data).hexdigest()
def pin(path):
    data = path.read_bytes()
    return dict(path=path.relative_to(ROOT).as_posix(), bytes=len(data), sha256=sha(data))
def save(name, value):
    with (HERE/name).open('x', encoding='utf-8', newline='\n') as stream:
        json.dump(value, stream, indent=2, ensure_ascii=False, allow_nan=False)
        stream.write('\n')
def git(*args):
    return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args], cwd=ROOT)

def main():
    state = dict(branch=git('branch','--show-current').decode().strip(),
                 head=git('rev-parse','HEAD').decode().strip(), main=git('rev-parse','main').decode().strip(),
                 merge_base=git('merge-base','HEAD','main').decode().strip(),
                 index=git('diff','--cached','--name-only').decode(),
                 status=git('status','--short').decode(), worktrees=git('worktree','list').decode())
    assert state['branch']=='codex/04A-38' and state['head']==state['main']==state['merge_base']==BASE
    assert state['index']==''
    diff = subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True)
    assert diff.returncode==0
    # Rehash each entire older publication, never update it.
    predecessors=[]
    for rel, count in [('publication-manifest.json',611),('continuation/v1/publication-manifest.json',687),
                       ('late-archived-entry-supplement/v1/publication-manifest.json',774)]:
        path=ROOT/'evidence/04A38'/rel
        manifest=json.loads(path.read_bytes()); entries=manifest['entries']
        assert len(entries)==count+1
        assert len({r['path'].casefold() for r in entries})==len(entries)
        for row in entries:
            if row['sha256'] is not None: assert pin(ROOT/row['path'])==row, row['path']
        predecessors.append(dict(manifest=pin(path),external_payloads=count))
    snapshots=[]
    for row in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']:
        blob=git('cat-file','blob',BASE+':'+row['source']['path'])
        snapshot=(ROOT/row['snapshot']['path']).read_bytes()
        assert pin(ROOT/row['snapshot']['path'])==row['snapshot']
        relation='EXACT' if blob==snapshot else 'PROVED_LF_CRLF_ONLY'
        assert blob==snapshot or blob.replace(b'\r\n',b'\n')==snapshot.replace(b'\r\n',b'\n')
        if row['source']['path'].endswith('/GeeUDFs.vb'):
            assert blob==snapshot and len(blob)==114843
            assert sha(blob)=='35a3a35ddf4dfdb074a082dfe92495969dd1a8532f86af6650d6bde33bcf7649'
        snapshots.append(dict(source_path=row['source']['path'],git_bytes=len(blob),git_sha256=sha(blob),snapshot=row['snapshot'],relation=relation))
    assert len(snapshots)==41
    # The pristine source tree is a read-only input; build outputs must use new paths.
    tree=ROOT/'.test-rationalization-04A38.tmp/late38-v1/original-tree'
    auth=json.loads((ROOT/'evidence/04A38/late-archived-entry-supplement/v1/original-source-authenticity.json').read_bytes())
    names=[p for p in git('ls-tree','-r','--name-only','-z',BASE).decode().split('\0') if p]
    by_public={r['path']:r for r in auth['files']}; checked=[]
    for name in names:
        token='<PRIVATE_PROTECTED_PATH:'+sha(name.encode())[:24]+'>'
        public=token if token in by_public else name
        row=by_public[public]
        # Extended paths permit original long tracked paths without changing OS settings.
        data=Path('\\\\?\\'+str((tree/name).absolute())).read_bytes()
        blob=git('cat-file','blob',BASE+':'+name)
        assert data==blob and len(data)==row['bytes'] and sha(data)==row['sha256'], public
        checked.append(row)
    assert len(checked)==4695
    save('owner-authorization.json',dict(owner_quote='I approve the additional 12-case-per-configuration GEE archived-entry supplement for 04A.38.',
         owner_decision_date='2026-11-09',provenance='Explicit owner authorization in this execution prompt',
         reference_class=CLASS,cases_per_configuration=12,cultures=['Invariant','sk-SK'],case_kinds=6,
         configurations=['Debug','Release'],source_commit=BASE,recorded_utc=datetime.now(timezone.utc).isoformat(),
         separate_prior_432_approval=pin(ROOT/'evidence/04A38/late-archived-entry-supplement/v1/owner-authorization.json'),
         proposal=pin(ROOT/'evidence/04A38/late-archived-entry-supplement/v1/proposed-gee-profile-decision.json'),
         scientific='NOT_GOLDEN',native='NOT_RUN'))
    save('entry-inspection.json',dict(status='AUTHENTICATED_ENTRY',captured_utc=datetime.now(timezone.utc).isoformat(),
         git=state,diff_check_exit=diff.returncode,predecessors=predecessors,source_commit=BASE,
         udf_snapshots=snapshots,original_tree_files=checked,original_tree_rehashed_against_live_git=True,
         new_path=HERE.relative_to(ROOT).as_posix(),production_changes=0,reference_executions=0,
         historical_TestResults='NOT_TRAVERSED_OR_REHASHED',reference_class=CLASS))
    print('Authenticated 4695 original tree files, 41 UDF snapshots and 2072 predecessor hash assertions; recorded separate GEE approval')

if __name__=='__main__': main()
