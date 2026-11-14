"""Additively clarify failed predecessors versus successful reader4; fresh final Git."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v11'))
from common import *
old_addendum=W2/'continuation-v11-postseal-handover-addendum-v1.md'
new_addendum=W2/'continuation-v11-postseal-handover-addendum-v2.md'
before='Reader-v4 replaces that wrapper import with standard-library hashing; its actual exit1 and unchanged five ZIPs / first copy remain.'
after='Reader-v4 replaces that wrapper import with standard-library hashing, and the third copied replay exited0. The two earlier copied readers exited1; both failed copies, their raw traces, and all original five ZIPs remain unchanged.'
text=old_addendum.read_text();assert text.count(before)==1
text=text.replace(before,after)
text=text.replace('continuation-v11-final-git-preservation-v1.json is the last actual','continuation-v11-final-git-preservation-v2.json is the last actual')
text+='\nThis additive v2 corrects an ambiguous antecedent in the v1 handover prose. It does not change any actual outcome, sealed payload, private pin, physical ZIP or source byte. The predecessor v1 handover and final Git receipt remain unchanged.\n'
with new_addendum.open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
old_git=W2/'continuation-v11-final-git-preservation-v1.json';doc=json.loads(old_git.read_bytes());checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),
    ('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
    argv=['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args]
    begin=time.monotonic();result=subprocess.run(argv,cwd=ROOT,capture_output=True);assert result.returncode==0
    prior=next(r for r in doc['Git'] if r['check']==name);assert sha(result.stdout)==prior['stdout_sha256']
    checks.append(dict(check=name,command=clean(argv),exit_code=result.returncode,seconds=time.monotonic()-begin,
        stdout=result.stdout.decode(),stderr=clean(result.stderr.decode()),stdout_sha256=sha(result.stdout),stderr_sha256=sha(result.stderr),exact_stdout_equal=True))
assert pin(ROOT/'.git/index')==doc['index_file']
for row in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
for key in ['private_preservation','private_verification','live_preservation','publication','parts_index','copied_reader','post_publication_artifacts']:assert pin(ROOT/doc[key]['path'])==doc[key]
doc.update(Git=checks,postseal_addendum=pin(new_addendum),previous_final_git_receipt=pin(old_git),
    previous_postseal_addendum=pin(old_addendum),final_wording_tool=pin(Path(__file__)),
    correction='Only ambiguous prose antecedent: reader4 copied replay EXIT0, two retained predecessor copies EXIT1. Fresh Git/index/source/external pins remain exact; private verification was not substituted or relabelled.')
target=W2/'continuation-v11-final-git-preservation-v2.json'
with target.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
print(json.dumps(dict(final_git_receipt=pin(target),postseal_addendum=pin(new_addendum),status=doc['status'],private_pins=5661,files=7584,parts=7)))
