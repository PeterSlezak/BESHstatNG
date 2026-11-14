from common import *
from verify_preservation import inspect
r=command('independent-preservation-final',[sys.executable,'-B',str(HERE/'verify_preservation.py')]);assert r['exit_code']==0
save('preservation.json',inspect())
import platform
records=[];runner=m.Runner(RUNROOT)
for label,args in [('dotnet',['dotnet','--info']),('node',['node','--version']),('msbuild',[runner.msbuild,'-nologo','-version'])]:
 r=command('toolchain-'+label,args);assert r['exit_code']==0
 records.append(dict(tool=label,command=clean(r),stdout=clean((RUNROOT/'commands'/('w2c2-toolchain-'+label)/'stdout.log').read_text(encoding='utf-8-sig'))))
save('toolchain.json',dict(Python=platform.python_version(),commands=records,compiler=[dict(tool=n,bytes=Path(compiler(n)).stat().st_size,sha256=sha(Path(compiler(n)).read_bytes())) for n in ['vbc.exe','csc.exe']],targets=['netstandard2.0','net48'],restore_install='NOT_RUN'))
