"""Prepare publication and exact current Git bindings; retain all predecessor sources."""
from common import *
def write(name,value):
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(value)
v=(PREVIOUS/'publish_parts_v1.py').read_text().replace('publication_privacy_c12_v1','publication_privacy_c13_v1').replace('continuation-v12-','continuation-v13-').replace('W2_v12_','W2_v13_').replace('C12_REVIEW','C13_REVIEW').replace('inherited5661','inherited8801')
v=v.replace("W2/'continuation-v11-review-parts-index-v3.json'","W2/'continuation-v12-review-parts-index-v1.json'")
a=v.index('    # Final prior external sources/receipts');b=v.index('    return names',a)
v=v[:a]+'''    # Every final/failed/superseded public v12 receipt/tool remains additive authority.
    names.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v12-*') if p.is_file() and p.suffix in ['.json','.py','.md'])
    names.add((W2/'continuation-v13-retained-private-reader-v1.py').relative_to(ROOT).as_posix())
    names={name for name in names if not name.startswith('.test-rationalization-04A38.tmp/')}
'''+v[b:]
v=v.replace("source=pin(PREVIOUS/'review-exclusions-v1.json'),","source=pin(PREVIOUS/'review-exclusions-v1.json'),private_entry_authorities=[r for r in json.loads((HERE/'entry-authority-pins-v1.json').read_bytes())['authorities'] if r['path'].startswith('.test-rationalization-04A38.tmp/')],")
write('publish_parts_v1.py',v)
for old,new,changes in [('copy_review_v1.py','copy_review_v1.py',{'c12':'c13','C12':'C13','continuation-v12':'continuation-v13'}),('finalize_live_v1.py','finalize_live_v1.py',{'c12':'c13','C12':'C13','continuation-v12':'continuation-v13'})]:
 value=(PREVIOUS/old).read_text()
 for a,b in changes.items():value=value.replace(a,b)
 if old=='finalize_live_v1.py':value=value[:value.index('\ndef final():')]+"\nif __name__=='__main__':preserve()\n"
 write(new,value)
v=(PREVIOUS/'capture_exit_git_v1.py').read_text().replace('w2c11-exit-git-','w2c13-exit-git-');write('capture_exit_git_v1.py',v)
save('delivery-tools-source-crosswalk-v1.json',dict(sources=[pin(HERE/n) for n in ['publish_parts_v1.py','copy_review_v1.py','finalize_live_v1.py','capture_exit_git_v1.py']],
    scope='C13 exact complete v12 union and additive authority, private raw entry records omitted only from derivatives. Fresh Git Runner labels and bound raw folders are both w2c13; never inherit stale predecessor labels. All actual prior correction receipts remain.',original_queries=0))
