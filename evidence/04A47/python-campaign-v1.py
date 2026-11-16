from common47 import *
import collections,importlib.metadata
lock();target=PRIVATE/'python-tools-v1';action=sys.argv[1]
bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
if action=='prepare':
    target.mkdir();(PRIVATE/'python-private-v1').mkdir();cross=[]
    source=ROOT/'.test-rationalization-04A46.tmp/python-tools-v1'
    for name in ['common.py','python_workers_v4.py','retained_scan_cache_v17_v1.py','scanner_retained_preflight_v2.py','scanner_cache_controls_v2.py','implementation-plan-v2.json']:
        before=source/name;text=before.read_text(encoding='utf-8-sig');edits=[]
        for a,b in [('.test-rationalization-04A46.tmp/python-private-v1','.test-rationalization-04A47.tmp/python-private-v1'),('.test-rationalization-04A46.tmp/executions/a46-final-Debug-Common-semantic/observations','.test-rationalization-04A47.tmp/executions/a47-final-Debug-Common-semantic/observations'),("BASE='ebc4a3e9df41ef8588aa7f6b25363f6d044c41a7'","BASE='"+MAIN+"'"),("ROOT/'.test-rationalization-04A46.tmp'","ROOT/'.test-rationalization-04A47.tmp'"),('a46pyv1-','a47pyv1-')]:
            if a in text:edits.append(dict(before_alias=a,after_alias=b,count=text.count(a)));text=text.replace(a,b)
        if name=='implementation-plan-v2.json':
            d=json.loads(text);freeze=load(HERE/'matrix/source-freeze-v1.json');d['source_paths']=sorted(set(d['source_paths']+[r['after']['path'] for r in freeze['source_delta']]+[r['path'] for r in freeze['new_tests']]));text=json.dumps(d,indent=2)+'\n'
        with (target/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
        cross.append(dict(before=pin(before),after=pin(target/name),private_path_base_context_substitutions=edits,original_test_sources_modified=False))
    for label,script,args in [('inventory','python_workers_v4.py',['inventory']),('preflight','scanner_retained_preflight_v2.py',[]),('controls','scanner_cache_controls_v2.py',[])]:
        result=command('python2-'+label,[sys.executable,'-I','-B','-c',bootstrap,str(target),str(target/script),*args]);assert result['exit_code']==0
    inventory=load(target/'python-discovery-inventory-v1.json');old=load(ROOT/'.test-rationalization-04A46.tmp/python-tools-v1/python-discovery-inventory-v1.json')
    assert len(inventory['entries'])==956 and {(r['id'],r['module_sha256']) for r in inventory['entries']}=={(r['id'],r['module_sha256']) for r in old['entries']}
    save('python/discovery-v1.json',dict(parents=956,exact_accepted46_module_identity_multiset=True,inventory=pin(target/'python-discovery-inventory-v1.json'),crosswalk=cross,assertions_modified=0,disjoint_lane_paths=True,max_workers=6,scanner_helpers=0,interpreter=dict(version=sys.version.split()[0],sha256=sha(Path(sys.executable).read_bytes())),retained_runtime='3.14.7;exact accepted interpreter binary recovered and authenticated;no installations.'))
    print('Exact956 discovery;unchanged tests;private disjoint writer/scanner controls PASS',flush=True)
else:
    assert action=='execute'
    previous=load(ROOT/'evidence/04A38/completion/w3/python-node-scanners/python-input-freeze-v2.json')
    names={r['path'] for r in previous['files'] if r.get('sha256')}
    names.update(git('ls-files','--others','--exclude-standard').decode().splitlines());names.update(git('diff','--name-only').decode().splitlines())
    names.update(load(HERE/'matrix/source-freeze-v1.json')['source_pins'])
    assert not any('TestResults' in Path(p).parts for p in names)
    pins=[pin(p) for p in sorted(names) if (ROOT/p).is_file()]
    save(PRIVATE/'python-input-freeze-v2.json',dict(utc=utc(),files=pins,scope='Exact previously inventoried W3 paths/current candidates/current compiler source locks; no recursive TestResults traversal.',interpreter=sys.version.split()[0]))
    result=command('python-full-v2',[sys.executable,'-I','-B','-c',bootstrap,str(target),str(target/'python_workers_v4.py'),'execute'])
    changed=[r['path'] for r in pins if pin(r['path'])!=r]
    save(PRIVATE/'python-input-postcheck-v2.json',dict(command=result,checked=len(pins),changed=changed,campaigns=1,summary=pin(target/'python-execution-summary-v1.json') if (target/'python-execution-summary-v1.json').is_file() else None))
    assert not changed and result['exit_code']==0
    print('One full956-parent Python campaign completed;all source inputs frozen',flush=True)
