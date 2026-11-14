from common import *
from proof_kernel_v6 import definitions
rows=[]
for config in ['Debug','Release']:
 meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes());defs=definitions(meta)
 roots=[r['original_parent']['method'] for r in json.loads((HERE/'original-km-callback-bindings-v1.json').read_bytes())['rows'] if r['configuration']==config] if False else []
 for name in ['BuildKaplanMeierStepTable','GuessPositiveClass']:
  p=next(m for m in meta['methods'] if m['member_name']==name and m['declaring_type'] in ['BESHStatNG.WorksheetFunctions.PlotDataUDFs','BESHStatNG.UdfDataImport'])
  targets=[defs[i['operand'][7:]] for i in p['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs and defs[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
  ctors=[i for i in p['instructions'] if i['op']=='newobj' and 'System.Func`2' in (i.get('operand') or '')]
  assert len(targets)==len(ctors)==(2 if name=='BuildKaplanMeierStepTable' else 1)
  rows.append(dict(configuration=config,original_parent=p,original_generated_callbacks=targets,actual_closed_generic_delegate_constructors=ctors,
   authority='Existing archived complete CIL. Closed Func(Of Object, SurvivalTableRecord) and Func(Of String, String) constructions fix original parameter/return ABI. No original lambda reflection return row exists; none is invented. Current actual return metadata is independently measured.'))
save('original-callback-delegate-return-abi-v1.json',dict(rows=rows,original_queries=0))
