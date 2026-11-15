"""Replay unchanged original scanners over physical members on the owner repository."""
import argparse,hashlib,json,os,sys,zipfile
from pathlib import Path

def run(archive,repo):
    source_root=Path(__file__).resolve().parents[2]
    sys.path.insert(0,str(source_root/'evidence/04A39/physical-witness-v1'));import physical
    repo=Path(repo).resolve();assert (repo/'.git').exists() and os.environ.get('COMPUTERNAME'),'LIVE_OWNER_GIT_CONTEXT_REQUIRED'
    assert any(p.parent.name.casefold()=='users' for p in repo.parents),'OWNER_CONTEXT_UNAVAILABLE'
    physical.ROOT=repo
    class Scope(physical.ZipRoot):
        parents=repo.parents;name=repo.name
        def __fspath__(self):return str(repo)
        def as_posix(self):return repo.as_posix()
    results=[]
    with zipfile.ZipFile(archive) as z:
        inventory=physical.inventory(z);authority=json.loads(z.read('evidence/04A39/physical-witness-v1/exact-25-fixtures.json'))
        allowed={tuple(r['key']) for r in authority['fixtures']};assert len(allowed)==25
        for phase in ['13','14']:
            m=physical.scanner(phase);total=0;unresolved=0;views=0
            for r in inventory:
                b=z.read(r['path'])
                try:b.decode('utf-8-sig');encodings=['utf-8-sig']
                except UnicodeDecodeError:
                    encodings=['cp1250','cp1252']
                    for enc in encodings:assert b.decode(enc).encode(enc)==b
                for enc in encodings:
                    views+=1;m.ROOT=Scope(z,enc);found,reviewed,details=physical.detailed(m,[r['path']])
                    total+=len(found)
                    for row in details:
                        key=(row['path'],r['sha256'],row['origin'],row.get('line'),row['category'],row['offset'],row['match_sha256'],enc)
                        unresolved+=key not in allowed
            results.append(dict(scanner=phase,physical_members=len(inventory),encoding_views=views,original_findings=total,unresolved=unresolved,original_rules_unchanged=True))
    assert all(r['unresolved']==0 for r in results),'UNRESOLVED_PHYSICAL_PRIVACY_FINDINGS'
    return dict(status='PHYSICAL_DERIVATIVE_PUBLICATION_PASS',reports=results,owner_local_context=True,
                independent_limitation='Requires authentic owner Git HEAD and local username/hostname context; generic structural reader makes no such attestation.')
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('archive');p.add_argument('--repo',required=True);a=p.parse_args();print(json.dumps(run(a.archive,a.repo),sort_keys=True))
