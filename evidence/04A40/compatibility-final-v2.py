"""Authenticated current-only recorder recipe; sealed originals are read, never executed."""
from common40_v3 import *
import types
lock();config=sys.argv[1]
assert load(HERE/('abi-cil/'+config+'-proof-v2.json'))['exports']==268
target=PRIVATE/'compatibility-final2';target.mkdir(exist_ok=True)
with (target/'production-test-freeze-v1.json').open('x' if not (target/'production-test-freeze-v1.json').exists() else 'r',encoding='utf-8') as f:
    if f.writable():json.dump(load(HERE/'matrix/source-freeze-v2.json'),f)
accepted=ROOT/'evidence/04A38/completion/w2/continuation-v17-postcorrection-v1/compatibility_Debug_v3.py'
source=accepted.read_text(encoding='utf-8')
edits=[("assert config=='Debug'","assert config in ['Debug','Release']"),("for config in ['Debug']:","for config in ["+repr(config)+"]:"),
 ("for c in ['Debug']","for c in ["+repr(config)+"]"),("run=PRIVATE/stage/config","run=PRIVATE/config"),
 ("print('Current Debug preedit28966 + separate late458 EXACT; Release deferred',stage)","print('40 current-only frozen29424 EXACT',stage)")]
for a,b in edits:assert source.count(a)==1;source=source.replace(a,b)
module40=types.ModuleType('common');module40.__dict__.update(globals());module40.HERE=target;module40.PRIVATE=PRIVATE/'final2';module40.DEFERRED='NOT_APPLICABLE40_SEPARATE_CONFIG'
module40.save=lambda name,value:save(target/name,value)
module40.clean=lambda value:value
module40.library_pin=lambda path:dict(name=Path(path).name,bytes=Path(path).stat().st_size,sha256=pin(path)['sha256'])
def compiler(name):
    locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
module40.compiler=compiler
sys.modules['common']=module40
driver=PRIVATE/('compatibility-driver-'+config+'-v2.py')
with driver.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save(PRIVATE/('compatibility-driver-'+config+'-crosswalk-v2.json'),dict(authority=pin(accepted),driver=pin(driver),substitutions=edits,assertions_changed=0,original_queries=0))
sys.argv=[str(driver),'a40-'+config+'-v2']
exec(compile(source,str(driver),'exec'),dict(__name__='__main__',__file__=str(driver)))
