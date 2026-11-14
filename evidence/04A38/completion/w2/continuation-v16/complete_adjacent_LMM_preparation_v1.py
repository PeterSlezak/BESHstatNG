"""Exact necessary adjacent array preparation, with separately authenticated IDs."""
from common import *
import textwrap
path='BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb'
authority=json.loads((HERE/'adjacent-LMM-helper-authority-v1.json').read_bytes())
assert pin(ROOT/path)==authority['source']['live']
catalog=json.loads((HERE/'adjacent-LMM-source-before-census-v1.json').read_bytes())
source=(ROOT/path).read_text(encoding='utf-8-sig');native=source;moves=[];methods=[]
for name,signature,args in [('AddInterceptColumn','Function AddInterceptColumn(expandedX As Double(,), nRows As Integer) As Double(,)','expandedX, nRows'),('AddInterceptName','Function AddInterceptName(expandedNames As String()) As String()','expandedNames')]:
 n=next(r for r in catalog['members'] if r['name']==name and r['kind']=='FunctionBlock')
 start=native.index('        Friend Function '+name+'(');a=native.index('\n',start)+1;b=native.index('        End Function',a)
 segment=native[a:b].rstrip();phase=textwrap.dedent(segment)
 methods.append('        Public Shared '+signature+'\n'+textwrap.indent(phase,'            ')+'\n        End Function\n')
 call='            Return Global.BESHStatNG.ExcelCommon.UdfLmmInputPreparation38.'+name+'('+args+')'
 native=native[:a]+call+'\n'+native[b:]
 moves.append(dict(kind='AUTHENTICATED_ADJACENT_EXTRACTION',path=path,host=name,name=name,source_identity=n,destination='UdfLmmInputPreparation38',original_segment=segment,portable_segment=phase,portable_suffix='',portable_signature=signature,native_call=call,aliases=[]))
target=ROOT/'BESHStat.Excel.Common/src/Analysis/UdfLmmInputPreparation38.vb'
old=target.read_bytes();before=pin(target);preimage=HERE/'source-provisional/before-adjacent-LMM-completion-v1/UdfLmmInputPreparation38.vb.txt';preimage.parent.mkdir(parents=True,exist_ok=True)
with preimage.open('xb') as f:f.write(old)
content=old.decode('utf-8-sig')
oldcall='Global.BESHStatNG.regression.MixedModelFrontEndHelpers.DefaultNames'
newcall='Global.BESHStatNG.ExcelCommon.MixedModelUDFsMetadata38.DefaultNames'
assert content.count(oldcall)==1;content=content.replace(oldcall,newcall)
assert content.count('    End Class\nEnd Namespace')==1
content=content.replace('    End Class\nEnd Namespace','\n'+'\n'.join(methods)+'    End Class\nEnd Namespace')
target.write_text(content,encoding='utf-8',newline='\n')
file=ROOT/path;raw=file.read_bytes();newline='\r\n' if b'\r\n' in raw else '\n'
file.write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+native.replace('\r\n','\n').replace('\n',newline).encode())
save('adjacent-LMM-source-moves-v1.json',dict(status='NECESSARY_NONSCIENTIFIC_LMM_ARRAY_PREPARATION_EXTRACTED',moves=moves,before_common=before,common_preimage=pin(preimage),current_sources=[pin(target),pin(file)],fallback_rebinding=dict(before=oldcall,after=newcall,exact_existing_native_source=pin(HERE/'source-before'/(path+'.txt')),existing_portable_source=pin(ROOT/'BESHStat.Excel.Common/src/Analysis/UdfMetadataPreparation38.vb'),domain='Fallback reached only after ExpandedPredictorMatrix.GetLength(1)>0; existing Common positive-count allocation/loop/name/culture behavior is identical. Native AddInterceptIfRequested null/error logger and alias policy remain unchanged. Existing Common MMRM naming differences are not adopted.'),failed_build=pin(HERE/'build-combined-v2-Debug.json'),Core_changes=0,unit_test_runs=0,original_queries=0))
print('Two source-identified adjacent LMM helpers extracted; fallback reuses existing Common metadata',flush=True)
