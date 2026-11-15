"""Same956 tests/unchanged assertions, new corrected frozen source state and isolated paths."""
from common40_v3 import *
lock();target=PRIVATE/'python-tools-v2';target.mkdir();cross=[]
for name in ['common.py','python_workers_v4.py','retained_scan_cache_v17_v1.py','scanner_retained_preflight_v2.py','scanner_cache_controls_v2.py','implementation-plan-v2.json']:
    before=PRIVATE/'python-tools'/name;text=before.read_text(encoding='utf-8-sig');edits=[]
    for a,b in [('.test-rationalization-04A40.tmp/python-private','.test-rationalization-04A40.tmp/python-private-v2'),('a40pyv1-','a40pyv2-'),('executions/a40-final-Debug-Common-semantic','executions/a40-final2-Debug-Common-semantic')]:
        count=text.count(a)
        if count:text=text.replace(a,b);edits.append(dict(before=a,after=b,count=count))
    (target/name).open('x',encoding='utf-8',newline='\n').write(text);cross.append(dict(before=pin(before),after=pin(target/name),path_substitutions=edits))
(PRIVATE/'python-private-v2').mkdir()
code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
for label,script,args in [('inventory','python_workers_v4.py',['inventory']),('cache-preflight','scanner_retained_preflight_v2.py',[]),('cache-controls','scanner_cache_controls_v2.py',[])]:
    result=command('python2-'+label,[sys.executable,'-I','-B','-c',code,str(target),str(target/script),*args]);save(PRIVATE/('python2-'+label+'-command.json'),result);assert result['exit_code']==0
plan=load(target/'python-discovery-inventory-v1.json');old=load(PRIVATE/'python-tools/python-discovery-inventory-v1.json')
assert len(plan['entries'])==956 and {(r['id'],r['module_sha256']) for r in plan['entries']}=={(r['id'],r['module_sha256']) for r in old['entries']}
save('python/discovery-v2.json',dict(parents=956,exact39_and_first40_identity_module_multiset=True,inventory=pin(target/'python-discovery-inventory-v1.json'),crosswalk=cross,assertions_changed=0,max_workers=3,scanner_helpers=0))
s=(HERE/'full-python-v1.py').read_text(encoding='utf-8')
for a,b in [("PRIVATE/'python-tools'","PRIVATE/'python-tools-v2'"),('python-input-freeze-v1.json','python-input-freeze-v2.json'),('python-input-postcheck-v1.json','python-input-postcheck-v2.json'),("command('python-full'","command('python-full-v2'")]:assert a in s;s=s.replace(a,b)
(HERE/'full-python-v2.py').open('x',encoding='utf-8').write(s)
s=(HERE/'python-reconcile-v1.py').read_text(encoding='utf-8')
for a,b in [("PRIVATE/'python-tools'","PRIVATE/'python-tools-v2'"),('python-event-crosswalk-raw-v1.json','python-event-crosswalk-raw-v2.json'),('python-input-freeze-v1.json','python-input-freeze-v2.json'),('python-input-postcheck-v1.json','python-input-postcheck-v2.json'),('parent-event-reconciliation-v1.json','parent-event-reconciliation-v2.json'),('python/discovery-v1.json','python/discovery-v2.json')]:assert a in s;s=s.replace(a,b)
(HERE/'python-reconcile-v2.py').open('x',encoding='utf-8').write(s)
print('Corrected-state956 discovery exact; independent paths; previous campaign retained',flush=True)
