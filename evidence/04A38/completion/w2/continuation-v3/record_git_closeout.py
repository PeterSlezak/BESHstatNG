from common import *
from verify_preservation import inspect
version=sys.argv[1];p=inspect(ROOT,True);entry=json.loads((HERE/'entry.json').read_bytes());oldstatus=entry['git_status'].splitlines();retained=[]
for line in oldstatus:
 if not line:continue
 path=line[3:]
 if path.startswith('"'):path=json.loads(path)
 assert (ROOT/path).exists(),'Pre-existing modified/untracked path missing: '+path
 retained.append(path)
result=dict(preservation=p,preexisting_status_paths_retained=len(retained),git_status=git('status','--porcelain=v1','-uall').decode(),index_entries=git('diff','--cached','--name-only').decode(),branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),source_transitions=pin(HERE/'source-transitions-final.json'),old_source_snapshots='UNCHANGED',Git_publication_actions='NONE',Phase04A39='NOT_STARTED')
assert result['index_entries']=='' and result['HEAD']==result['main']==result['merge_base']==BASE and result['branch']=='codex/04A-38'
save('git-closeout-'+version+'.json',result)
print('Live Git/index/diff/preservation PASS; retained entry-status paths',len(retained))
