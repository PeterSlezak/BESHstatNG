"""Fail-closed physical derivative gate. Never creates an archive or edits source."""
from common import *
from scanner_adapter import module, detailed
import re

class StageRoot:
    parents=ROOT.parents
    name=ROOT.name
    def __init__(self,stage):self.stage=Path(stage)
    def __truediv__(self,path):return self.stage/path
    def __fspath__(self):return str(ROOT)
    def as_posix(self):return ROOT.as_posix()

def safe_name(name):
    assert name and '\\' not in name and not name.startswith('/') and ':' not in name
    assert all(p not in ('','.', '..') for p in name.split('/'))
    assert all(p.casefold() not in {'.git','testresults','__pycache__'} for p in name.split('/'))
    assert not any(p.startswith('.test-rationalization') for p in name.split('/'))
    assert all(not p.endswith((' ','.')) for p in name.split('/'))
    assert not any(re.match(r'(?i)^(con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|$)',p) for p in name.split('/'))

def physical_inventory(stage,expected,protected):
    stage=Path(stage);assert stage.is_dir() and not stage.is_symlink()
    actual=[]
    # Only the newly allocated bounded private stage is traversed.
    for path in stage.rglob('*'):
        assert not path.is_symlink() and not path.is_junction(), 'UNSAFE_LINK'
        if path.is_file():actual.append(path.relative_to(stage).as_posix())
    for path in actual:safe_name(path)
    assert len(actual)==len(set(actual))==len({p.casefold() for p in actual}), 'PATH_COLLISION'
    expected_by_path={r['path']:r for r in expected}
    assert len(expected_by_path)==len(expected), 'DUPLICATE_EXPECTED_PATH'
    assert set(actual)==set(expected_by_path), 'MISSING_OR_EXTRA_PAYLOAD'
    blocked_paths={r['path'] for r in protected}
    blocked_hashes={r['original']['sha256'] for r in protected}
    for name in actual:
        row=expected_by_path[name];path=stage/name
        assert path.stat().st_size==row['bytes'] and digest(path)==row['sha256'], 'CHANGED_STAGED_BYTES'
        assert name not in blocked_paths and row['sha256'] not in blocked_hashes, 'PROTECTED_RAW_PAYLOAD'
    return sorted(actual)

def scan(stage,names):
    m=module('13');m.ROOT=StageRoot(stage)
    return detailed(m,names)

def finding_key(row,file_sha):
    return (row['path'],file_sha,row['origin'],row.get('line'),row['category'],row['offset'],row['match_sha256'])

def decide(findings,details,files,reviewed):
    bypath={r['path']:r for r in files}
    allowed={tuple(r['key']):r for r in reviewed}
    assert len(allowed)==len(reviewed), 'DUPLICATE_DISPOSITION'
    unresolved=[];used=set()
    for row in details:
        key=finding_key(row,bypath[row['path']]['sha256'])
        if key not in allowed:unresolved.append(row)
        else:
            assert allowed[key]['disposition']=='REVIEWED_PUBLIC_OR_SYNTHETIC_SOURCE_LITERAL'
            assert allowed[key]['proof'] and allowed[key]['source']
            used.add(key)
    assert used==set(allowed), 'STALE_OR_BROAD_ALLOWANCE'
    assert not unresolved, 'UNRESOLVED_SENSITIVE_MATERIAL'
    return dict(scanner_findings=len(findings),reviewed_exact_occurrences=len(used),unresolved=0,privacy_status='PASS_STAGED_DERIVATIVE_ONLY',repository_candidate_status='FAIL_UNWAIVED')
