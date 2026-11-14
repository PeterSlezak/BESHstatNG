"""Independent C4 operation/context/actual-CIL compatibility reader. NOT READY."""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
def need(v,m):
 if not v:raise ValueError(m)
def sha(b):return hashlib.sha256(b).hexdigest()
def tokens(s):return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',s) if not x.startswith("'")]
def member(s,n):
 m=re.search(r'^([ \t]*)(?:Public|Private|Friend) (?:Shared )?Function '+n+r'\b',s,re.M);need(m,'Member absent '+n);end=s.index('\n'+m[1]+'End Function',m.end());return s[m.start():end+len('\n'+m[1]+'End Function')]
def body(s):
 lines=s.splitlines();i=next(i for i,l in enumerate(lines) if ') As ' in l and not l.rstrip().endswith(','));return '\n'.join(lines[i+1:-1])
def semantic(m):return {k:v for k,v in m.items() if k!='raw_IL_sha256'}
def operations(m):return {k:m[k] for k in ['member_name','parameter_types','implementation_flags','locals','init_locals','exceptions','instructions']}
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def raw(p):return overrides[p] if p in overrides else (root/p).read_bytes()
 def load(p):return json.loads(raw(p))
 def text(p):return raw(p).decode('utf-8-sig').replace('\r\n','\n')
 entry=load(base+'entry.json');trans=load(base+'source-transitions.json')
 for r in entry['source_before']:need((len(raw(r['preimage']['path'])),sha(raw(r['preimage']['path'])))==(r['live']['bytes'],r['live']['sha256']),'Bad current preimage')
 for r in trans['files']:need(raw(r['path'])==raw(r['snapshot']['path']) and sha(raw(r['path']))==r['after']['sha256'],'Source successor differs from built slice')
 for r in load(base+'accepted-Core-dependency-pins.json')['files']:need(sha(raw(r['current']['path']))==r['current']['sha256'],'Existing Core dependency changed')
 originalrows=load('evidence/04A38/preedit/entry-source-snapshot.json')['files']
 def original(p):
  r=next(r for r in originalrows if r['source']['path']==p)['snapshot'];b=raw(r['path']);need((len(b),sha(b))==(r['bytes'],r['sha256']),'Original snapshot changed');return b.decode('utf-8-sig').replace('\r\n','\n')
 history=load('evidence/04A38/completion/w2/continuation-v3/historical-ownership.json')['members'];proofs=[];records={}
 def closed(path,name,reason,configurations):
  h=next(r for r in history if r['path']==path and r['name']==name);need(not h['proof'],'Reclosed inherited proof');proofs.append(dict(original_id=h['canonical_original_id'],path=path,name=name,reason=reason,configurations=configurations))
 def lookup(ms,owner,name):
  rows=[m for m in ms if m['declaring_type']==owner and m['member_name']==name];need(len(rows)==1,'Ambiguous/missing method '+owner+'.'+name);return rows[0]
 for c in ['Debug','Release']:
  o=load(base+'metadata-v6/parametric-v4/original-'+c+'.json');n=load(base+'metadata-v6/parametric-v4/current-'+c+'.json');records[c]=(o,n)
  # Fresh compiled public ABI, including attributes/defaults and declaration types.
  a=load('evidence/04A38/preedit/compiled-'+c+'.json')['exports'];b=load(base+'inspection/parametric-v4/Inspect38-'+c+'.json')['exports'];key=lambda r:r['effective_excel_name'];need(len(b)==268 and sorted(a,key=key)==sorted(b,key=key),'268 compiled exports changed')
 # Complete inversion source composition, without broadening original Try boundary.
 path='BESHstatNG/src/ExcelUDFs/UdfLinearAlgebra.vb';core='BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb';a=body(member(original(path),'TryInvertMatrix'))
 split=re.search(r'^\s*Try\s*$',a,re.M);catch=re.search(r'^\s*Catch\s*$',a,re.M);need(split and catch,'Original catch scope absent')
 prefix=a[:split.start()].replace('        inv = Nothing\n','').strip();prefix=re.sub(r'Dim (nRows|nCols) As Integer =',r'\1 =',prefix)
 shape=body(member(text(core),'TryGetSquareMatrixDimensions'));need(tokens(prefix+'\nReturn True')==tokens(shape),'Shape acquisition/guard sequence changed')
 kernel=a[split.end():catch.start()]
 kernel=re.sub(r'(?:Global\.)?(?:BESHStatNG\.)?Matrix\.Matrix\.MatInv','Global.BESHStatNG.Matrix.MatrixDecompositionCore.InvertMatrix',kernel)
 need(tokens(kernel)==tokens(body(member(text(core),'TryInvertPreparedSquareMatrix'))),'Engine/iErr/partial-output/shape operation changed')
 expected='''inv = Nothing
 Dim nRows As Integer = 0
 Dim nCols As Integer = 0
 If Not Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TryGetSquareMatrixDimensions(a, nRows, nCols) Then Return False
 Try
 Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TryInvertPreparedSquareMatrix(a, nRows, nCols, inv)
 Catch
 inv = Nothing
 Return False
 End Try'''
 need(tokens(body(member(text(path),'TryInvertMatrix')))==tokens(expected),'Native reset/real-catch sequencing changed')
 inversion=[]
 for c,(o,n) in records.items():
  old=lookup(o['methods'],'BESHStatNG.UdfLinearAlgebra','TryInvertMatrix');now=lookup(n['methods'],'BESHStatNG.UdfLinearAlgebra','TryInvertMatrix');k=lookup(n['methods'],'BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38','TryInvertPreparedSquareMatrix')
  need(len(old['exceptions'])==len(now['exceptions'])==1 and not k['exceptions'],'Catch boundary lost or swallowed by portable kernel')
  def vb(m):return [i['operand'] for i in m['instructions'] if isinstance(i['operand'],str) and 'Microsoft.VisualBasic.CompilerServices.ProjectData @ Microsoft.VisualBasic,' in i['operand']]
  need(vb(old)==vb(now) and len(vb(now))==2,'Real Framework VB catch helpers differ')
  eh=now['exceptions'][0];shapeCall=next(i for i in now['instructions'] if isinstance(i['operand'],str) and '.TryGetSquareMatrixDimensions(' in i['operand']);kernelCall=next(i for i in now['instructions'] if isinstance(i['operand'],str) and '.TryInvertPreparedSquareMatrix(' in i['operand']);need(now['instructions'].index(shapeCall)<eh['try_start']<=now['instructions'].index(kernelCall)<eh['try_end'],'Catch improperly includes validation or excludes kernel')
  facade=lookup(o['methods'],'BESHStatNG.Matrix.Matrix','MatInv');engineCalls=[i['operand'] for i in facade['instructions'] if i['op']=='call' and 'MatrixDecompositionCore' in str(i['operand'])];need(len(engineCalls)==1 and engineCalls[0] in [i['operand'] for i in k['instructions']],'Original forwarding engine target changed')
  embedded=[m for m in n['methods'] if m['declaring_type']=='Microsoft.VisualBasic.CompilerServices.ProjectData' and m['member_name'] in ['SetProjectError','ClearProjectError']];need(len(embedded)==2 and all([i['op'] for i in m['instructions']]==['ret'] for m in embedded),'Embedded-runtime premise changed')
  before=load(base+'inversion-before-'+c+'.json');after=load(base+'inversion-parametric-v4-'+c+'.json');need(len(before['cases'])==len(after['cases'])==8,'Missing current-only inversion observation')
  for left,right in zip(before['cases'],after['cases']):
   need({k:v for k,v in left.items() if k!='after'}=={k:v for k,v in right.items() if k!='after'},'Inversion output/bits/alias/exception regression')
   caught=right['name'] in ['singular','ill-conditioned','nonzero-bounds'];need(right['after']==after['reference_catch_after'] if caught else right['after']==right['before'],'Err branch contract changed')
  inversion.append(dict(configuration=c,real_Framework_catch=True,validation_outside_catch=True,current_only_cases=8,unchanged_result_bits=True,exception_state_fix='Actual native catch clears only on original caught paths'))
 runtime=load(base+'framework-runtime-metadata.json');pd=next(r for r in runtime['types'] if r['type']=='Microsoft.VisualBasic.CompilerServices.ProjectData');need(any(r['name']=='m_oProject' and r['thread_static'] for r in pd['fields']),'Thread-local Err state not authenticated');need(all(len(m['instructions'])>1 for m in pd['methods'] if m and m['member_name'] in ['SetProjectError','ClearProjectError']),'Framework runtime incorrectly no-op')
 closed(path,'TryInvertMatrix','Original prefix/kernel/real Windows catch composed; runtime metadata and current-only Err/ByRef/control probe; unchanged forwarding engine and actual Cox caller',inversion)
 # Numeric bodies: recompute eligible set from original/current complete operations,
 # static/initializer context and actual native consumers; no asserted candidate list.
 numeric=[]
 for h in history:
  if h['proof']:continue
  configs=[]
  for c,(o,n) in records.items():
   aa=[m for m in o['methods'] if m['member_name']==h['name'] and m['declaring_type'] in h['canonical_original_id']];bb=[m for m in n['methods'] if m['member_name']==h['name'] and m['declaring_type']=='BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38']
   if len(aa)!=1 or len(bb)!=1:break
   a,b=aa[0],bb[0];current=next((m for m in n['methods'] if m['method']==a['method']),None)
   if current is None or operations(a)!=operations(b) or b['exceptions'] or b['type_initializer'] or b['nonliteral_static_fields']:break
   ot=next(t for t in o['types'] if t['type']==a['declaring_type']);nt=next(t for t in n['types'] if t['type']==a['declaring_type'])
   if ot!=nt:break
   nm={m['method']:m for m in n['methods']};ctors=[m for m in o['methods'] if m['declaring_type']==a['declaring_type'] and m['member_name'] in ['.ctor','.cctor']]
   if any(m['method'] not in nm or operations(m)!=operations(nm[m['method']]) for m in ctors):break
   if 'METHOD:'+b['method'] not in [i['operand'] for i in current['instructions'] if i['op'] in ['call','callvirt']]:break
   configs.append(dict(configuration=c,source_method=a['method'],portable_method=b['method'],whole_resolved_body=True,native_fields_and_initializers_exact=True))
  if len(configs)==2:numeric.append(h['name']);closed(h['path'],h['name'],'Whole instructions/locals/EH/params and actual consumer; native context exact; transitive existing Core source/project dependencies pinned',configs)
 need(set(numeric)=={'GetCategoriesInModelOrder','ComputeMultinomialEtas','ComputeOrdinalProbabilities','ComputeLinearPredictor'},'Numeric closure cohort changed')
 # All30 explicitly bounded options: complete alias/default/return/throw source,
 # primitive boxed copying, actual callee and exact native cache initialization.
 inventory=load(base+'option-body-inventory.json')['rows'];need(len(inventory)==30 and len({r['original_id'] for r in inventory})==30,'Option cohort missing/duplicate')
 need(sum(r['path'].endswith('MultivariateAnalysisUDFs.vb') for r in inventory)==21 and sum(r['path'].endswith('UdfDataImportAgreement.vb') for r in inventory)==8,'Wrong option family scope')
 optpath='BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb'
 for r in inventory:
  orig=body(member(original(r['path']),r['name']));portable=body(member(text(optpath),r['name']));choices=[]
  for var,items in re.findall(r'Select Case LegacyUdfText38.CaseToken\((\w+), (?:New String\(\) )?\{([^\n]*)\}\)',portable):
   choices+=re.findall(r'"((?:""|[^"])*)"',items);portable=portable.replace('Select Case LegacyUdfText38.CaseToken('+var+', New String() {'+items+'})','Select Case '+var).replace('Select Case LegacyUdfText38.CaseToken('+var+', {'+items+'})','Select Case '+var)
  portable=re.sub(r'LegacyUdfText38.EqualText\((\w+), ("(?:""|[^"])*")\)',r'\1 = \2',portable)
  need(tokens(orig)==tokens(portable) and choices==r['choices'],'Option alias/return/default/throw body altered')
  need(re.search(r'^\s*Dim \w+ As String = NormalizeToken\('+r['first_argument']+r'\)',orig),'Option conversion order not justified')
  wrapper=body(member(text(r['path']),r['name']));need(len(wrapper.splitlines())==1 and 'AsString('+r['first_argument']+')' in wrapper and r['common_owner'].split('.')[-1]+'.'+r['name'] in wrapper,'Native option boundary changed')
  configs=[]
  for c,(o,n) in records.items():
   before=lookup(o['methods'],r['native_owner'],r['name']);now=lookup(n['methods'],r['native_owner'],r['name']);callee=lookup(n['methods'],r['common_owner'],r['name']);need(before['parameter_types']==now['parameter_types'] and not now['exceptions'] and not callee['exceptions'] and not callee['type_initializer'] and not callee['nonliteral_static_fields'],'Option parameter/EH/static contract changed')
   calls=[i['operand'] for i in now['instructions'] if i['op']=='call'];need(len(calls)==3 and '.GetObjectValue(' in calls[0] and '.AsString(' in calls[1] and calls[2]=='METHOD:'+callee['method'],'Option conversion/callee CIL sequence changed')
   ot=next(t for t in o['types'] if t['type']==r['native_owner']);nt=next(t for t in n['types'] if t['type']==r['native_owner']);need(ot==nt,'Original native option cache fields changed')
   nm={m['method']:m for m in n['methods']}
   for ctor in [m for m in o['methods'] if m['declaring_type']==r['native_owner'] and m['member_name'] in ['.ctor','.cctor']]:need(operations(ctor)==operations(nm[ctor['method']]),'Option cache initializer changed')
   configs.append(dict(configuration=c,actual_callee=callee['method'],native_field_count=len(ot['fields']),initializer_preserved=True))
  closed(r['path'],r['name'],'Whole source decision/default/throw composition; C3 proven normalizers; fresh fixed-RHS/runtime option/boxed-conversion probe; real consumer and native cache fields/initializers',configs)
 for c in ['Debug','Release']:
  p=load(base+'option-probe-v3-'+c+'.json');need(p['classification']=='CURRENT_ONLY_NOT_ARCHIVED_REFERENCE' and p['original_calls']==0 and p['differences']==0 and p['comparisons']==len(p['cases'])==3604,'Option probe missing/changed observations')
  rows=p['cases'];need(len({(r['method'],r['culture'],r['id']) for r in rows})==len(rows) and {(r['method'],r['culture']) for r in rows}=={(r['name'],culture) for r in inventory for culture in ['Invariant','sk-SK']},'Option/culture missing or duplicate')
  for row in rows:
   a={k:v for k,v in row['native'].items() if k!='reflection_wrapper'};b={k:v for k,v in row['control'].items() if k!='reflection_wrapper'};need(row['exact'] and a==b,'Current native/control option boundary drift')
 # Report service source mapping and precise old/new host ordering are separate
 # implementation progress: public bodies remain OPEN on import/output dependencies.
 native='BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb';report='BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb';before=text(base+'source-before/'+native+'.txt');now=text(native)
 for name in ['ANOVA1','ANOVA1_WELCH','ANOVA1_MCP']:need(tokens(member(original(native),name))==tokens(member(before,name)),'ANOVA preimage not authentic original body')
 classical='''Dim mdl As New parametric.OneWayANOVA(data, names)
 mdl.compute()
 Dim tables = mdl.wrapResults()
 Return PrepareResultTableForUdf(tables(0))'''
 need(tokens(body(member(text(report),'BuildOneWayAnovaTable')))==tokens(classical.replace('parametric.OneWayANOVA','Global.BESHStatNG.parametric.OneWayANOVA').replace('Return PrepareResultTableForUdf(tables(0))','Return tables(0)')),'Classical engine/order/selected table changed')
 oldWelch=body(member(before,'ANOVA1_WELCH'));start=oldWelch.index('Dim mdl As New parametric.OneWayANOVA');end=oldWelch.index('Return PrepareResultTableForUdf(t)')+len('Return PrepareResultTableForUdf(t)');fragment=oldWelch[start:end]
 need(tokens(body(member(text(report),'BuildWelchAnovaTable')))==tokens(fragment.replace('parametric.OneWayANOVA','Global.BESHStatNG.parametric.OneWayANOVA').replace('New ResultTable','New Global.BESHStatNG.ResultTable').replace('Return PrepareResultTableForUdf(t)','Return t')),'Welch compute/Welsh/types/headers/pvalue metadata changed')
 need('BuildOneWayAnovaTable(data, names)' in now and 'BuildWelchAnovaTable(data, names)' in now,'Unused ANOVA service')
 mcp=body(member(text(report),'BuildOneWayMcpTables'));need(mcp.index('mdl.compute()')<mcp.index('Select Case') and mcp.count('Return mdl.wrapResults()')==1,'MCP early invalid selection or duplicated wrapping')
 allpart=mcp[mcp.index('Case "ALL", "BOTH"'):mcp.index('Case Else',mcp.index('Case "ALL", "BOTH"'))];calls=re.findall(r'mdl\.(FisherLSD|TukeyKramer|GamesHowell)\(([^\n]*)\)',allpart);need(calls==[('FisherLSD','False, alphaValue'),('FisherLSD','True, alphaValue'),('TukeyKramer','alphaValue'),('GamesHowell','alphaValue')],'MCP ALL numerical order changed')
 host=body(member(now,'ANOVA1_MCP'));need(host.index('ResolveNames(')<host.index('TryParseAlpha(')<host.index('NormalizeText(method)')<host.index('BuildOneWayMcpTables('),'MCP host conversion/alpha/model order changed')
 need(host.count('PrepareResultTableForUdf(stacked)')==1 and 'stacked = StackWithBlankRow(stacked, PrepareResultTableForUdf(tables(i)))' in host,'MCP sequential render/stack/last normalize changed')
 for c,(o,n) in records.items():
  # Accepted engines and ResultTable implementations are independently unchanged
  # at complete resolved-CIL/field/constructor level, not just class names.
  nm={m['method']:m for m in n['methods']}
  engines=[m for m in o['methods'] if m['declaring_type'].startswith(('BESHStatNG.parametric.Parametric+OneWayANOVA','BESHStatNG.Matrix.MatrixDecompositionCore','BESHStatNG.ResultTable'))]
  need(engines,'Missing accepted engine/context metadata')
  for m in engines:need(m['method'] in nm and semantic(m)==semantic(nm[m['method']]),'Accepted engine/ResultTable method changed')
  for name,target in [('ANOVA1','BuildOneWayAnovaTable'),('ANOVA1_WELCH','BuildWelchAnovaTable'),('ANOVA1_MCP','BuildOneWayMcpTables')]:
   m=lookup(n['methods'],'BESHStatNG.WorksheetFunctions.ParametricUDFs',name);t=lookup(n['methods'],'BESHStatNG.ExcelCommon.ParametricUdfReports38',target);need('METHOD:'+t['method'] in [i['operand'] for i in m['instructions']] and not t['exceptions'],'Missing actual native report consumer or new swallowed error')
  focused=load(base+'focused-parametric-v4-'+c+'.json');need(all(r['command']['exit_code']==0 for r in focused['results']),'Focused failure')
 compat=load(base+'compatibility-parametric-v4.json');need(compat['new_original_queries']==0 and compat['totals_per_configuration']==dict(Debug=29424,Release=29424),'Extra/missing original authority/case count')
 for r in compat['results']:need(raw(r['original']['path'])==raw(r['current']['path']) and sha(raw(r['original']['path']))==r['original']['sha256'],'Frozen compatibility byte difference')
 need(len(proofs)==35,'Unsupported source closure count')
 return dict(status='INDEPENDENT_C4_SLICE_PROOFS_PASS_NOT_READY',proofs=proofs,new_supported_bodies=35,report_consumers_implemented_but_public_bodies_OPEN=['ANOVA1','ANOVA1_WELCH','ANOVA1_MCP'],report_blockers=['TryGetGroupedNumericColumns/TryReadGroupedNumericColumns/primitive host converter dependency closure','metadata-bearing PrepareResultTableForUdf and per-cell formatter complete ownership','MCP alpha parser dependency'],original_queries=0,scientific='NOT_GOLDEN')
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
