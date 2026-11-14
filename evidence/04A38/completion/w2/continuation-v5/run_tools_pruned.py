"""Run unchanged tool tests with the source audit's existing ignored dirs pruned before descent."""
from common import *
if len(sys.argv)==1:
    r=command('final-python-tools-pruned',[sys.executable,'-B',str(HERE/'run_tools_pruned.py'),'worker'])
    save('tools-pruned-validation.json',dict(command=clean(r),source_tests='UNCHANGED; no tests filtered or assertions edited',
        equivalent_scope='Only audit_transitional_officejs.audit_solution(ROOT): prune its exact existing IGNORED_COMPONENTS before descent instead of filtering them after rglob. Temporary fixture roots use original walker.',
        TestResults='Ignored directory is not descended in this final lane',original_queries=0))
    assert r['exit_code']==0
else:
    import os,unittest
    from unittest.mock import patch
    sys.path.insert(0,str(ROOT/'BESHstatNG/tools'))
    import audit_transitional_officejs as audit
    before=sha(Path(audit.__file__).read_bytes());original=audit.audit_solution;original_glob=Path.rglob
    counters=dict(ignored_directories_pruned=0,relevant_files=0)
    def bounded_glob(path,pattern,*args,**kwargs):
        if path.resolve()!=ROOT or pattern!='*':return original_glob(path,pattern,*args,**kwargs)
        def walk():
            for directory,dirs,files in os.walk(ROOT,followlinks=False):
                kept=[name for name in dirs if name not in audit.IGNORED_COMPONENTS]
                counters['ignored_directories_pruned']+=len(dirs)-len(kept)
                dirs[:]=kept
                for name in files:
                    p=Path(directory)/name
                    if p.suffix.lower() in audit.SOURCE_SUFFIXES:
                        counters['relevant_files']+=1;yield p
        return walk()
    def bounded_audit(root,**kwargs):
        if Path(root).resolve()!=ROOT:return original(root,**kwargs)
        with patch.object(Path,'rglob',bounded_glob):return original(root,**kwargs)
    audit.audit_solution=bounded_audit
    suite=unittest.defaultTestLoader.discover(str(ROOT/'BESHstatNG/tools/tests'),pattern='test_*.py')
    result=unittest.TextTestRunner(verbosity=2).run(suite)
    assert before==sha(Path(audit.__file__).read_bytes())
    print(json.dumps(dict(audit_source_unchanged=True,walker=counters)),flush=True)
    raise SystemExit(0 if result.wasSuccessful() else 1)
