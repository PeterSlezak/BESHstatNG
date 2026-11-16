"""Original Python campaign with output-path-only successor; assertions unchanged."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from recovery import *
TOOLS=PRIVATE/'python-tools-v1'
BOOT='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
def prepare():
    source_lock();payload();assert load(HERE/'focused-parent-comparison-v1.json')['accepted46_message_exact']
    TOOLS.mkdir(parents=True);(PRIVATE/'python-private-v1').mkdir();cross=[]
    source=ROOT/'.test-rationalization-04A47.tmp/python-tools-v1'
    for name in ['common.py','python_workers_v4.py','retained_scan_cache_v17_v1.py','scanner_retained_preflight_v2.py','scanner_cache_controls_v2.py','implementation-plan-v2.json']:
        p=source/name;text=p.read_text(encoding='utf-8-sig');edits=[]
        pairs=[('.test-rationalization-04A47.tmp/python-private-v1','.test-rationalization-04A47-recovery-v2.tmp/python-private-v1'),("m.Runner(ROOT/'.test-rationalization-04A47.tmp')","m.Runner(ROOT/'.test-rationalization-04A47-recovery-v2.tmp')"),('a47pyv1-','a47recoveryv2-')]
        for a,b in pairs:
            if a in text:edits.append(dict(before=a,after=b,count=text.count(a)));text=text.replace(a,b)
        with (TOOLS/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
        cross.append(dict(before=pin(p),after=pin(TOOLS/name),path_only_edits=edits))
    save('runner-path-transitions-v1.json',dict(files=cross,test_source_edits=0,assertion_edits=0,max_workers=3,semantic_observer_input='UNCHANGED_SEALED_04A47',interpreter=load(HERE/'payload-authentication-v1.json')['interpreter']))
    for label,name,args in [('inventory','python_workers_v4.py',['inventory']),('preflight','scanner_retained_preflight_v2.py',[]),('controls','scanner_cache_controls_v2.py',[])]:
        result=command(label,[sys.executable,'-I','-B','-c',BOOT,str(TOOLS),str(TOOLS/name),*args]);assert result['exit_code']==0
    inv=load(TOOLS/'python-discovery-inventory-v1.json');old=load(source/'python-discovery-inventory-v1.json')
    assert len(inv['entries'])==956 and {(r['id'],r['module_sha256']) for r in inv['entries']}=={(r['id'],r['module_sha256']) for r in old['entries']}
    save('discovery-v1.json',dict(parents=956,inventory=pin(TOOLS/'python-discovery-inventory-v1.json'),module_identities_exact_v1_and_accepted46=True,source_modules=inv['modules'],peak_workers=3,interpreter=load(HERE/'payload-authentication-v1.json')['interpreter']))
    print('Fresh956 discovery exact; path-only successor and retained scanner controls verified',flush=True)
def execute():
    source_lock();payload()
    prior=load(ROOT/'evidence/04A38/completion/w3/python-node-scanners/python-input-freeze-v2.json')
    names={r['path'] for r in prior['files'] if r.get('sha256')}
    names.update(git('diff','--name-only').decode().splitlines());names.update(git('ls-files','--others','--exclude-standard').decode().splitlines())
    names.update(load(OLD/'matrix/source-freeze-v1.json')['source_pins'])
    pins=[pin(p) for p in sorted(names) if (ROOT/p).is_file()];save(PRIVATE/'python-input-freeze-v2.json',dict(utc=utc(),files=pins,interpreter=load(HERE/'payload-authentication-v1.json')['interpreter'],payload=payload()))
    result=command('full-python',[sys.executable,'-I','-B','-c',BOOT,str(TOOLS),str(TOOLS/'python_workers_v4.py'),'execute'])
    changed=[r['path'] for r in pins if pin(r['path'])!=r]
    save(PRIVATE/'python-input-postcheck-v2.json',dict(command=result,changed=changed,checked=len(pins),summary=pin(TOOLS/'python-execution-summary-v1.json') if (TOOLS/'python-execution-summary-v1.json').is_file() else None,payload=payload(),campaigns=1))
    assert not changed and result['exit_code']==0
    print(json.dumps(load(TOOLS/'python-execution-summary-v1.json')),flush=True)
if __name__=='__main__':
    try:globals()[sys.argv[1]]()
    except Exception:
        import traceback
        save(PRIVATE/('campaign-failure-'+datetime.datetime.now().strftime('%Y%m%d-%H%M%S')+'.json'),dict(action=sys.argv[1],traceback=traceback.format_exc()))
        print('Campaign failure retained privately; no original output overwritten',flush=True);raise SystemExit(2)
