"""Independent operation-level proof of this bounded source slice, not READY.

No producer/statistical implementation is imported. Original source, snapshots,
actual compiled call operands and unchanged frozen results are checked here.
"""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;STAGE='p1-v5'
PREFIX='BESHstatNG/src/ExcelUDFs/'
PROFILE='BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb'
NUMERIC='BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb'
REPORT='BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb'
def need(v,m):
 if not v:raise ValueError(m)
def load(p):return json.loads(p.read_bytes())
def digest(b):return hashlib.sha256(b).hexdigest()
def tokens(s):return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',s) if not x.startswith("'")]
def member(s,name,kind='Function'):
 m=re.search(r'^([ \t]*)(?:Public|Private|Friend) (?:Shared )?'+kind+' '+name+r'\b',s,re.M);need(m,'Missing member '+name)
 end=s.index('\n'+m[1]+'End '+kind,m.start())+len('\n'+m[1]+'End '+kind)
 return s[m.start():end]
def body(s,kind='Function'):
 # These signatures finish at the first line with a terminal ) or return type.
 lines=s.splitlines();start=next(i for i,l in enumerate(lines) if (re.search(r'\) As [\w.(), ?]+$',l) if kind=='Function' else l.rstrip().endswith(')')))
 return '\n'.join(lines[start+1:-1])
