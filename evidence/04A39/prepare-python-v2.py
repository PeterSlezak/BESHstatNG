"""Path-only adaptation of the authenticated W3 worker and cache readers."""
from common39 import *
source_lock()
target=HERE/'python-node-scanners/campaign-v2';target.mkdir(parents=True,exist_ok=False)
source=W3/'python-node-scanners/campaign-v1';changes=[]
specs={
    'common.py':[
        ('ROOT=Path(__file__).resolve().parents[6]','ROOT=Path(__file__).resolve().parents[4]'),
        ("BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'","BASE='"+MAIN+"'"),
        ("PRIVATE=RUNROOT/'w3pyv1'","PRIVATE=ROOT/'.test-rationalization-04A39.tmp/python-private'"),
        ("m.Runner(RUNROOT).command('w3pyv1-'+label","m.Runner(ROOT/'.test-rationalization-04A39.tmp').command('a39pyv1-'+label")],
    'python_workers_v4.py':[
        ("PRIVATE=RUNROOT/'w3pyv1'","PRIVATE=ROOT/'.test-rationalization-04A39.tmp/python-private'"),
        ("RUNROOT/'executions/w3D-Common-semantic/observations'","ROOT/'.test-rationalization-04A39.tmp/executions/a39fD-full-Common-semantic/observations'")],
    'retained_scan_cache_v17_v1.py':[], 'scanner_retained_preflight_v2.py':[], 'scanner_cache_controls_v2.py':[]}
for name,replacements in specs.items():
    text=(source/name).read_text(encoding='utf-8-sig')
    for before,after in replacements:
        assert text.count(before)==1,(name,before,text.count(before))
        text=text.replace(before,after)
    (target/name).write_text(text,encoding='utf-8',newline='\n')
    changes.append(dict(before=pin(source/name),after=pin(target/name),substitutions=[dict(before=a,after=b) for a,b in replacements]))
(target/'implementation-plan-v2.json').write_bytes((source/'implementation-plan-v2.json').read_bytes())
(PRIVATE/'python-private').mkdir(exist_ok=False)
save('python-node-scanners/driver-crosswalk-v2.json',dict(changes=changes,assertions_changed=0,
    historical_RUNROOT='Unchanged04A38 private root used read-only for exact cache authority,5829 successful rows and11-file disposable provenance. All commands/results/temp/cache go into new04A39 private root.',
    expected_parents=956,max_parent_workers=3,scanner_workers=1,scanner_helpers=0,source_hashseed='38 unchanged from authenticated workers',
    scheduling='Five non-scanner lanes, maximum3 simultaneous processes; disposable lane5 part next; lane1 shared scanners alone last.',
    privacy='Two historical UnicodeDecodeError ERRORs remain unwaived; unchanged assertion/cache kernels and actual adverse controls.'))
code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
for name,script,args in [('inventory','python_workers_v4.py',['inventory']),('cache-preflight','scanner_retained_preflight_v2.py',[]),('cache-controls','scanner_cache_controls_v2.py',[])]:
    result=command('python-'+name,[sys.executable,'-I','-B','-c',code,str(target),str(target/script),*args])
    save('python-node-scanners/'+name+'-execution-v1.json',result);assert result['exit_code']==0
source_lock()
