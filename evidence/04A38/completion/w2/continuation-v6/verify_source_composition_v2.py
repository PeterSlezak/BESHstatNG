"""Independently reconstruct moved operations and preserved reset/API phases."""
import json,re
from pathlib import Path
from verify_checkpoint_v2 import ROOT,HERE,PREFIX,need,tokens,normalize,semantic
def member(source,name,kind='Function'):
 source=source.replace('\r\n','\n')
 m=re.search(r'^([ \t]*)(?:Public|Friend|Private)(?: Shared)? '+kind+' '+name+r'\b',source,re.M);need(m is not None,'Missing complete portable declaration '+name)
 end=source.index('\n'+m[1]+'End '+kind,m.end())+len('\n'+m[1]+'End '+kind)
 return source[m.start():end]
def block(code,kind='Function'):
 code=code.replace('\r\n','\n');lines=code.splitlines()
 start=next(i for i,line in enumerate(lines) if (') As ' in line if kind=='Function' else line.rstrip().endswith(')')))
 return '\n'.join(lines[start+1:-1])
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {}
 def raw(p):return overrides[p] if p in overrides else (root/p).read_bytes()
 def text(p):return raw(p).decode('utf-8-sig').replace('\r\n','\n')
 def load(p):return json.loads(raw(p))
 contracts={c['name']:c for c in load(PREFIX+'whole-body-contracts-v1.json')['contracts']}
 numeric=text('BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb')
 inference=text('BESHStat.Excel.Common/src/Results/UdfInferencePreparation38.vb')
 results=[]
 def equal(name,a,b):need(tokens(a)==tokens(b),'Source composition differs '+name);results.append(name)
 for parent,helper in [('TTEST_UNPAIRED_NI','MeanNonInferiorityBody'),('TTEST_UNPAIRED_EQUIV','MeanEquivalenceBody')]:
  original=contracts[parent]['original_whole_body'].replace('\r\n','\n');a=original.index('Dim body As Object(,) = {');b=original.index('\n\n                Return BuildResultTable',a)
  current=block(member(inference,helper));current=current[:current.rindex('Return body')]
  current=current.replace('UdfOutputTablesReports38.SafeCiLabel','SafeCiLabel').replace('UdfOutputTablesReports38.SafeCiText','SafeCiText')
  equal(parent+' entire literal/report initializer',original[a:b],current)
 for original_name,helper,count in [('KW_STAT','KruskalGroupNames','data.Length'),('FRIEDMAN_STAT','FriedmanGroupNames','k')]:
  source=contracts[original_name]['original_whole_body'].replace('\r\n','\n');a=source.index('Dim names(');b=source.index('\n\n',a)
  moved=block(member(inference,helper));moved=moved[:moved.rindex('Return names')];moved=re.sub(r'\bcount\b',count,moved)
  equal(original_name+' name allocation/format/order',source[a:b],moved)
 original=contracts['SPEARMAN_RHO']['original_whole_body'].replace('\r\n','\n');a=original.index('Dim n As Integer = pairs.GetLength(0)');b=original.index('\n\n            Dim test',a)
 old=original[a:b].replace('Dim x(n - 1) As Double','x = New Double(n - 1) {}').replace('Dim y(n - 1) As Double','y = New Double(n - 1) {}')
 equal('Original pair-column allocation/row ordering',old,block(member(inference,'SplitCorrelationColumns','Sub'),'Sub').replace('For i As Integer', 'For i'))
 original=contracts['TryReadIndependentNumericColumns']['original_whole_body'].replace('\r\n','\n');a=original.index('Dim gx As New List');b=original.rindex('\n    End Function')
 moved=block(member(numeric,'TryReadIndependentNumericColumnsPrepared'))
 for before,after in [('rowsX','ax.GetLength(0)'),('rowsY','ay.GetLength(0)'),('numericX(r)','TryGetDouble(ax(r, 0))'),('numericY(r)','TryGetDouble(ay(r, 0))'),('textX()','Convert.ToString(ax(0, 0)).Trim()'),('textY()','Convert.ToString(ay(0, 0)).Trim()')]:moved=moved.replace(before,after)
 equal('Independent complete numeric/name/ByRef phase',original[a:b],moved)
 original=contracts['TryGetOptionalProbabilityThresholds']['original_whole_body'].replace('\r\n','\n');a=original.index('If rows <> 1 AndAlso cols <> 1');b=original.rindex('\n    End Function')
 moved=block(member(numeric,'TryReadProbabilityThresholdsPrepared')).replace('appendCell(0, j, values)','TryAppendProbabilityThreshold(arr(0, j), values)').replace('appendCell(i, 0, values)','TryAppendProbabilityThreshold(arr(i, 0), values)')
 equal('Probability full shape/scan/sort/adjacent-input tolerance phase',original[a:b],moved)
 plan=load(PREFIX+'implementation-plan.json');pre=next(r for r in plan['source_before'] if r['live']['path']=='BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb')
 before=text(pre['preimage']['path'])
 for name in ['ExtractCompleteNumericMatrixCompleteCases','TryReadCompleteNumericMatrixWithHeaders']:
  wrapper=block(member(numeric,name));prepared=block(member(numeric,name+'Prepared'))
  expanded=re.sub(r'(?m)^\s*Return '+name+r'Prepared\([^\n]*\)\s*$',lambda m:prepared,wrapper)
  equal(name+' existing public API reset/body inlining',block(member(before,name)),expanded)
 # Complete current CIL must preserve all former operations after removing only
 # the original explicit entry reset block, with branch/EH indices rebased.
 il=[]
 for config in ['Debug','Release']:
  old=load('evidence/04A38/completion/w2/continuation-v5/dependency-metadata-v9/current-'+config+'.json')
  now=load(PREFIX+'metadata-full/families-v2/current-'+config+'.json')
  for name,args in [('TryReadCompleteNumericMatrixWithHeaders',[4,5]),('ExtractCompleteNumericMatrixCompleteCases',[3,4,5])]:
   owner='BESHStatNG.ExcelCommon.UdfNumericImportPreparation38'
   a=normalize(next(r for r in old['methods'] if r['declaring_type']==owner and r['member_name']==name))
   b=next(r for r in now['methods'] if r['declaring_type']==owner and r['member_name']==name+'Prepared')
   ins=a['instructions'];matches=[]
   def arg(i,number):return i['op']=='ldarg.'+str(number) or (i['op'] in ['ldarg','ldarg.s'] and i['operand']==number)
   for start in range(min(30,len(ins))):
    good=True
    for index,number in enumerate(args):
     p=start+index*3;null=name.startswith('TryRead') or index==0
     good=good and arg(ins[p],number) and ins[p+1]['op']==('ldnull' if null else 'ldc.i4.0') and ins[p+2]['op']==('stind.ref' if null else 'stind.i4')
    if good:matches.append(start)
   need(len(matches)==1,'No unique original reset block')
   start=matches[0];end=start+len(args)*3
   def rebase(n):
    need(not start<=n<end,'Branch/EH enters removed reset block');return n-(end-start) if n>=end else n
   revised=[]
   for index,i in enumerate(ins):
    if start<=index<end:continue
    j=dict(i)
    if isinstance(j['operand'],list):j['operand']=[rebase(n) for n in j['operand']]
    revised.append(j)
   comparison=semantic(a);comparison['instructions']=revised;comparison['method']=b['method'];comparison['member_name']=b['member_name']
   for exception in comparison['exceptions']:
    for key in ['try_start','try_end','handler_start','handler_end','filter_start']:
     if exception.get(key) is not None:exception[key]=rebase(exception[key])
   need(comparison==semantic(b),'Prepared complete CIL/exception context differs')
   il.append(dict(configuration=config,method=name,removed_reset_instructions=end-start,other_operations='EXACT',EH='EXACT_REBASED'))
 return dict(status='SOURCE_AND_COMPLETE_PREPARED_CIL_COMPOSITION_PASS',source_checks=results,prepared_CIL=il,original_queries=0)
if __name__=='__main__':print(json.dumps(inspect()))
