"""Independent bounded operation/domain/CIL/reference reader. Never grants batch READY.

Does not import the source editor, comparer or evidence producer. All comparisons
below are reconstructed from authenticated source, source spans and actual IL.
"""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;STAGE='b1-v1'
PREFIX='BESHstatNG/src/ExcelUDFs/'
NUM='BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb'
SPILL='BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb'
OPT='BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb'

def need(v,m):
 if not v:raise ValueError(m)
def digest(b):return hashlib.sha256(b).hexdigest()
def tokens(s):
 # Only collapse a single-return If block to its VB single-line form;
 # condition, short-circuit operations and returned expression remain literal.
 s=re.sub(r'(?m)^(\s*If [^\n]+ Then)\n\s*(Return [^\n]+)\n\s*End If',r'\1 \2',s)
 return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',s) if not x.startswith("'")]
def member(s,n):
 m=re.search(r'^([ \t]*)(?:Public|Private|Friend) (?:Shared )?Function '+n+r'\b',s,re.M);need(m,'Missing source member '+n)
 end=s.index('\n'+m[1]+'End Function',m.end());return s[m.start():end+len('\n'+m[1]+'End Function')]
def body(s):
 lines=s.splitlines();start=next(i for i,l in enumerate(lines) if ') As ' in l and not l.rstrip().endswith(','));return '\n'.join(lines[start+1:-1])

