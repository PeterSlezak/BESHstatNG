"""Exact inherited binder plus authenticated nested types inside generic parameters.

The prior binder misses List(Of PrivateNestedType) because the constructed
runtime List is not an internal declared type. No overload/ByRef/rank checks
are removed. The fallback requires the exact source-owner nested declaration.
"""
import sys,re
from pathlib import Path
sys.path.append(str(Path(__file__).resolve().parent.parent/'continuation-v15'))
from proof_kernel_v8 import *
from proof_kernel_v8 import bind as inherited_bind
from proof_kernel_v8 import map_current as inherited_map_current

def map_current(before,after):
 getters=[n for n in before if n['kind']=='GetAccessorBlock' and n['owner']=='BESHStatNG.WorksheetFunctions.LmmUDFs.LmmImportedInputs.N' and n['name']=='Get']
 need(len(getters)==1,'Exact selected LMM Get source identity missing')
 getter=getters[0]
 result=inherited_map_current([n for n in before if n is not getter],after)
 parent=result[getter['parent_member']]
 candidates=[n for n in after if n['kind']=='GetAccessorBlock' and n['signature_key']==getter['signature_key'] and n['parent_member']==parent['symbol_id']]
 need(len(candidates)==1,'Ambiguous selected N/Get source movement')
 result[getter['symbol_id']]=candidates[0]
 need(len(result)==len(before)==len(after) and len({n['symbol_id'] for n in result.values()})==len(result),'Changed current source denominator or overlapping identities')
 # The caller must additionally prove the exact complete N/Get Common macro
 # and its actual Debug getter/auto-property writer context. No lambda body
 # change is admitted; all other children retain inherited hash assertions.
 return result

def bind(node,meta,shapes):
 parameters=shapes[node['symbol_id']]['parameters'] or []
 declared={t['type'] for t in meta['types'] if t['type']}
 known=NESTED_CACHE_DECLARATIONS.get(node['owner'],set())
 def exact_owner(result):
  for source,actual in zip(parameters,result['parameter_types']):
   for name in known:
    if re.search(r'(?<![\w.])'+re.escape(name)+r'(?!\w)',source.get('type') or ''):
     expected=node['owner']+'+'+name
     need(expected in declared and re.search(re.escape(expected)+r'(?![\w+])',actual) is not None,'Source-owner nested parameter mismatch '+node['symbol_id'])
  return result
 try:return exact_owner(inherited_bind(node,meta,shapes))
 except ValueError as original_error:
  shape=shapes[node['symbol_id']];parameters=shape['parameters'] or []
  declared={t['type'] for t in meta['types'] if t['type']}
  nested={t.rsplit('+',1)[-1]:t for t in declared if t.startswith(node['owner']+'+') and t.count('+')==1}
  def match(source,actual):
   if parameter_matches(source,actual) or nested_parameter_matches(node,source,actual,meta):return True
   used={name:full for name,full in nested.items() if re.search(r'(?<![\w.])'+re.escape(name)+r'(?!\w)',source.get('type') or '')}
   if not used:return False
   if not all(full in actual for full in used.values()):return False
   references=re.findall(r'[A-Za-z_][A-Za-z0-9_.$`]*(?:\+[A-Za-z0-9_.$`]+)+',actual)
   if not references or not all(t in declared for t in references):return False
   return parameter_matches(source,actual.replace('+','.'))
  candidates=[r for r in meta['methods'] if (r['declaring_type'],r['member_name'])==(node['owner'],node['name']) and len(r['parameter_types'])==len(parameters) and all(match(p,a) for p,a in zip(parameters,r['parameter_types']))]
  need(len(candidates)==1,'Missing/ambiguous exact source-owner nested generic binding '+node['symbol_id']+'; inherited: '+str(original_error))
  return exact_owner(candidates[0])
