"""Path-only adaptation of accepted39 six-lane workers. All outputs are private."""
from common40_v3 import *
lock();target=PRIVATE/'python-tools';target.mkdir()
prior=ROOT/'evidence/04A39/python-node-scanners/campaign-v2'
names=['common.py','python_workers_v4.py','retained_scan_cache_v17_v1.py','scanner_retained_preflight_v2.py','scanner_cache_controls_v2.py']
cross=[]
for name in names:
    text=(prior/name).read_text(encoding='utf-8-sig');changes=[]
    replacements={"ROOT=Path(__file__).resolve().parents[4]":"ROOT=Path(__file__).resolve().parents[2]",
        "BASE='22da2214da6bb1cad4f2934d0b36276324a1432f'":"BASE='"+MAIN+"'",
        ".test-rationalization-04A39.tmp/python-private":".test-rationalization-04A40.tmp/python-private",
        "ROOT/'.test-rationalization-04A39.tmp'":"ROOT/'.test-rationalization-04A40.tmp'",
        "'a39pyv1-'":"'a40pyv1-'",
        ".test-rationalization-04A39.tmp/executions/a39fD-full-Common-semantic/observations":".test-rationalization-04A40.tmp/executions/a40-final-Debug-Common-semantic/observations"}
    for before,after in replacements.items():
        count=text.count(before)
        if count:text=text.replace(before,after);changes.append(dict(before=before,after=after,count=count))
    with (target/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
    cross.append(dict(before=pin(prior/name),after=pin(target/name),substitutions=changes))
with (target/'implementation-plan-v2.json').open('xb') as f:f.write((prior/'implementation-plan-v2.json').read_bytes())
(PRIVATE/'python-private').mkdir()
save('python/driver-crosswalk-v1.json',dict(files=cross,assertions_changed=0,max_workers=3,owner_ceiling=6,scanner_workers=1,original_tests_changed=False,
    historical_cache='Unchanged5829-row reader; current HEAD differs, so old environment-dependent cache may decline reuse. No new fallback assertion or source normalization.',
    source_binding='Stock Python modules/dependencies unchanged; production and branch deltas remain real assertions/nonpasses, never rebaselined.'))
code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
for label,script,args in [('inventory','python_workers_v4.py',['inventory']),('cache-preflight','scanner_retained_preflight_v2.py',[]),('cache-controls','scanner_cache_controls_v2.py',[])]:
    result=command('python-'+label,[sys.executable,'-I','-B','-c',code,str(target),str(target/script),*args])
    save(PRIVATE/('python-'+label+'-command.json'),result);assert result['exit_code']==0
plan=load(target/'python-discovery-inventory-v1.json');assert len(plan['entries'])==956
old=load(prior/'python-discovery-inventory-v1.json')
assert {(r['id'],r['module_sha256']) for r in plan['entries']}=={(r['id'],r['module_sha256']) for r in old['entries']}
save('python/discovery-v1.json',dict(parents=956,exact39_identity_and_module_multiset=True,inventory=pin(target/'python-discovery-inventory-v1.json'),
    scripts=[pin(target/n) for n in names],cache_controls=pin(target/'scanner-cache-controls-v2.json') if (target/'scanner-cache-controls-v2.json').exists() else None))
print('Python956 identities and test-module bytes exact39; worker/cache controls prepared with private output paths',flush=True)
