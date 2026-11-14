import platform
from common import *
records=[];runner=m.Runner(RUNROOT)
for label,args in [('dotnet',['dotnet','--info']),('node',['node','--version']),('msbuild',[runner.msbuild,'-nologo','-version'])]:
 r=command('toolchain-'+label,args);assert r['exit_code']==0
 stdout=(RUNROOT/'commands'/('w2c1-toolchain-'+label)/'stdout.log').read_text(encoding='utf-8-sig')
 records.append(dict(tool=label,command=clean(r),stdout=clean(stdout)))
save('toolchain-current.json',dict(Python=platform.python_version(),commands=records,compiler=[dict(tool=n,bytes=Path(compiler(n)).stat().st_size,sha256=sha(Path(compiler(n)).read_bytes())) for n in ['vbc.exe','csc.exe']],framework_targets_unchanged=['netstandard2.0','net48'],restore_install_network='NOT_RUN'))
