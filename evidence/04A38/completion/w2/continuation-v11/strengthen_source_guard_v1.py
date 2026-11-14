from common import *
from verify_source_transition_v1 import inspect
raw=lambda path:(ROOT/path).read_bytes()
load=lambda path:json.loads(raw(path))
result=inspect(load,raw,HERE.relative_to(ROOT).as_posix()+'/')
save('whole-file-source-transition-proof-v1.json',dict(result=result,reader=pin(HERE/'verify_source_transition_v1.py'),original_queries=0))
source=(HERE/'verify_c11_checkpoint_template_v1.py').read_text(encoding='utf-8')
point="    sys.path.insert(0,str(root/OLD))"
replacement="    from verify_source_transition_v1 import inspect as source_transition\n    source_transition(load,raw,PREFIX)\n"+point
assert point in source;source=source.replace(point,replacement)
point='    if check_validation:\n'
provenance="""    provenance=load(PREFIX+'frozen-provenance-v1.json')
    need(provenance['new_original_queries']==0 and provenance['scientific_truth']=='NOT_GOLDEN','Changed original/scientific provenance')
    check(provenance['comparison'])
    need(provenance['comparison']['path']==PREFIX+'compatibility-packet-v2.json','Wrong current compatibility authority')
    need(len(provenance['configurations'])==2 and {r['configuration'] for r in provenance['configurations']}=={'Debug','Release'},'Dropped configuration provenance')
    for row in provenance['configurations']:
        need(row['genuine_pre_edit']==28966 and row['total']==29424 and row['all_exact'] and {r['authority']:r['cases'] for r in row['late']}==dict(A=432,B=12,C=2,D=4,E=2,F=2,G=4) and all(r['classification']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT' for r in row['late']),'Late references relabelled or genuine population changed')
"""
assert source.count(point)==1;source=source.replace(point,provenance+point)
with (HERE/'verify_c11_checkpoint_template_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('reader-template-source-guard-v2.json',dict(before=pin(HERE/'verify_c11_checkpoint_template_v1.py'),
    after=pin(HERE/'verify_c11_checkpoint_template_v2.py'),guard=pin(HERE/'verify_source_transition_v1.py'),
    scope='Strengthen final reader with whole-file reconstruction, unchanged unrelated entry files and source-prefix preservation of every existing native test/assertion. No production/test changes.',original_queries=0))
print(result,flush=True)
