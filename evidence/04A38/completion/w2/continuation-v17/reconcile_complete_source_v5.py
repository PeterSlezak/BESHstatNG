"""Recompute source composition independently of proposed ownership flags."""
from proof_support_v17_v5 import *
u=load(FINAL/'current-universe.json');common=load(FINAL/'dependency-full-common.json');core=load(FINAL/'dependency-full-core.json')
api={(n['owner'].split('.')[-1],n['name']):n for n in common['members']+core['members'] if n['parent_member'] is None and n['kind'] in ['FunctionBlock','SubBlock']}
def code(owner,name):
 n=api[owner,name];return body((ROOT/n['path']).read_bytes(),n)[0]
entry=load(HERE/'entry-queue-receipt-v1.json')
move_names=['packet0-source-transitions-v1.json','shared-packets-source-moves-v1.json','remaining-packets-source-moves-v1.json','mmrm-zip-source-moves-v1.json','formula-mmrm-tail-source-moves-v1.json','response-import-source-moves-v1.json']
moves=[dict(m,receipt=name) for name in move_names for m in load(HERE/name)['moves']]
corrections=load(HERE/'response-alias-preservation-v1.json')['changes']
files=[]
for pre in entry['source_preimages']:
 path=pre['live']['path'];expected=normalize((ROOT/pre['preimage']['path']).read_bytes().decode('utf-8-sig'))
 used=[]
 for m in moves:
  if m['path']!=path:continue
  old=normalize(m['original_segment']);new=normalize(m.get('native_call',m.get('current_segment','')))
  assert expected.count(old)==1,(path,m.get('host'),m['receipt']);expected=expected.replace(old,new,1);used.append(m['receipt'])
 expected=expected.replace('ComputeSurvivalMedianCi(timeArr,','ComputeSurvivalMedianCi(nRows, timeArr,').replace('ComputeSurvivalLogRank(timeArr,','ComputeSurvivalLogRank(nRows, timeArr,').replace('BuildOrdinalClassification(cls, cats, k, hdr)','BuildOrdinalClassification(cls, k, hdr)')
 for r in corrections:
  if r['path']==path:
   for p in r['replacements']:
    assert expected.count(p['before'])==1;expected=expected.replace(p['before'],p['after'],1)
 fix=load(HERE/'intercept-alias-correction-v1.json')
 if path==fix['path']:
  p=fix['replacement'];assert expected.count(normalize(p['before']))==1;expected=expected.replace(normalize(p['before']),normalize(p['after']),1)
 actual=normalize((ROOT/path).read_bytes().decode('utf-8-sig'))
 assert expected==actual,'Whole native source reconstruction failed '+path
 files.append(dict(path=path,preimage=pre['preimage'],current=pin(ROOT/path),moves=used,entire_source_reconstructed=True,normalization='CR discarded for source comparison only; original raw before/after files remain separately SHA pinned. No production EOL rewriting.'))
common0=next(m for m in moves if m['path']=='BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb')
pre0=next(r for r in load(HERE/'packet0-source-transitions-v1.json')['before'] if r['path']==common0['path'])
expected=(ROOT/pre0['preimage']['path']).read_text(encoding='utf-8-sig');assert expected.count(common0['original_segment'])==1
same(expected.replace(common0['original_segment'],common0['current_segment'],1),(ROOT/common0['path']).read_text(encoding='utf-8-sig'),'Full existing Common options file')
portable=[]
textfix=load(HERE/'portable-text-conversion-preservation-v1.json')['transformations']
selectorfix=load(HERE/'text-policy-integration-correction-v2.json')['changes']
for m in moves:
 p=m.get('portable')
 if not p:continue
 name=p['call'].split('(',1)[0] if p.get('existing') else re.search(r'\b(?:Function|Sub)\s+(\w+)',p['signature'])[1]
 expected=p['body']
 if p.get('suffix'):expected+='\n'+p['suffix']
 if p.get('control_structure_out'):expected=expected.replace('Dim control As Global.BESHStatNG.regression.MixedModelControl = req.Control','control = req.Control')
 expected=re.sub(r'(?<!BESHStatNG\.)\bregression\.', 'Global.BESHStatNG.regression.',expected)
 if name in ['GetReferenceCategory','ParseSurvivalWeightText']:
  authoritative=next(r for r in textfix if r.get('method')==name)['before']
  same(expected,phase(authoritative+'\n        End Function'),'Recorded selector preimage '+name)
  expected=phase(next(r for r in selectorfix if r['method']==name)['after']+'\n        End Function')
 else:
  for r in textfix:
   if 'method' not in r:expected=expected.replace(r['before'],r['after'])
 if name=='ComputeSurvivalMedianCi':expected=expected.replace(', unavailable, CType(', ', CType(unavailable, Object), CType(')
 if name=='ReadIntegerOutcomes':
  expected=expected.replace('For i As Integer = 0 To rowCount - 1','For i As Integer = 0 To data.nRows - 1').replace('convertDouble(matrix(i, 0))','convertDouble(data.FinalData(i, 0))')
 same(expected,phase(code(p['owner'],name)),'Entire portable phase '+name)
 portable.append(dict(native_path=m['path'],native_parent=m['host'],portable_source=api[p['owner'],name],receipt=m['receipt'],all_statements_reconciled=True,aliases=m.get('aliases',[]),policy_details=p))

