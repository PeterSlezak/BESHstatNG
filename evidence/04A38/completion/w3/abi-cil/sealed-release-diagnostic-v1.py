from common import *
a=load(W2/'continuation-v12/metadata-all/packet-v1-context-v5/original-Release.json')
b=load(PRIVATE/'Release/metadata/current.json')
method=next(r['method'] for r in b['methods'] if 'DefaultErrorReporter @' in r['method'] and '.LogAndThrow(' in r['method'])
x=next(r for r in a['methods'] if r['method']==method);y=next(r for r in b['methods'] if r['method']==method)
diff={k:dict(before=x.get(k),after=y.get(k)) for k in x.keys()|y.keys() if x.get(k)!=y.get(k)}
save('abi-cil/sealed-release-diagnostic-v1.json',dict(method=method,keys=list(diff),differences=diff))
print('Changed metadata keys',list(diff),flush=True)
print('Instruction lengths',len(x['instructions']),len(y['instructions']),flush=True)
