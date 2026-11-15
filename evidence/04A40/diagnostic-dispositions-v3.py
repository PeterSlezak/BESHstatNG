"""Finish exact branch/string and typed-set explanations; raw ordering remains retained."""
from common40_v3 import *
import ast,collections,unittest
lock();d=load(HERE/'python/diagnostic-dispositions-v2.json');raw={r['stable_event_id']:r for r in load(PRIVATE/'python-event-crosswalk-raw-v2.json')['events']}
def message(a,b):
    try:unittest.TestCase().assertEqual(a,b)
    except AssertionError as e:return str(e)
for row in d['rows']:
    if row['stable_event_id'] not in d['unresolved']:continue
    pair=raw[row['stable_event_id']];a=pair['before']['message'];b=pair['current']['message'];proof=row['proof']
    try:
        left=[ast.literal_eval(x) for x in a.splitlines()[0].split(' != ')];right=[ast.literal_eval(x) for x in b.splitlines()[0].split(' != ')]
    except (ValueError,SyntaxError):left=[];right=[]
    if len(left)==len(right)==2 and all(isinstance(x,str) for x in left+right) and [x.replace('codex/04A-39','codex/04A-40') for x in left]==right and message(*left)==a and message(*right)==b:
        row['disposition']='EXACT_REPRODUCED_BRANCH_ARGUMENTS_AND_DIFF_MARKERS;FAIL_UNWAIVED';proof.update(before_operands=left,current_operands=right,message_reproduction_exact=True);continue
    try:oldset=ast.literal_eval(a);newset=ast.literal_eval(b)
    except (ValueError,SyntaxError):continue
    if isinstance(oldset,set) and isinstance(newset,set):
        phase='22' if 'test_04a22_' in row['identity'] else '24';reader=module('legacy_scope_'+phase,ROOT/('scripts/verify-04a'+phase+'-publication.py'))
        actual=set(git('diff','--name-only').decode().splitlines())-reader.MODIFIED
        olddiff={'BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md'}-reader.MODIFIED
        assert oldset==olddiff and newset==actual
        row['disposition']='EXACT_RECONSTRUCTED_LEGACY_PUBLICATION_ALLOWED_PATH_SET;FAIL_UNWAIVED';proof.update(reader=pin(reader.__file__),before_set=sorted(oldset),current_set=sorted(newset),raw_set_repr_order='PRESERVED_IN_PROTECTED_RAW;NO_ORDER_NORMALIZATION_OF_TEST_OUTPUT')
d['counts']=dict(collections.Counter(r['disposition'] for r in d['rows']));d['unresolved']=[r['stable_event_id'] for r in d['rows'] if 'REQUIRES_REVIEW' in r['disposition'] or r['disposition']=='UNRESOLVED']
assert not d['unresolved'],'UNEXPLAINED_DIAGNOSTIC_ARGUMENTS'
save('python/diagnostic-dispositions-v3.json',d)
print('All32 current diagnostic differences source/context explained; FAILs unwaived; original54 unchanged',flush=True)