rows=[];unresolved=[]
for r in entry['rows']:
 a=r['original_record'];candidates=r['current_candidates']
 if len(candidates)!=1:
  rows.append(dict(original_id=r['original_id'],mode='DISTINCT_PROPERTY_OR_CALLBACK_SOURCE_AND_DEBUG_BINDING_REQUIRED',mechanical_parent_proof=False));continue
 c=candidates[0];original=r['original_whole_body'];current=c['whole_body'];name=a['name']
 try:
  if tokens(original)==tokens(current):mode='EXACT_ENTIRE_ORIGINAL_ENTRY_BODY'
  elif name=='AddInterceptIfRequested':
   same(phase(original).replace('If Not includeIntercept Then Return x','If Not includeIntercept Then Return DirectCast(x.Clone(), Double(,))'),phase(code('UdfNumericPreparation38',name)),'Entire Core with explicit copy-contract difference isolated');same(phase(current),'Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.AddInterceptIfRequested(x, includeIntercept)','Native '+name);mode='ENTIRE_UNCHANGED_CORE_BODY_AND_EXACT_NATIVE_FORWARDER'
  elif name=='AverageFittedDesignRowForLsmProfile':
   guard='If h Is Nothing OrElse h.FittedDesign Is Nothing OrElse component Is Nothing Then Return Nothing'
   suffix=phase(original).split(guard,1)[1]
   aliases=[('h.FittedDesign','design'),('h.VisitValues','timeValues'),('component.VisitSpecified','timeSpecified'),('component.VisitValue','targetTime'),('component.ProfileValues','profiles'),('For Each profileValue As MmrmLsmEstimateProfileValue','For Each profile As T'),('profileValue.ColumnIndex','column(profile)'),('profileValue.Value','value(profile)'),('regression.MixedModelPostEstimation.NearlyEqual(','equal(')]
   for x,y in aliases:suffix=suffix.replace(x,y)
   same('matchedCount = 0\n'+suffix,phase(code('UdfProfilePreparation38','AverageDesign')),'MMRM complete lazy averaging')
   same(phase(current),'matchedCount = 0\n'+guard+'\nReturn Global.BESHStatNG.WorksheetCompatibility.UdfProfilePreparation38.AverageDesign(h.FittedDesign,h.VisitValues,component.VisitSpecified,component.VisitValue,component.ProfileValues,Function(profile) profile.ColumnIndex,Function(profile) profile.Value,AddressOf regression.MixedModelPostEstimation.NearlyEqual,matchedCount)','Native lazy averaging')
   mode='COMPLETE_GUARDED_GENERIC_AVERAGE_BYREF_AND_FIXED_CALLBACKS'
  elif name=='BuildResidualMatrixOutput':
   target=code('UdfRegressionOutputReports38',name)
   expected=phase(original).replace('Return ExcelError.ExcelErrorNA','Return UdfReportProjection38.FromFault(UdfReportFault38.NotAvailable)').replace('Return out','Return New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(out), UdfReportPreparation38.Raw)')
   same(expected,phase(target),name);mode='COMPLETE_TYPED_REPORT_PROJECTION_AND_NATIVE_ERROR_MATERIALIZATION'
  elif name=='NormalizeMixedModelResidualCovarianceName':
   old=phase(original).replace('Dim token As String = NormalizeMixedModelOptionToken(arg)','Dim token As String = NormalizeMixedModelOptionToken(arg)')
   before=untext(phase(common0['original_segment'])).replace('Return arg','Return ExcelArgReaders.AsString(arg)')
   same(old,before,'Original covariance grammar including fallback')
   classifier=phase(code('UdfScalarUtilitiesOptions38','TryClassifyMixedModelResidualCovarianceName'))
   pairs=re.findall(r'If (.*?) Then Return ([^\n]+)',phase(common0['original_segment']))
   assert len(pairs)==10
   classifier_expected='Dim token As String = NormalizeMixedModelOptionToken(arg)\n'
   for condition,value in pairs:classifier_expected+='If '+condition+' Then\ncanonical = '+value+'\nReturn True\nEnd If\n'
   classifier_expected+='canonical = Nothing\nReturn False';same(classifier_expected,classifier,'Complete classifier decision graph')
   mode='ORIGINAL_NATIVE_FIRST_AND_UNKNOWN_ONLY_SECOND_CONVERSION_PLUS_COMPLETE_PURE_CLASSIFIER'
  else:
   call=re.search(r'Global\.BESHStatNG\.ExcelCommon\.(\w+)\.(\w+)\(',current);assert call,'Missing exact Common adapter'
   owner,target=call.groups();actual=untext(phase(code(owner,target)));expected=phase(original)
   expected=expected.replace('Dim s As String = AsString(v)','Dim s As String = v').replace('UBound(out)','(out.Length - 1)')
   if name=='FindDesignColumnIndex':expected=expected.replace('NormalizeDesignColumnName(', 'MixedModelUDFsOptions38.NormalizeDesignColumnName(')
   same(expected,actual,name);mode='ENTIRE_ORIGINAL_PURE_BODY_AND_EXPLICIT_TEXT_OR_NATIVE_CONVERSION_ADAPTER'
  rows.append(dict(original_id=r['original_id'],original_record=a,current_entry=c['record'],mode=mode,mechanical_parent_proof=True))
 except (AssertionError,ValueError,KeyError) as e:unresolved.append(dict(original_id=r['original_id'],name=name,reason=str(e)))
save('source-composition-proof-v4.json',dict(status='SOURCE_COMPOSITION_COMPLETE' if not unresolved else 'PARTIAL_PROOF_WITH_OPEN_OBLIGATIONS',whole_native_files=files,existing_Common_full_file=pin(ROOT/common0['path']),portable_phases=portable,original_entry_rows=rows,unresolved=unresolved,normalization='Whitespace/comments omitted only by inherited VB token reader; all string literals/order/operators retained. Raw source and spans separately authenticated.',Release=DEFERRED,original_queries=0))
print('Whole native files',len(files),'portable phases',len(portable),'original-entry bodies',len(rows),'unresolved',len(unresolved))
for r in unresolved:print(r['name'],r['reason'])
