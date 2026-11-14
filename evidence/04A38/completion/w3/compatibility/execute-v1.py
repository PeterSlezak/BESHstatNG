"""Adapt accepted current-only recorder recipe for exactly one configuration.

The accepted W2 Debug-only script is read verbatim and transformed only at
documented scheduling/path sites. Source and original references are untouched.
"""
from common import *
import types

lock()
config=sys.argv[1]
assert config in ['Debug','Release']
assert load(HERE/('abi-cil/'+config+'-inspection-v1.json'))['exact_exports']
authority=ACCEPTED/'compatibility_Debug_v3.py'
source=authority.read_text(encoding='utf-8')
# Use a private local module so the accepted program sees its established API,
# while all output receipts are additive W3 paths.
module=types.ModuleType('common')
module.__dict__.update(globals())
module.HERE=HERE/'compatibility'
module.PRIVATE=PRIVATE
module.DEFERRED='NOT_APPLICABLE_W3_FRESH_SEPARATE_CONFIGURATION'
module.save=lambda name,value: save('compatibility/'+name,value)
module.library_pin=lambda p:dict(name=Path(p).name,bytes=Path(p).stat().st_size,sha256=sha(Path(p).read_bytes()))
module.clean=lambda value:value
module.command=lambda label,args,**kwargs:command('compat-'+label,args,**kwargs)
sys.modules['common']=module
changes=[("assert config=='Debug'", "assert config in ['Debug','Release']"),
         ("for config in ['Debug']:", "for config in ["+repr(config)+"]:"),
         ("for c in ['Debug']", "for c in ["+repr(config)+"]"),
         ("run=PRIVATE/stage/config", "run=PRIVATE/config"),
         ("print('Current Debug preedit28966 + separate late458 EXACT; Release deferred',stage)",
          "print('W3 current-only frozen29424 EXACT',stage)")]
for before,after in changes:
    assert source.count(before)==1,(before,source.count(before))
    source=source.replace(before,after)
output=HERE/'compatibility'/('recorder-driver-'+config+'-v1.py')
with output.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('compatibility/driver-crosswalk-'+config+'-v1.json',dict(authority=pin(authority),driver=pin(output),
    exact_substitutions=[dict(before=a,after=b) for a,b in changes],
    assertions_changed=0,scope='Scheduling and additive paths only; existing exact byte comparison, recorder source, native-safe current harness, original inputs and outputs remain unchanged.'))
sys.argv=[str(output),'w3-'+config+'-v1']
exec(compile(source,str(output),'exec'),dict(__name__='__main__',__file__=str(output)))
lock()
