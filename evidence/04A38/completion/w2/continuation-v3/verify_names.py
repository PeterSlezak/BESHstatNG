"""Independent source-operation/typed-boundary/CIL reader. Not a batch READY gate."""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;STAGE='names-v3'
PREFIX='BESHstatNG/src/ExcelUDFs/';COMMON='BESHStat.Excel.Common/src/Analysis/UdfMetadataPreparation38.vb';SPILL='BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb'
def need(v,m):
 if not v:raise ValueError(m)
def sha(b):return hashlib.sha256(b).hexdigest()
def tokens(s):return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',s) if not x.startswith("'")]
def member(s,n):
 m=re.search(r'^([ \t]*)(?:Public|Private|Friend) (?:Shared )?Function '+n+r'\b',s,re.M);need(m,'Missing member '+n);end=s.index('\n'+m[1]+'End Function',m.end());return s[m.start():end+len('\n'+m[1]+'End Function')]
def body(s):
 lines=s.splitlines();i=next(i for i,l in enumerate(lines) if ') As ' in l and not l.rstrip().endswith(','));return '\n'.join(lines[i+1:-1])
def stable(r):return {k:v for k,v in r.items() if k not in ['method','declaring_type','raw_IL_sha256']}
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def raw(p):return overrides.get(p,(root/p).read_bytes())
 def load(p):return json.loads(raw(p))
 def text(p):return raw(p).decode('utf-8-sig').replace('\r\n','\n')
 for r in load(base+'entry.json')['source_before']:
  need(sha(raw(r['preimage']['path']))==r['live']['sha256'],'Current preimage missing/changed')
 trans=load(base+'source-transitions-'+STAGE+'.json')['files']
 for r in trans:need(raw(r['after']['path'])==raw(r['snapshot']['path']) and sha(raw(r['after']['path']))==r['after']['sha256'],'Source differs from compiled successor')
 originals=load('evidence/04A38/preedit/entry-source-snapshot.json')['files']
 def original(p):
  r=next(r for r in originals if r['source']['path']==p)['snapshot'];b=raw(r['path']);need(sha(b)==r['sha256'] and len(b)==r['bytes'],'Original snapshot changed');return b.decode('utf-8-sig').replace('\r\n','\n')
 before=lambda p:text(base+'source-before/'+p+'.txt')
 matrix=load('evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members'];proofs=[]
 def prove(p,n,target,reason):
  r=next(r for r in matrix if r['path']==p and r['name']==n);proofs.append(dict(original_id=r['canonical_original_id'],path=p,name=n,target=target,reason=reason))
 # Precisely two helpers removed from the coordinate class; canonical bodies unchanged.
 old=before(SPILL);new=text(SPILL);oldcell=old[old.index('    Public NotInheritable Class UdfSpillCell38'):old.index('    Public NotInheritable Class UdfSpillPreparation38')];newcell=new[new.index('    Public NotInheritable Class UdfSpillCell38'):new.index('    Public NotInheritable Class UdfSpillPreparation38')]
 for n in ['StackWithBlankRow','NormalizeBlanks']:oldcell=oldcell.replace(member(oldcell,n),'');need(tokens(member(old[old.index('    Public NotInheritable Class UdfSpillPreparation38'):],n))==tokens(member(new,n)),'Canonical spill method changed')
 need(tokens(oldcell)==tokens(newcell),'Coordinate properties/constructor or other code changed')
 p=PREFIX+'ParametricUDFs.vb';old=original(p);need(tokens(member(old,'ResolveNames'))==tokens(member(before(p),'ResolveNames')),'Name resolver preimage not original')
 whole=body(member(old,'ResolveNames'));a=whole.index('For i As Integer');end=whole.index('Return names',a)+len('Return names');fill=whole[a:end]
 need(tokens(fill)==tokens(body(member(text(COMMON),'FillExplicitNames'))),'Explicit-name mutation, actual length or identity changed')
 fallback=whole[whole.index('If detectedNames IsNot Nothing'):]
 need(tokens(fallback)==tokens(body(member(text(COMMON),'ResolveDetectedNames'))),'Detected copy/default/count/culture behavior changed')
 expected='''If Not IsMissingArg(explicitNames) Then
 Dim names = Global.BESHStatNG.UdfDataImport.GetVariableNames(explicitNames, expectedCount)
 Return Global.BESHStatNG.ExcelCommon.UdfNamePreparation38.FillExplicitNames(names, prefix)
 End If
 Return Global.BESHStatNG.ExcelCommon.UdfNamePreparation38.ResolveDetectedNames(detectedNames, expectedCount, prefix)'''
 need(tokens(expected)==tokens(body(member(text(p),'ResolveNames'))),'Host explicit priority/acquisition order changed')
 prove(p,'ResolveNames','UdfNamePreparation38.FillExplicitNames + ResolveDetectedNames','Complete original three-branch operation composition; host missing/acquisition precedes typed policy; explicit same-array mutation uses actual length; detected fresh copy; current-culture ordinals and negative allocation behavior preserved')
 p=PREFIX+'UDFdataImport/UdfDataImportFormula.vb';old=original(p);need(tokens(member(old,'GetVariableNames'))==tokens(member(before(p),'GetVariableNames')),'Variable-name original preimage changed')
 expected=body(member(old,'GetVariableNames')).replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg)','isMissing()').replace('TryCast(arg, String)','scalarText()').replace('Dim arr As Object(,) = Get2D(arg)','Dim arr As UdfVariableNameRange38 = acquireRange()').replace('arr.GetLength(0)','arr.RowCount').replace('arr.GetLength(1)','arr.ColumnCount')
 for indices in ['0, j','i, 0']:
  pattern='''Dim cell As Object = arr('''+indices+''')
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then
                    list.Add(String.Empty)
                Else
                    list.Add(Convert.ToString(cell).Trim())
                End If'''
  need(pattern in expected,'Unexpected original host cell policy');expected=expected.replace(pattern,'list.Add(arr.ReadText('+indices+'))')
 need(tokens(expected)==tokens(body(member(text(COMMON),'GetVariableNames'))),'Variable-name grammar/traversal/conditional trim/exception order changed')
 host=body(member(text(p),'GetVariableNames'));expected_host='''If expectedCount <= 0 Then Return New String() {}
 Return Global.BESHStatNG.ExcelCommon.UdfNamePreparation38.GetVariableNames(expectedCount,
 Function() Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg),
 Function() TryCast(arg, String),
 Function() As Global.BESHStatNG.ExcelCommon.UdfVariableNameRange38
 Dim arr As Object(,) = Get2D(arg)
 If arr Is Nothing Then Return Nothing
 Dim rows As Integer = arr.GetLength(0)
 Dim cols As Integer = arr.GetLength(1)
 Return New Global.BESHStatNG.ExcelCommon.UdfVariableNameRange38(rows, cols,
 Function(row As Integer, column As Integer) As String
 Dim cell As Object = arr(row, column)
 If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then Return String.Empty
 Return Convert.ToString(cell).Trim()
 End Function)
 End Function)'''
 need(tokens(host)==tokens(expected_host),'Native acquisition/blank/error/Convert callback changed')
 prove(p,'GetVariableNames','UdfNamePreparation38.GetVariableNames + UdfVariableNameRange38','Exact original fallback-before-missing, comma RemoveEmptyEntries, row-before-column, conversion-before-count-mismatch and fill order; original native Get2D normalizes/copies bounds; no ExcelError crosses typed string boundary')
 configs=[]
 for c in ['Debug','Release']:
  oldmeta=load(base+'metadata-v5/'+STAGE+'/original-'+c+'.json');meta=load(base+'metadata-v5/'+STAGE+'/current-'+c+'.json');methods=meta['methods'];om=oldmeta['methods']
  for owner,n in [('BESHStatNG.WorksheetFunctions.ParametricUDFs','ResolveNames'),('BESHStatNG.UdfDataImport','GetVariableNames')]:
   m=next(r for r in methods if r['declaring_type']==owner and r['member_name']==n);need(any(isinstance(i['operand'],str) and 'BESHStatNG.ExcelCommon.UdfNamePreparation38 @ ' in i['operand'] for i in m['instructions']),'Missing real compiled name-policy consumer')
  cellmethods=[r['member_name'] for r in methods if r['declaring_type']=='BESHStatNG.ExcelCommon.UdfSpillCell38'];need(sorted(cellmethods)==['.ctor','get_ColumnOrdinal','get_RowOrdinal'],'Unintended coordinate API persists')
  for name in ['StackWithBlankRow','NormalizeBlanks']:
   need(len([r for r in methods if r['declaring_type']=='BESHStatNG.ExcelCommon.UdfSpillPreparation38' and r['member_name']==name])==1,'Missing/duplicate canonical spill body')
  for owner,n in [('BESHStatNG.UdfDataImport','Get2D'),('BESHStatNG.WindowsSpreadsheetTransport','ReadUdfMatrix'),('BESHStatNG.WindowsSpreadsheetTransport','CopyValues'),('BESHStatNG.WorksheetFunctions.ExcelArgPredicates','IsMissingArg'),('BESHStatNG.WorksheetFunctions.ExcelArgPredicates','IsBlankCell')]:
   a=next(r for r in om if r['declaring_type']==owner and r['member_name']==n);b=next(r for r in methods if r['declaring_type']==owner and r['member_name']==n);need(stable(a)==stable(b),'Original acquisition/cell/exception dependency changed '+n)
  for owner in ['BESHStatNG.ExcelCommon.UdfNamePreparation38','BESHStatNG.ExcelCommon.UdfVariableNameRange38']:
   ms=[r for r in methods if r['declaring_type']==owner];need(ms and all(not r['type_initializer'] and r['nonliteral_static_fields']==0 for r in ms),'Hidden portable state')
   for m in ms:
    need(all('ExcelDna' not in str(i['operand']) and 'Microsoft.VisualBasic' not in str(i['operand']) for i in m['instructions']),'Host/VB helper leaked into portable name policy')
  exports=load(base+'inspection/'+STAGE+'/Inspect38-'+c+'.json')['exports'];baseline=load('evidence/04A38/preedit/compiled-'+c+'.json')['exports'];key=lambda x:x['effective_excel_name'];need(len(exports)==268 and sorted(exports,key=key)==sorted(baseline,key=key),'Compiled UDF ABI changed')
  focus=load(base+'focused-'+STAGE+'-'+c+'.json');need(all(r['command']['exit_code']==0 for r in focus['results']),'Focused tests nonpass');configs.append(dict(configuration=c,outcomes=[r['tests']['outcomes'] for r in focus['results'] if r['kind']=='TEST']))
 compat=load(base+'compatibility-'+STAGE+'.json')
 for r in compat['results']:
  a=raw(r['original']['path']);b=raw(r['current']['path']);need(a==b and sha(a)==r['original']['sha256'] and sha(b)==r['current']['sha256'],'Original/current frozen bytes mismatch')
 need(compat['totals_per_configuration']==dict(Debug=29424,Release=29424) and compat['new_original_queries']==0,'Missing/extra or original query')
 return dict(status='NAMES_AND_CANONICAL_SPILL_SOURCE_CIL_PASS_NOT_READY',proofs=proofs,configurations=configs,duplicates_removed_from_coordinate_class=2,canonical_spill_methods_unchanged=True,original_queries=0,W2='IN_PROGRESS',W3='NOT_STARTED',boundary='No opaque ExcelError/cell enters portable policy; host accessor supplies already interpreted string after original copied/normalized Get2D')
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
