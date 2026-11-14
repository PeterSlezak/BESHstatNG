"""Exact existing open generic cache definition, using its complete recorded handle."""
from proof_kernel_v7 import *
import proof_kernel_v7 as prior_kernel

def bind(node,meta,shapes):
 if (node['owner'],node['name'])!=('BESHStatNG.WorksheetFunctions.UdfCacheHelpers','TryGetCachedHandle'):return prior_kernel.bind(node,meta,shapes)
 shape=shapes[node['symbol_id']]
 need(' '.join(shape['header'].split())=='Friend Function TryGetCachedHandle(Of T As Class)(handle As Object, cache As ConcurrentDictionary(Of String, T), ByRef value As T) As Boolean','Changed generic cache source declaration')
 need([(p['type'],p['byref']) for p in shape['parameters']]==[('Object',False),('ConcurrentDictionary(Of String, T)',False),('T',True)],'Changed generic cache parameter/rank constraint')
 native=next(a['name'] for a in meta['assemblies'] if a['name'].startswith('BESHStatNG,'))
 runtime=next(a['name'] for a in meta['loaded_modules'] if a['name'].startswith('mscorlib,'))
 key=node['owner']+' @ '+native+'.TryGetCachedHandle``1(System.Object @ '+runtime+',System.Collections.Concurrent.ConcurrentDictionary`2[System.String,T] @ '+runtime+',T& @ '+native+')'
 candidates=[m for m in meta['methods'] if m['method']==key]
 need(len(candidates)==1 and candidates[0]['parameter_types']==['System.Object',None,None],'Actual complete open generic CLR cache handle missing')
 return candidates[0]
