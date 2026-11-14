"""Independent reader for one explicitly private environment diagnostic.

The independently pinned public view retains every source/input/chronology
field. Only the machine-specific worktree location is excluded from export.
"""
import json,hashlib
from pathlib import Path
PREFIX='evidence/04A38/late-archived-entry-supplement/v2-gee-profile/'
PRIVATE=PREFIX+'entry-inspection.json'
PUBLIC=PREFIX+'entry-inspection-public.json'
PUBLIC_HASH='d856f6d81128d51fea83084f1f6048039846b9aa429bb66f55865ad0409de296'

def inspect(root):
 root=Path(root);data=(root/PUBLIC).read_bytes()
 if hashlib.sha256(data).hexdigest()!=PUBLIC_HASH:raise ValueError('Changed public provenance/environment control')
 public=json.loads(data)
 if '<REPO_ROOT>' not in public['git']['worktrees']:raise ValueError('Unredacted environment')
 if (root/PRIVATE).exists():
  raw=json.loads((root/PRIVATE).read_bytes())
  old=raw['git']['worktrees'];suffix='  ba1636f [codex/04A-38]\n'
  if not old.endswith(suffix):raise ValueError('Unexpected raw worktree diagnostic')
  raw['git']['worktrees']='<REPO_ROOT>'+suffix
  if raw!=public:raise ValueError('Public view changed a nonenvironment field')
 auth=json.loads((root/'evidence/04A38/late-archived-entry-supplement/v1/original-source-authenticity.json').read_bytes())
 if public['original_tree_files']!=auth['files']:raise ValueError('Public source provenance differs from authentic original tree')
 return public
