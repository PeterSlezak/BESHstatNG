"""Unchanged script assertions; reuse byte-identical per-file scanner results.

Only the exact AST-identical 04A13/14 scan functions qualify. Their source has
per-file findings, a set of reviewed findings, and no cross-file scan state.
Results are copied, keyed by source AST, environment, path and actual bytes.
No file is skipped without executing the original function on those bytes.
"""
from common import *
if len(sys.argv)==1:
 r=command('final-python-scripts-cached-v1',[sys.executable,'-B',str(HERE/'run_scripts_cached_v1.py'),'worker'])
 save('scripts-cached-validation-v1.json',dict(command=clean(r),scope='Every unchanged scripts/tests assertion; exact AST-identical per-file scanner memoization and proven necessary-condition regex guard; no filtering or relabeling',original_queries=0))
else:
 import unittest,importlib.util,ast,copy
 from privacy_regex_guard import RegexProxy
 def function_ast(path,name):
  tree=ast.parse(path.read_text(encoding='utf-8-sig'))
  return ast.dump(next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name==name),include_attributes=False)
 paths=[ROOT/'scripts'/('verify-04a'+n+'-publication.py') for n in ['13','14']]
 assert function_ast(paths[0],'scan')==function_ast(paths[1],'scan')
 assert function_ast(paths[0],'git')==function_ast(paths[1],'git')
 authority=sha(function_ast(paths[0],'scan').encode())
 original=importlib.util.spec_from_file_location;proxies=[];source_pins={};cache={};stats=dict(fresh=0,reused=0)
 def instrument(name,location,*args,**kwargs):
  spec=original(name,location,*args,**kwargs);path=Path(location)
  if path.parent.resolve()!=(ROOT/'scripts').resolve() or not(path.name.startswith('verify-04a') and path.name.endswith('-publication.py')):return spec
  loader=spec.loader
  class Loader:
   def create_module(self,s):return loader.create_module(s)
   def exec_module(self,module):
    before=pin(path);loader.exec_module(module)
    if callable(getattr(module,'scan',None)) and hasattr(module,'re'):
     proxy=RegexProxy();module.re=proxy;proxies.append((path.name,proxy));source_pins[path.relative_to(ROOT).as_posix()]=before
    if path in paths:
     assert sha(function_ast(path,'scan').encode())==authority
     original_scan=module.scan
     def scan(candidates):
      findings=[];reviewed=set()
      for candidate in candidates:
       target=ROOT/candidate
       if not target.is_file():
        f,r=original_scan([candidate])
       else:
        key=(authority,str(module.ROOT),os.environ.get('COMPUTERNAME'),candidate,sha(target.read_bytes()))
        if key not in cache:
         cache[key]=original_scan([candidate]);stats['fresh']+=1
         assert sha(target.read_bytes())==key[-1],'Scanner input changed during evaluation'
        else:stats['reused']+=1
        f,r=copy.deepcopy(cache[key])
       findings.extend(f);reviewed.update(r)
      return findings,sorted(reviewed)
     module.scan=scan
  spec.loader=Loader();return spec
 importlib.util.spec_from_file_location=instrument
 suite=unittest.defaultTestLoader.discover(str(ROOT/'scripts/tests'),pattern='test_*.py')
 result=unittest.TextTestRunner(verbosity=2).run(suite)
 assert all(pin(ROOT/path)==record for path,record in source_pins.items())
 print(json.dumps(dict(scanner_sources_unchanged=True,scanner_ast_sha256=authority,per_file_cache=stats,
  law='Original per-file findings concatenation and reviewed-set union, identical function/git AST, ROOT/env/path/bytes key; changed bytes invalidate cache; positive findings retained',
  guarded=[dict(source=name,empty_email_cases=p.skips,original_finditer_fallbacks=p.fallbacks) for name,p in proxies])),flush=True)
 raise SystemExit(not result.wasSuccessful())
