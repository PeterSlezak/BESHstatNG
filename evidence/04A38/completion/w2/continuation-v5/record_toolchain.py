from common import *
records=[]
for name,args in [('Python',[sys.executable,'--version']),('dotnet',['dotnet','--version']),('Node',['node','--version']),('VB compiler',[compiler('vbc.exe'),'/version'])]:
    label='final-toolchain-'+name.replace(' ','-')
    r=command(label,args);assert r['exit_code']==0
    output=(RUNROOT/'commands'/('w2c5-'+label)/'stdout.log').read_text(encoding='utf-8-sig')
    records.append(dict(tool=name,command=clean(r),output=clean(output)))
save('toolchain-final.json',dict(records=records,frameworks='netstandard2.0 Core/Common; .NET Framework4.8 Windows/tests, unchanged',
    Python='Owner-configured executable; no package/network/install operation',
    canonical_wrappers='NOT_RUN: inspected scripts/verify-beshstatng.ps1 implicit restore/shared TestResults/Legacy packaging; explicit isolated offline build/test equivalents used'))
