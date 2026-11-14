from common import *
records=[]
for label,args in [('python',[sys.executable,'--version']),('dotnet',['dotnet','--info']),('node',['node','--version']),('csc',[compiler('csc.exe'),'/version'])]:
 r=command('toolchain-'+label,args);assert r['exit_code']==0
 stdout=(RUNROOT/'commands'/('w2c3-toolchain-'+label)/'stdout.log').read_text(encoding='utf-8-sig');records.append(dict(tool=label,receipt=clean(r),output=clean(stdout)))
save('toolchain.json',dict(records=records,targets='Unchanged:netstandard2.0 portable;.NETFramework4.8 Windows',restore='NOT_RUN',wrapper='scripts/verify-beshstatng.ps1 inspected; offline explicit lanes used because implicit restore/shared outputs/Legacy packaging are not authorized'))
# Independent actual IL edge list, including native callbacks and canonical spill.
out=[]
for c in ['Debug','Release']:
 data=json.loads((HERE/'metadata-v6/parametric-v4'/('current-'+c+'.json')).read_bytes());edges=[]
 for method in data['methods']:
  if not method['declaring_type'].startswith(('BESHStatNG.WorksheetFunctions.ParametricUDFs','BESHStatNG.UdfDataImport','BESHStatNG.UdfOutputTables','BESHStatNG.UdfLinearAlgebra')):continue
  for i in method['instructions']:
   if i['op'] in ['call','callvirt','newobj','ldftn'] and isinstance(i['operand'],str) and any(s in i['operand'] for s in ['UdfNamePreparation38','UdfVariableNameRange38','UdfSpillPreparation38','ParametricUdfReports38','UdfNumericPreparation38']):edges.append(dict(caller=method['method'],opcode=i['op'],target=i['operand'],offset=i.get('offset')))
 need_name=any('FillExplicitNames' in r['target'] for r in edges) and any('ResolveDetectedNames' in r['target'] for r in edges) and any('GetVariableNames' in r['target'] for r in edges)
 assert need_name and any('StackWithBlankRow' in r['target'] for r in edges) and any('NormalizeBlanks' in r['target'] for r in edges)
 out.append(dict(configuration=c,metadata=pin(HERE/'metadata-v6/parametric-v4'/('current-'+c+'.json')),edges=edges))
save('production-consumer-edges.json',dict(configurations=out,proof_scope='Actual freshly compiled native instructions; full-body proof separately verify_names.py; original metadata inspection only'))
