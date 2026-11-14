"""Add process/time receipts without rediscovering or changing any test selection."""
from common import *
import ast
before=(HERE/'python_workers_v1.py').read_text(encoding='utf-8');after=before.replace('python_workers_v1.py','python_workers_v2.py')
after=after.replace('import ast, collections, importlib.util, inspect, unittest, traceback','import ast, collections, importlib.util, inspect, unittest, traceback, datetime')
after=after.replace('records=[];active={};occurrences=collections.Counter();start=time.monotonic()', "records=[];active={};occurrences=collections.Counter();start=time.monotonic();start_utc=datetime.datetime.now(datetime.timezone.utc).isoformat()")
after=after.replace('report=dict(lane=number,phase=phase,records=', "report=dict(lane=number,phase=phase,pid=os.getpid(),start_utc=start_utc,end_utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),records=")
with (HERE/'python_workers_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(after)
trees=[ast.parse(s) for s in [before,after]]
for name in ['cases','discover','lane','inventory_worker']:
    a,b=[next(n for n in t.body if isinstance(n,ast.FunctionDef) and n.name==name) for t in trees]
    assert ast.dump(a,include_attributes=False)==ast.dump(b,include_attributes=False)
save('python-executor-version-crosswalk-v2.json',dict(before=pin(HERE/'python_workers_v1.py'),after=pin(HERE/'python_workers_v2.py'),discovery_AST_unchanged=True,selection_assertions_unchanged=True,change='Record PID and UTC start/end alongside per-test monotonic timings; all execution/discovery command source paths refer to this version.',original_queries=0))