def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()
 def raw(p):return overrides.get(p,(root/p).read_bytes())
 def text(p):return raw(p).decode('utf-8-sig').replace('\r\n','\n')
 def load(p):return json.loads(raw(p))
 def packet(n):return load(base+'/'+n)
 entry=packet('entry.json');trans=packet('source-transitions-'+STAGE+'.json')
 for r in entry['source_before']:
  b=raw(r['preimage']['path']);need(digest(b)==r['live']['sha256'] and len(b)==r['live']['bytes'],'Missing/changed preimage '+r['live']['path'])
 extra=packet('source-before-b1-addendum.json');need(digest(raw(extra['snapshot']['path']))==extra['source']['sha256'],'Additional preimage changed')
 for r in trans['files']:
  b=raw(r['snapshot']['path']);need(b==raw(r['after']['path']) and digest(b)==r['after']['sha256'],'Live/compiled source successor changed '+r['after']['path'])
 originals=load('evidence/04A38/preedit/entry-source-snapshot.json')['files']
 def original(p):
  r=next(r for r in originals if r['source']['path']==p)['snapshot'];b=raw(r['path']);need(digest(b)==r['sha256'] and len(b)==r['bytes'],'Original source changed '+p);return b.decode('utf-8-sig').replace('\r\n','\n')
 before=lambda p:text(base+'/source-before/'+p+'.txt')
 proofs=[]
 historical=load('evidence/04A38/completion/w2/continuation-v1/historical-ownership.json')['members']
 def proved(path,name,target,why):
  candidates=[r for r in historical if r['path']==path and (r['canonical_original_id'].split('|')[2]=='FunctionBlock') and re.search(r'Function '+re.escape(name)+r'\b',member(original(path),name))]
  # Bind the name through the W1 source census, never position alone.
  w1=load('evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members']
  r=next(r for r in w1 if r['path']==path and r['name']==name)
  need(any(x['canonical_original_id']==r['canonical_original_id'] for x in candidates),'Unknown original identity '+name)
  proofs.append(dict(original_id=r['canonical_original_id'],path=path,name=name,portable_target=target,reason=why))
 distribution=PREFIX+'DistributionUDFs.vb';old=original(distribution);new=text(distribution);numeric=text(NUM)
 for n in ['PRTRNG','PRTRNG_TAIL','F_PDF']:need(tokens(member(old,n))==tokens(member(before(distribution),n)),'Distribution preimage not original '+n)
 expected=body(member(old,'PRTRNG')).replace('ExcelError.ExcelErrorNum','Nothing').replace('iFault','fault').replace('Dim p As Double','Dim probability As Double').replace('IsNaN(p)','IsNaN(probability)').replace('IsInfinity(p)','IsInfinity(probability)').replace('Return p','Return probability')
 need(tokens(expected)==tokens(body(member(numeric,'PrepareStudentizedRangeCdf'))),'CDF guard/engine/fault/order mismatch')
 expected=body(member(old,'F_PDF')).replace('ExcelError.ExcelErrorNum','Nothing').replace('Dim p As Double','Dim density As Double').replace('IsNaN(p)','IsNaN(density)').replace('IsInfinity(p)','IsInfinity(density)').replace('Return p','Return density')
 need(tokens(expected)==tokens(body(member(numeric,'PrepareFDistributionDensity'))),'F density zero/NaN/engine guard mismatch')
 expected=body(member(old,'PRTRNG_TAIL'))
 a=expected.index('Dim cdfObj As Object');b=expected.index('Dim tail As Double')
 expected=expected[:a]+'''Dim cdf As Double? = PrepareStudentizedRangeCdf(q, v, r)
            If Not cdf.HasValue Then Return Nothing
            '''+expected[b:].replace('1.0 - cdf','1.0 - cdf.Value')
 need(tokens(expected)==tokens(body(member(numeric,'PrepareStudentizedRangeTail'))),'Tail order/clamping mismatch')
 for n,target,args in [('PRTRNG','PrepareStudentizedRangeCdf','q,v,r'),('PRTRNG_TAIL','PrepareStudentizedRangeTail','q,v,r'),('F_PDF','PrepareFDistributionDensity','x,df1,df2')]:
  expected='Dim result As Double? = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.'+target+'('+args+')\nIf Not result.HasValue Then Return ExcelError.ExcelErrorNum\nReturn result.Value'
  need(tokens(expected)==tokens(body(member(new,n))),'Boxed native ExcelError wrapper changed '+n)
  proved(distribution,n,'UdfNumericPreparation38.'+target,'Original validation, fault, NaN/infinity-result guard and arithmetic order; same Core engine; nullable result maps to native boxed ExcelError only at host')
 # Copy and blank handling are complete source-operation comparisons, not an edge-only closure.
 p=PREFIX+'UdfOutputTables.vb';expected=body(member(original(p),'PrepareResultTableForUdf'))
 need(tokens(expected)==tokens(body(member(text(SPILL),'NormalizeBlanks'))),'Blank projection cell/order/reference/bounds changed')
 need(tokens(body(member(text(p),'PrepareResultTableForUdf')))==tokens('Return Global.BESHStatNG.ExcelCommon.UdfSpillPreparation38.NormalizeBlanks(table)'),'Wrong native blank projection')
 proved(p,'PrepareResultTableForUdf','UdfSpillPreparation38.NormalizeBlanks','Exact TryCast, zero-based row-major copy, Nothing/DBNull conversion, same cell references and original nonzero-bound exception behavior')
 p=PREFIX+'ParametricUDFs.vb';expected=body(member(before(p),'StackWithBlankRow'))
 need(tokens(expected)==tokens(body(member(text(SPILL),'StackWithBlankRow'))),'Stack original successor operations changed')
 # Reconstruct the pre-existing layout decomposition back to original expressions.
 expected=expected.replace('Dim layout As New Global.BESHStatNG.ExcelCommon.UdfSpillStack38(rowsA, colsA, rowsB, colsB)','Dim cols As Integer = Math.Max(colsA, colsB)').replace('layout.RowCount','rowsA + 1 + rowsB').replace('layout.ColumnCount','cols').replace('layout.SecondStartRow','rowsA + 1')
 need(tokens(expected)==tokens(body(member(original(p),'StackWithBlankRow'))),'Original stack/layout decomposition mismatch')
 layout=text('BESHStat.Excel.Common/src/Results/UdfSpillLayout38.vb')
 for operation in ['RowCount = firstRows + 1 + secondRows','ColumnCount = Math.Max(firstColumns, secondColumns)','SecondStartRow = firstRows + 1']:need(operation in layout,'Stack layout dependency changed')
 need(tokens(body(member(text(p),'StackWithBlankRow')))==tokens('Return Global.BESHStatNG.ExcelCommon.UdfSpillPreparation38.StackWithBlankRow(a,b)'),'Stack native wrapper changed')
 proved(p,'StackWithBlankRow','UdfSpillPreparation38.StackWithBlankRow','Exact null identity, dimensions, checked arithmetic, original copy order, separator/padding Nothing and cell identity; pure pre-existing layout decomposition')
 # Levene reset BEFORE host conversion, including allocation failure and callback mutations.
 p=PREFIX+'AssumptionsUDFs.vb';wrapper=body(member(text(p),'TryParseLeveneCenter'))
 need(tokens(wrapper)==tokens('''useMedian=False
 title="Levene Test"
 Return Global.BESHStatNG.ExcelCommon.AssumptionsUDFsOptions38.TryParseLeveneCenterFromReader(Function() Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg),useMedian,title)'''),'Levene host reset/reader ordering changed')
 need(tokens(body(member(text(OPT),'TryParseLeveneCenterFromReader')))==tokens('''useMedian=False
 title="Levene Test"
 Dim text As String=readText()
 Dim parsedMedian As Boolean=False
 Dim parsedTitle As String=Nothing
 If Not TryParseLeveneCenter(text,parsedMedian,parsedTitle) Then Return False
 useMedian=parsedMedian
 title=parsedTitle
 Return True'''),'Levene callback/ByRef partial-output preservation changed')
 for n in ['TryParseLeveneCenter','TryParseSymmetryMethod']:
  need(tokens(member(before(OPT),n))==tokens(member(text(OPT),n)),'Existing typed parser policy changed '+n)
 need(tokens(body(member(text(p),'TryParseSymmetryMethod')))==tokens(body(member(before(p),'TryParseSymmetryMethod'))),'Symmetry exception ordering changed')
 proved(p,'TryParseLeveneCenter','AssumptionsUDFsOptions38.TryParseLeveneCenterFromReader','Actual ByRef reset before host callback/allocation; typed parser uses local outputs; invalid/throw retains callback mutations; successful branch copies actual outputs in original order; no statistical policy change')
 # Fixed-RHS caller domain, not a false claim that every normalizer removes ignorables.
 table=packet('text-call-sites.json');classified=packet('text-caller-domains.json');calls=table['calls'];need(classified['policy_change']=='NONE','Unexpected text policy patch')
 need(len(calls)==len({r['site_id'] for r in calls}),'Duplicate caller site')
 direct=[r for r in calls if r['kind']=='EqualText'];cases=[r for r in calls if r['kind']=='CaseToken'];need((len(direct),len({r['line_site_id'] for r in direct}),len(cases),sum(len(r['choices']) for r in cases))==(145,59,63,733),'Text caller coverage changed')
 need({r['site_id'] for r in classified['calls']}=={r['site_id'] for r in calls},'Missing domain classification')
 for r in calls:
  source=raw(r['path']);need(digest(source)==r['source_sha256'],'Stale caller source '+r['path'])
  snippet=source.decode('utf-8-sig').encode('utf-16-le')[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le')
  need(r['right_expression'] in snippet,'Caller span/right expression forged')
  values=[r['right_literal']] if r['kind']=='EqualText' else r['choices']
  need(all(isinstance(v,str) and all(32<=ord(c)<=126 for c in v) for v in values),'Reachable null/ignorable/dynamic RHS')
  if r['kind']=='CaseToken':
   decoded=re.findall(r'"((?:""|[^"])*)"',r['right_expression']);need([s.replace('""','"') for s in decoded]==values,'Choice order/literals changed')
  else:
   expr=r['right_expression'];decoded='' if expr=='String.Empty' else expr[1:-1].replace('""','"');need(decoded==r['right_literal'],'RHS decoded incorrectly')
  item=next(x for x in classified['calls'] if x['site_id']==r['site_id']);need(item['normalization_review'] and item['normalization_to_be_independently_classified'] is False,'Unreviewed caller domain')
 policy='BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb';need(digest(raw(policy))=='d42882ab96ae198e6e8c2c260f38d3e59439ab1adbe7c49e1c4f9347fb1c46cf','Comparison flags/CaseToken order changed')
 for config in ['Debug','Release']:
  probe=packet('text-domains-a1-v2-v2-'+config+'.json');need(probe['source_table_sha256']==digest(raw(base+'/text-call-sites.json')),'Wrong source table digest')
  observations=probe['results'];need(len(observations)==len(calls)*2,'Missing source-domain observations')
  keys=[(r['site_id'],r['culture']) for r in observations];need(len(set(keys))==len(keys) and set(keys)=={(r['site_id'],c) for r in calls for c in ['Invariant','sk-SK']},'Case/culture matrix incomplete')
  need(all(r['checked_values']==3771 and not r['mismatches'] and r['differences']==0 for r in observations),'Source-domain text nonpass')
  synthetic=probe['synthetic'];need(len(synthetic)==2 and all(r['differences']==4 and len(r['mismatches'])==4 and r['nonnull_differences']==0 for r in synthetic),'Erased original synthetic nonpasses')
  for s in synthetic:
   need({(r['left_utf16'],r['right_utf16'],r['native'],r['portable']) for r in s['mismatches']}=={('NULL',v,False,True) for v in ['AD-00','00-00']}|{(v,'NULL',False,True) for v in ['AD-00','00-00']},'Synthetic mismatch detail changed')
  meta=packet('inspection/'+STAGE+'/Inspect38-'+config+'.json');original_meta=load('evidence/04A38/preedit/compiled-'+config+'.json');key=lambda r:r['effective_excel_name']
  need(len(meta['exports'])==268 and sorted(meta['exports'],key=key)==sorted(original_meta['exports'],key=key),'Compiled268 export contract changed')
  # Expanded passive scope includes native root and compiler-generated callback bodies.
  full=packet('metadata/'+STAGE+'/current-'+config+'.json');need(len(full['scope'])==33,'Missing native module owner')
  for proof in proofs:
   owner=proof['original_id'].split('|')[1];native=next((r for r in full['methods'] if r['declaring_type']==owner and r['member_name']==proof['name'] and (proof['name']!='PrepareResultTableForUdf' or r['parameter_types']==['System.Object'])),None);need(native,'Missing compiled production caller')
   target=proof['portable_target'];need(any(isinstance(i['operand'],str) and '.'+target.split('.')[0]+' @ ' in i['operand'] and '.'+target.split('.')[1]+'(' in i['operand'] for i in native['instructions']),'Forged/missing actual portable CIL edge '+target)
  common_tests=packet('focused-'+STAGE+'-'+config+'.json');need(all(r['command']['exit_code']==0 for r in common_tests['results']),'Focused tests did not pass')
 compat=packet('compatibility-'+STAGE+'.json');counts={}
 for r in compat['results']:
  a=raw(r['original']['path']);b=raw(r['current']['path']);need(a==b and digest(a)==r['original']['sha256'] and digest(b)==r['current']['sha256'],'Frozen/current output drift')
  counts[r['configuration']]=counts.get(r['configuration'],0)+r['cases']
 need(counts=={'Debug':29424,'Release':29424} and compat['new_original_queries']==0,'Missing/extra frozen case or unauthorized original query')
 return dict(status='BOUNDED_V2_A_B1_PASS_NOT_BATCH_READY',proofs=proofs,text_direct_source_lines=59,text_direct_invocations=145,text_CaseToken_sites=63,text_source_domain_differences=0,synthetic_differences_retained_per_configuration=8,compatibility_per_configuration=dict(preedit=28966,late_A_to_G=458),W2='IN_PROGRESS',W3='NOT_STARTED',original_queries=0)

if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
