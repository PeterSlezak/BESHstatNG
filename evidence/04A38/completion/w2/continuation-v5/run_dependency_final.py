from common import *
exe=PRIVATE/'dependency-metadata-v7/MethodMetadata38.exe'
records=[]
for config in ['Debug','Release']:
    assert json.loads((HERE/('build-cells-v4-'+config+'.json')).read_bytes())['all_exit_zero']
    dst=HERE/'dependency-metadata-v8'/('current-'+config+'.json');dst.parent.mkdir(exist_ok=True)
    r=command('dependency-metadata-v8-current-'+config,[str(exe),str(PRIVATE/'cells-v4'/config/'lib'),str(dst),str(HERE/'native-metadata-owners-v7.txt')])
    records.append(clean(r));assert r['exit_code']==0
save('dependency-metadata-v8.json',dict(commands=records,original='Exact immutable v7 passive originals reused; actual DLL hashes rechecked by final reader',original_queries=0))
