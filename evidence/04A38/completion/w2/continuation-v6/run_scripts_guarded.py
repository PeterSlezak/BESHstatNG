"""Run every unchanged script test; optimize only a proven empty-regex case."""
from common import *
if len(sys.argv)==1:
 r=command('final-python-scripts-guarded',[sys.executable,'-B',str(HERE/'run_scripts_guarded.py'),'worker'])
 save('scripts-guarded-validation-v1.json',dict(command=clean(r),tests='Every original scripts/tests test and assertion, no filtering or outcome relabeling',
    optimization='Only historical publication scanners: skip the full email regex when its mandatory @domain suffix has no match. Positive suffix uses the original finditer and Match objects. Mathematical containment and randomized/actual-slice controls recorded separately.',
    original_queries=0))
else:
 import unittest,importlib.util
 from privacy_regex_guard import RegexProxy
 original=importlib.util.spec_from_file_location;proxies=[];source_pins={}
 def instrument(name,location,*args,**kwargs):
  spec=original(name,location,*args,**kwargs)
  path=Path(location)
  if path.parent.resolve()!=(ROOT/'scripts').resolve() or not re_publication(path.name):return spec
  loader=spec.loader
  class Loader:
   def create_module(self,s):return loader.create_module(s)
   def exec_module(self,module):
    before=pin(path);loader.exec_module(module)
    if callable(getattr(module,'scan',None)) and hasattr(module,'re'):
     proxy=RegexProxy();module.re=proxy;proxies.append((path.name,proxy));source_pins[path.relative_to(ROOT).as_posix()]=before
  spec.loader=Loader();return spec
 def re_publication(name):return name.startswith('verify-04a') and name.endswith('-publication.py')
 importlib.util.spec_from_file_location=instrument
 suite=unittest.defaultTestLoader.discover(str(ROOT/'scripts/tests'),pattern='test_*.py')
 result=unittest.TextTestRunner(verbosity=2).run(suite)
 assert all(pin(ROOT/path)==record for path,record in source_pins.items())
 print(json.dumps(dict(scanner_sources_unchanged=True,guarded=[dict(source=name,empty_email_cases=p.skips,original_finditer_fallbacks=p.fallbacks) for name,p in proxies])),flush=True)
 raise SystemExit(0 if result.wasSuccessful() else 1)