def inspect(root=ROOT):
 root=Path(root);here=root/HERE.relative_to(ROOT);trans=load(here/('source-transitions-'+STAGE+'.json'))
 def current(p):
  row=next(r for r in trans['files'] if r['after']['path']==p);raw=(root/row['snapshot']['path']).read_bytes()
  need((len(raw),digest(raw))==(row['after']['bytes'],row['after']['sha256']),'Source snapshot changed '+p)
  need((root/p).read_bytes()==raw,'Live source differs from compiled slice '+p)
  return raw.decode('utf-8-sig').replace('\r\n','\n')
 before_records=load(here/'source-before-p1.json')['files']+load(here/'source-before-p1-reports.json')['files']
 def before(p):
  row=next(r for r in before_records if r['live']['path']==p);raw=(root/row['preimage']['path']).read_bytes()
  need((len(raw),digest(raw))==(row['live']['bytes'],row['live']['sha256']),'Source preimage changed '+p)
  return raw.decode('utf-8-sig').replace('\r\n','\n')
 originals=load(root/'evidence/04A38/preedit/entry-source-snapshot.json')['files']
 def original(p):
  # Archived originals are authenticated by their pre-edit snapshot ledger.
  row=next(r for r in originals if r['source']['path']==p);record=row['snapshot']
  raw=(root/record['path']).read_bytes();need((len(raw),digest(raw))==(record['bytes'],record['sha256']),'Original snapshot changed '+p)
  return raw.decode('utf-8-sig').replace('\r\n','\n')
 matrix=load(root/'evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members'];proofs=[]
 def proved(p,name,target,reason,category='SOURCE_OPERATIONS_PORTABLE_CONSUMED'):
  rows=[r for r in matrix if r['path']==p and r['name']==name];need(len(rows)==1,'Ambiguous historical identity '+name)
  proofs.append(dict(original_id=rows[0]['canonical_original_id'],name=name,path=p,portable_target=target,disposition=category,proof=reason))
 portable=current(PROFILE)
 for family,path,oldapply,oldhas in [('Gee',PREFIX+'Regression/GeeUDFs.vb','ApplyGeeLsmEstimateAtProfile','GeeComponentHasProfileColumn'),('Mmrm',PREFIX+'Regression/MixedModelUDFs.vb','ApplyMmrmLsmEstimateAtProfile','ComponentHasProfileColumn')]:
  old=before(path);now=current(path)
  for name,kind in [(oldapply,'Sub'),(oldhas,'Function')]:
   need(tokens(member(original(path),name,kind))==tokens(member(old,name,kind)),'Original operation changed before P1 '+name)
  expected=body(member(old,oldapply,'Sub'),'Sub')
  for a,b in [(family+'LsmEstimateComponent','TComponent'),(family+'LsmEstimateProfileValue','TValue'),(oldhas+'(',family+'ComponentHasProfileColumn(Of TComponent, TValue)(')]:expected=expected.replace(a,b)
  need(tokens(expected)==tokens(body(member(portable,'Apply'+family+'AtProfile','Sub'),'Sub')),'Apply operation/exception/order changed '+family)
  expected=body(member(old,oldhas)).replace(family+'LsmEstimateProfileValue','TValue')
  need(tokens(expected)==tokens(body(member(portable,family+'ComponentHasProfileColumn'))),'Contains operation changed '+family)
  wrapper=body(member(now,oldapply,'Sub'),'Sub')
  need(tokens(wrapper)==tokens('Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38.Apply'+family+'AtProfile(Of '+family+'LsmEstimateComponent, '+family+'LsmEstimateAtProfile, '+family+'LsmEstimateProfileValue)(components, atProfile)'),'Wrong native apply wiring')
  need(not re.search(r'Private Function '+oldhas+r'\b',now),'Ornamental native contains body retained')
  # Native TValue construction has no subclass/foreign constructor substitution.
  native_value=re.search(r'(?:Private|Friend) Class '+family+r'LsmEstimateProfileValue\b([\s\S]*?)End Class',now)
  need(native_value and 'Inherits Global.BESHStatNG.ExcelCommon.UdfLsmProfileValue38' in native_value[1] and len(native_value[1].splitlines())<=3,'Native TValue initializer/constructor changed')
  proved(path,oldapply,'UdfModelProfilePreparation38.Apply'+family+'AtProfile','Exact statements/branches/setter order; native trivial TValue ctor; copy only absent index; no overwritten explicit setting')
  proved(path,oldhas,'UdfModelProfilePreparation38.'+family+'ComponentHasProfileColumn','Exact null/iteration/index equality operations; called by production portable apply; native helper relocated')
 hostpath=PREFIX+'UDFdataImport/UdfDataImport.vb';old=before(hostpath);now=current(hostpath);numeric=current(NUMERIC)
 for name in ['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']:
  need(tokens(member(original(hostpath),name))==tokens(member(old,name)),'Original import preimage changed '+name)
  expected=body(member(old,name))
  if name=='TryGetGroupedCompleteMatrix':
   expected=expected[:expected.index('Dim dataArr As Object(,)')]+expected[expected.index('If dataArr Is Nothing'):]
   expected=expected.replace('LooksLikeHeaderRow(dataArr, dataColIndexes)','header(dataArr, dataColIndexes)').replace('Convert.ToString(groupArr(startGroup + i, 0)).Trim()','labelText(groupArr(startGroup + i, 0))').replace('RowsToMatrix(','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.RowsToMatrix(')
   need('Dim dataArr As Object(,) = Get2D(data)' in now and 'Dim groupArr As Object(,) = Get2D(groups)' in now,'Original acquisition order missing')
   need('dataArr, groupArr, AddressOf LooksLikeHeaderRow, Function(cell) Convert.ToString(cell).Trim(),' in now,'Grouped native policies changed')
  else:
   expected=expected[:expected.index('Dim arr As Object(,)')]+expected[expected.index('If arr Is Nothing'):]
   expected=re.sub(r'Convert.ToString\((arr\([^\n]*?\))\).Trim\(\)',r'labelText(\1)',expected)
   expected=expected.replace('g <> "" AndAlso sg <> ""','Not LegacyUdfText38.EqualText(g, "") AndAlso Not LegacyUdfText38.EqualText(sg, "")')
   need('arr, Function(cell) Convert.ToString(cell).Trim(),' in now,'Nested native label policy changed')
  expected=expected.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble','numeric')
  need(tokens(expected)==tokens(body(member(numeric,name))),'Import operation/partial ByRef/order changed '+name)
  proved(hostpath,name,'UdfNumericImportPreparation38.'+name,'Get2D remains native; exact callback and reset order; stable source row traversal; group finite gate; nested nullable gate with Option Compare Text adapter')
 # Prove the shared header dependency instead of closing grouped import on an edge.
 ranges=PREFIX+'UDFdataImport/UdfDataImportRangeReaders.vb';raw=(root/ranges).read_text(encoding='utf-8-sig').replace('\r\n','\n');base=original(ranges)
 oldheader=body(member(base,'LooksLikeHeaderRow')).replace('arr.GetLength(0)','rows').replace('TryGetDouble(arr(0, c)).HasValue','numeric(0, c)').replace('TryGetDouble(arr(r, c)).HasValue','numeric(r, c)')
 sharedrange=(root/'BESHStat.Excel.Common/src/Analysis/UdfRangePreparation38.vb').read_text(encoding='utf-8-sig').replace('\r\n','\n')
 need(tokens(oldheader)==tokens(body(member(sharedrange,'LooksLikeHeaderRow'))),'Header dependency changed')
 need(tokens(body(member(raw,'LooksLikeHeaderRow')))==tokens('Return Global.BESHStatNG.ExcelCommon.UdfRangePreparation38.LooksLikeHeaderRow(arr.GetLength(0), numericCols, Function(row, column) TryGetDouble(arr(row, column)).HasValue)'),'Header native cell callback changed')
 proved(ranges,'LooksLikeHeaderRow','UdfRangePreparation38.LooksLikeHeaderRow','Full original predicate and short circuit/order match, existing host nullable reader and original cases')
 need(tokens(member(base,'Get2D'))==tokens(member(raw,'Get2D')),'Get2D host behavior changed')
 proved(ranges,'Get2D',None,'Unchanged source; ExcelMissing/Empty/reference coercion callback and catch boundary; accepted WindowsSpreadsheetTransport (pinned entry dependency); managed rectangular routes exercised; native callback NOT_RUN','HOST_ONLY_WITH_BOUNDARY_PROOF')
 report=current(REPORT);plot=PREFIX+'PlotDataUDFs.vb';old=before(plot);now=current(plot)
 for name in ['BuildCalibrationPointsTable','BuildRocPointsTable','BuildRocStatsTable']:
  need(tokens(member(original(plot),name))==tokens(member(old,name)),'Original report preimage changed '+name)
  expected=body(member(old,name))
  if name=='BuildCalibrationPointsTable':
   expected=expected.replace('ExcelError.ExcelErrorNA','missingIntervalValue').replace('Return PrepareResultTableForUdf(out)','Return New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(out), UdfReportPreparation38.NormalizeBlanks)')
   need('BuildCalibrationPointsTable(rows, CDbl(ExcelError.ExcelErrorNA))' in now,'Lost original Enum/Double numeric coercion')
  else:
   expected=expected.replace('TryGetFiniteDoubleFlexible(thresholdObj, threshold)','tryFinite(thresholdObj, threshold)').replace('Return PreparePValueRowsForUdf(out)','Return out')
   expected=expected.replace('Return BuildSimpleNoteTable(', 'Return UdfOutputTablesReports38.BuildSimpleNoteTable(')
   for msg in ['No ROC cut-off table was returned.','No ROC summary table was returned.']:expected=expected.replace('"'+msg+'")','"'+msg+'").ToOutputModel().Values')
  need(tokens(expected)==tokens(body(member(report,name))),'Report operation/literal/type/order changed '+name)
  proved(plot,name,'PlotDataUDFsReports38.'+name,'Original row/cell/header/order/exception operations; Core bins reused; original numeric missing interval conversion at host; ROC native threshold reader and pvalue adapter preserved')
 for config in ['Debug','Release']:
  meta=load(here/'inspection'/STAGE/('Inspect38-'+config+'.json'));oldmeta=load(root/'evidence/04A38/preedit'/('compiled-'+config+'.json'))
  key=lambda r:r['effective_excel_name'];need(len(meta['exports'])==268 and sorted(meta['exports'],key=key)==sorted(oldmeta['exports'],key=key),'268 export ABI/attributes/defaults changed')
  for p in proofs:
   if p['name'].endswith('ComponentHasProfileColumn'):continue
   method=next((m for m in meta['methods'] if m['name']==p['name'] and m['declaring_type'].replace('+','.') in [p['original_id'].split('|')[1]]),None)
   need(method,'Actual compiled shipping method missing '+p['name'])
   if p['portable_target']:
    need(any('.'+p['portable_target']+'(' in c['target'] for c in method['calls']),'Actual compiled portable edge missing '+p['name'])
  full=load(here/'inspection'/STAGE/('MethodBodies38-'+config+'.json'))
  for name in ['ApplyGeeAtProfile','ApplyMmrmAtProfile','GeeComponentHasProfileColumn','MmrmComponentHasProfileColumn','TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData','BuildCalibrationPointsTable','BuildRocPointsTable','BuildRocStatsTable']:
   matches=[r for r in full if ('.ExcelCommon.' in r['method'] and '.'+name+'(' in r['method'])]
   need(matches and all(not r['type_initializer'] and not r['nonliteral_static_fields'] for r in matches),'Portable hidden state/missing full IL '+name)
  for family in ['Gee','Mmrm']:
   apply=next(r for r in full if '.UdfModelProfilePreparation38.Apply'+family+'AtProfile(' in r['method'])
   need(any(isinstance(i['operand'],str) and family+'ComponentHasProfileColumn(' in i['operand'] for i in apply['instructions']),'Orphan copied membership helper')
  focus=load(here/('focused-'+STAGE+'-'+config+'.json'));need(all(r['command']['exit_code']==0 for r in focus['results']),'Focused test nonpass')
  need([r['tests']['outcomes'] for r in focus['results'] if r['kind']=='TEST']==[{'Passed':51},{'Passed':28}],'Focused test identity/cardinality changed')
 compat=load(here/('compatibility-'+STAGE+'.json'))
 for row in compat['results']:
  old=(root/row['original']['path']).read_bytes();new=(root/row['current']['path']).read_bytes()
  need(old==new and digest(old)==row['original']['sha256'] and digest(new)==row['current']['sha256'],'Frozen result/type/error/ByRef mismatch')
 need(compat['totals_per_configuration']=={'Debug':29424,'Release':29424} and compat['new_original_queries']==0,'New original or missing frozen case')
 return dict(status='BOUNDED_P1_SOURCE_OPERATIONS_AND_COMPILED_CONSUMERS_PASS_NOT_BATCH_READY',proofs=proofs,proof_supported_historical_additions=len(proofs),configurations=['Debug','Release'],focused_tests_per_configuration=79,compatibility_per_configuration=dict(genuine_preedit=28966,late_A_to_G=458),new_original_queries=0,W2='IN_PROGRESS',W3='NOT_STARTED')
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except (ValueError,OSError,KeyError,TypeError,StopIteration) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
