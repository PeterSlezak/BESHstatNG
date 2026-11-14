"""Add independent host, callback-slot, fragment and denominator reconstruction."""
from common import *
source=HERE/'verify_c13_checkpoint_template_v1.py';v=source.read_text()
v=v.replace(" shapes={r['symbol_id']", " shapes={r['symbol_id']",1)
v=v.replace("  retention.append(dict(configuration=config,reached=730,exports=268,changed_native=8,added=17))",'''  content=lambda s:'\\n'.join(line.strip() for line in s.splitlines()[1:] if line.strip())
  need(content(p['original_normalizer_source'])==content(p['current_normalizer_source']),'Portable callback normalizer source mismatch')
  a=p['original_callback'];b=p['current_callback'];left=p['original_normalizer']['method'];right=p['current_normalizer']['method']
  need([dict(i,operand='METHOD:'+right) if i.get('operand')=='METHOD:'+left else i for i in a['instructions']]==b['instructions'] and all(a[k]==b[k] for k in ['parameter_types','locals','exceptions']),'Portable callback full CIL normalization mismatch')
  need(a['method'] in [i['operand'][7:] for i in od[p['original_parent']]['instructions'] if i['op']=='ldftn'] and b['method'] in [i['operand'][7:] for i in defs[p['current_parent']]['instructions'] if i['op']=='ldftn'],'Portable historical callback actual construction missing')
  for r in load(PREFIX+'callback-identity-context-v1.json')['rows']:
   if r['configuration']!=config:continue
   need(r['complete_callback']==defs[r['actual_ldftn']] and r['actual_ldftn'] in [i['operand'][7:] for i in defs[r['current_parent']]['instructions'] if i['op']=='ldftn'],'Separate KM callback mismatch')
   method=defs[r['portable_method']['method']];index=r['portable_select_instruction'];need(method==r['portable_method'] and any(i['op']=='ldarg.'+str(r['parameter_slot']) for i in method['instructions'][max(0,index-5):index]),'Wrong KM count/fill selector argument slot')
  for owner,suffix in [('BESHStatNG.ExcelCommon.UdfPlotFiniteDoubleReader38','System.Double&'),('BESHStatNG.ExcelCommon.UdfPlotStatusReader38','System.Int32&')]:
   methods=[m for m in meta['methods'] if m['declaring_type']==owner];need(len(methods)==4 and all(m['implementation_flags']=='CodeTypeMask' and not m['instructions'] and not m['exceptions'] for m in methods),'CLR delegate implementation boundary changed')
   invoke=next(m for m in methods if m['member_name']=='Invoke');need(invoke['parameter_types']==['System.Object',suffix] and any(r['declaring_type']==owner and r['member_name']=='Invoke' and r['return_type']=='System.Boolean' for r in ret),'Typed ByRef delegate return ABI changed')
  retention.append(dict(configuration=config,reached=730,exports=268,changed_native=8,added=17))''')
v=v.replace(" need(composition(root,overrides)",''' from verify_c13_host_contexts_v1 import inspect as host_check
 host_check(load,raw,PREFIX,o,u,{c:load(PREFIX+'metadata-all/packet-v1-context-v5/current-'+c+'.json') for c in ['Debug','Release']})
 need(composition(root,overrides)''')
v=v.replace(" for population,universe,rows in",''' def actual_headers(universe):
  found={};keys={(s['path'],s['kind'],s['owner'],s['declaration']) for s in cur['support'] if s['fully_reconciled']}
  for s in universe['supports']:
   if (s['path'],s['kind'],s['owner'],s['declaration']) not in keys:continue
   if s['kind']=='ModuleBlock':kinds=['ModuleStatement','EndModuleStatement']
   elif s['kind']=='ClassBlock' and s['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs' and s['declaration'].startswith('Private Class '):kinds=['ClassStatement','EndClassStatement']
   elif s['kind']=='PropertyStatement' and s['owner'].startswith('BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs.'):kinds=['PropertyStatement']
   else:continue
   for f in universe['fragments']:
    if f['member_id'] is None and f['path']==s['path'] and f['kind'] in kinds and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:found[f['fragment_id']]=s['support_id']
  return found
 need(headers==dict(current=actual_headers(u),original=actual_headers(o)),'Support fragment/header authority forged')
 measured_fragments={}
 for population,universe,rows in''')
v=v.replace(" cross=load(PREFIX+'new-parent",'''  predicate=lambda f:f['member_id'] in (closed if population=='current' else hclosed) or f['fragment_id'] in headers[population]
  measured_fragments[population]=dict(total=len(rows),covered=sum(predicate(f) for f in rows),branches=sum(f['is_branch'] for f in rows),branches_covered=sum(f['is_branch'] and predicate(f) for f in rows),inline=sum(f['is_inline'] for f in rows),inline_covered=sum(f['is_inline'] and predicate(f) for f in rows))
 cross=load(PREFIX+'new-parent''')
v=v.replace(" need(len(load(PREFIX+'new-closures-final", " need(summary['fragments']==measured_fragments,'Fragment denominator/coverage forged')\n need(len(load(PREFIX+'new-closures-final")
with (HERE/'verify_c13_checkpoint_template_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
compile(v,'verify_c13_checkpoint_template_v2.py','exec')
save('independent-reader-source-crosswalk-v1.json',dict(first=pin(source),final_template=pin(HERE/'verify_c13_checkpoint_template_v2.py'),
    scope='Actual predecessor replay plus complete current source/metadata/dependency/body/callback/return ABI, seven host handles/eight copies, KM separate selector slots, original-only Common callback, explicit runtime delegates, universes and fragment denominator reconstruction. Original DLL/source never executed.',original_queries=0))
