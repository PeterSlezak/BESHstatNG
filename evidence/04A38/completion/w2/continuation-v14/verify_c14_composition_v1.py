"""Independent exact source composites, bounded typed grammar and alias repair proof."""
import json,re,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5] if 'evidence' in Path(__file__).parts else Path.cwd()
PREFIX='evidence/04A38/completion/w2/continuation-v14/';OLD=PREFIX.replace('v14/','v13/')
sys.path.append(str(ROOT/'evidence/04A38/completion/w2/continuation-v11'))
from verify_composition_v2 import tokens,unwrap
from source_views import body

def need(v,message):
 if not v:raise ValueError(message)
def phase(text):
 text=text.replace('\r\n','\n');m=re.search(r'\b(Function|Sub)\s+\w+',text);need(m is not None,'Missing actual method declaration');start=text.index('(',m.end());depth=0;end=None
 for i in range(start,len(text)):
  if text[i]=='(':depth+=1
  elif text[i]==')':
   depth-=1
   if depth==0:end=i;break
 end=text.index('\n',end)+1;stop=text.rfind('End '+m[1]);need(stop>=end,'Missing complete method end');return text[end:stop].strip()
def unwrap_cases(text):
 marker='LegacyUdfText38.CaseToken('
 while marker in text:
  start=text.index(marker);i=start+len(marker);begin=i;level=1;quoted=False;comma=None
  while i<len(text):
   ch=text[i]
   if ch=='"':
    if quoted and i+1<len(text) and text[i+1]=='"':i+=2;continue
    quoted=not quoted
   elif not quoted:
    if ch=='(':level+=1
    elif ch==')':
     level-=1
     if level==0:break
    elif ch==',' and level==1 and comma is None:comma=i
   i+=1
  need(comma is not None and level==0,'Malformed actual legacy text policy');text=text[:start]+text[begin:comma]+text[i+1:]
 return text

def inspect(root=ROOT,overrides=None,load_override=None):
 root=Path(root);overrides=overrides or {};raw=lambda p:overrides[p] if p in overrides else (root/p).read_bytes();load=load_override or (lambda p:json.loads(raw(p)))
 cs=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];props=load(PREFIX+'property-body-contracts-v1.json')['contracts'];pc=load(PREFIX+'portable-api-contracts-v1.json')['contracts'];api={(p['source_record']['owner'].split('.')[-1],p['source_record']['name']):p for p in pc}
 current=load(PREFIX+'current-universe-packet-v1.json');old=load(OLD+'current-ownership-v2.json');byold={(n['path'],n['name']):n for n in old['members'] if n['parent_member'] is None};trans={r['path']:r for r in load(PREFIX+'source-transitions-final-v1.json')['files']}
 moves=load(PREFIX+'implementation-source-composition-v1.json')['phases'];rows=[];moved={}
 for c in cs:
  name=c['current_record']['name'];path=c['current_record']['path'];mine=[m for m in moves if m['parent']==name and m['path']==path]
  if not mine:continue
  entry=body(raw(trans[path]['preimage']['path']),byold[path,name])[0].replace('\r\n','\n');need(tokens(entry)==tokens(c['original_whole_body']),'Prepared entry not exact original '+name);expected=entry
  for m in mine:
   if 'original_segment' in m:need(expected.count(m['original_segment'])==1,'Missing exact source movement '+m['api']);expected=expected.replace(m['original_segment'],m['actual_native_call'],1)
   else:need(expected.count(m['original_whole_prefix'])==1,'Wrong table dispatch prefix');expected=expected.replace(m['original_whole_prefix'],m['current_whole_prefix'],1)
   target=api['UdfPsmDataPreparation38',m['api']];need(tokens(phase(target['whole_body']))==tokens(m['portable_segment']),'Portable full body not exact '+m['api'])
  need(tokens(expected)==tokens(c['current_whole_body']),'Native changed outside exact prepared movements '+name);moved[c['current_id']]=mine
  rows.append(dict(original_id=c['original_id'],current_id=c['current_id'],name=name,mode='EXACT_WHOLE_NATIVE_PREFIX_PREPARED_BODY_SUFFIX',apis=[m['api'] for m in mine],movements=mine))
 # Native untouched parents and the already-portable option grammars.
 for c in cs:
  if not c['new_support'] or c['current_id'] in moved:continue
  name=c['current_record']['name'];before=c['original_whole_body'];now=c['current_whole_body'];mode=None
  if tokens(before)==tokens(now):mode='EXACT_ORIGINAL_CURRENT_WHOLE_PARENT'
  elif name in ['NormalizeFitOptions','ApplyOptionText','ApplyOption','EnsureCoarseningSpec','ParseRunMethod','ParseScoreMethod','ParseDistance','ParseCaliperScale','ParseOrder','ParseSupport','ParseBoolean']:
   owner='UdfPsmPreparation38' if name in ['NormalizeFitOptions','ApplyOptionText','ApplyOption','EnsureCoarseningSpec'] else 'PropensityScoreUDFsOptions38';p=api[owner,name]
   expected=phase(before).replace('ChrW(10)','Convert.ToChar(10)').replace('ChrW(13)','Convert.ToChar(13)');need(tokens(expected)==tokens(unwrap_cases(phase(p['whole_body']))),'Complete PSM option grammar differs '+name)
   args='fitOptions' if name in ['NormalizeFitOptions','EnsureCoarseningSpec'] else 'fitOptions, optionText' if name=='ApplyOptionText' else 'fitOptions, key, value' if name=='ApplyOption' else 'value, defaultValue'
   expected_call=('' if c['current_record']['kind']=='SubBlock' else 'Return ')+'Global.BESHStatNG.ExcelCommon.'+owner+'.'+name+'('+args+')'
   need(tokens(phase(now))==tokens(expected_call),'Changed native full option wrapper '+name);mode='EXACT_FULL_LEGACY_GRAMMAR_WITH_NATIVE_CALLER_AND_EXPLICIT_TEXT_COMPARE_POLICY'
  elif name=='ParseEstimand':
   helper=next(r for r in cs if r['current_record']['name']=='ParseEnumValue');p=api['UdfPsmPreparation38',name]
   need(tokens(phase(before))==tokens('Return CType(ParseEnumValue(GetType(PsmEstimand), value, defaultValue), PsmEstimand)'),'Original enum entry changed')
   expected='''If String.IsNullOrWhiteSpace(value) Then Return defaultValue
Try
Return [Enum].Parse(enumType, value.Trim(), True)
Catch
Return defaultValue
End Try''';need(tokens(phase(helper['original_whole_body']))==tokens(expected),'Original generic enum failure grammar changed')
   typed=expected.replace('Return [Enum].Parse(enumType, value.Trim(), True)','Return CType([Enum].Parse(GetType(PsmEstimand), value.Trim(), True), PsmEstimand)')
   need(tokens(phase(p['whole_body']))==tokens(typed),'Current typed enum parse grammar changed');need(tokens(phase(now))==tokens('Return Global.BESHStatNG.ExcelCommon.UdfPsmPreparation38.ParseEstimand(value, defaultValue)'),'Wrong native typed enum consumer');mode='EXPLICIT_GENERIC_PARSE_TRY_DEFAULT_AND_STRONGLY_TYPED_ENUM_CAST_COMPOSITE'
  elif name=='GetFormulaAddressingMode':
   full=load(PREFIX+'dependency-full-common-v1.json');n=next(n for n in full['members'] if n['owner']=='BESHStatNG.ExcelCommon.UdfDataImportOptions38' and n['name']==name);portable=body(raw(n['path']),n)[0]
   expected=phase(before).replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)','arg')
   need(tokens(expected)==tokens(unwrap_cases(phase(portable))),'Whole formula addressing fallback differs');need(tokens(phase(now))==tokens('Return Global.BESHStatNG.ExcelCommon.UdfDataImportOptions38.GetFormulaAddressingMode(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), defaultMode)'),'Formula addressing native coercion order changed');mode='EXACT_HOST_TEXT_CONVERSION_DEFAULT_AND_TYPED_FORMULA_ADDRESSING_GRAMMAR'
  elif name=='TryGetRocInput':
   m=next(m for m in load(OLD+'implementation-source-composition-v2.json')['phases'] if 'Function TryGetRocInput(' in m['parent']);expected=before.replace('\r\n','\n').replace(m['original_segment'],m['actual_native_call'],1)
   need(tokens(expected)==tokens(now),'Whole ROC importer native prefix/materialization changed');p=api['UdfPlotDataPreparation38','CompleteRocInputPrepared'];need(tokens(phase(p['whole_body']))==tokens(m['portable_segment']),'Complete acquired ROC row preparation differs');mode='EXACT_C13_PREPARED_ROC_INPUT_COMPOSITE_WITH_REPAIRED_POSITIVE_LABEL_DEPENDENCY'
  elif name=='GuessPositiveClass':
   p=api['UdfClassLabelPreparation38',name];newapi=api['UdfClassLabelPreparation38','GuessPositiveClassAfterConversion'];oldfull=load(OLD+'dependency-full-common-v1.json');oldnode=next(n for n in oldfull['members'] if n['owner']==p['source_record']['owner'] and n['name']==name)
   oldfile=raw(trans[p['source_record']['path']]['preimage']['path']);need(body(oldfile,oldnode)[0]==p['whole_body'],'Existing resetting Common API body changed')
   oldcode=phase(before);prefix='''positiveLabel = Nothing
Dim explicitText As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(explicitPositive)'''
   need(tokens(oldcode).startswith(tokens(prefix)),'Original reset/conversion sequence changed')
   suffix=oldcode[oldcode.index('        If Not String.IsNullOrWhiteSpace(explicitText)'):]
   suffix=suffix.replace('statusCol.GetLength(0)','rowCount()').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(statusCol(i, 0))','text(i)')
   block='''Dim iv As Integer
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(statusCol(i, 0), iv) Then
                allBinary01 = False
            End If''';need(tokens(block) in tokens(suffix),'Original binary classification boundary missing')
   suffix=re.sub(r'Dim iv As Integer\s+If Not Global\.BESHStatNG\.WorksheetFunctions\.ExcelArgNumeric\.TryGetStatus01Flexible\(statusCol\(i, 0\), iv\) Then\s+allBinary01 = False\s+End If','If Not binary(i) Then allBinary01 = False',suffix)
   suffix=re.sub(r'\bs\b','token',suffix);suffix=re.sub(r'\bnorm\b','normalized',suffix);suffix=suffix.replace('Dim norms =','Dim norms As String() =').replace('Function(x) NormalizeOptionKey(x)','Function(token) UdfDataImportOptions38.NormalizeOptionKey(token)').replace('NormalizeOptionKey(token)','UdfDataImportOptions38.NormalizeOptionKey(token)').replace('UdfDataImportOptions38.UdfDataImportOptions38.','UdfDataImportOptions38.')
   portable=phase(p['whole_body']);need(tokens(portable).startswith(tokens('positiveLabel = Nothing')),'Existing Common reset contract lost');portable=portable[portable.index('\n')+1:]
   need(tokens(suffix)==tokens(unwrap_cases(portable)),'Entire original positive-label selection grammar differs')
   expected_new='''Dim prepared As String = Nothing
Dim result As Boolean = GuessPositiveClass(explicitText, rowCount, text, binary, prepared)
If result Then positiveLabel = prepared
Return result''';need(tokens(phase(newapi['whole_body']))==tokens(expected_new),'Wrong post-conversion alias commit policy')
   assignments=re.findall(r'(?m)^\s*positiveLabel\s*=([^\n]+)\n\s*Return True\b',oldcode);allwrites=re.findall(r'(?m)^\s*positiveLabel\s*=',oldcode);need(len(assignments)==4 and len(allwrites)==5,'Original can expose an unmodelled partial output assignment')
   expected_native=prefix+'''\nReturn Global.BESHStatNG.ExcelCommon.UdfClassLabelPreparation38.GuessPositiveClassAfterConversion(
explicitText, Function() statusCol.GetLength(0),
Function(row) Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(statusCol(row, 0)),
Function(row) IsStatusBinaryCell38(statusCol(row, 0)), positiveLabel)''';need(tokens(phase(now))==tokens(expected_native),'Native reset/conversion/callback order changed')
   numeric=next(r for r in cs if r['current_record']['name']=='TryGetStatus01Flexible');need(tokens(phase(numeric['current_whole_body'])).startswith(tokens('value = 0')),'Status policy no longer resets its private integer')
   mode='COMPLETE_ORIGINAL_SELECTION_GRAMMAR_WITH_PROVEN_LOCAL_RESULT_COMMIT_AND_NATIVE_PRECONVERSION_RESET'
  else:raise ValueError('Unreconciled whole parent '+name)
  rows.append(dict(original_id=c['original_id'],current_id=c['current_id'],name=name,mode=mode))
 for p in props:
  need(tokens(p['original_whole_body'])==tokens(p['current_whole_body']),'Alpha declaration/default changed');need(len(p['compiled'])==2 and all(len(r['current_methods'])==3 for r in p['compiled']),'Alpha accessors/constructor missing');rows.append(dict(original_id=p['original_id'],current_id=p['current_id'],name='Alpha',mode='EXACT_PROPERTY_DECLARATION_GETTER_SETTER_DEFAULT_CONSTRUCTOR_AND_ALL_WRITERS'))
 newids={c['current_id'] for c in cs+props if c['new_support']};need(newids<={r['current_id'] for r in rows} and len(newids)==35,'Whole-parent behavioral proof omitted a proposed closure')
 return dict(status='COMPLETE_SOURCE_RECONSTRUCTION_WITH_ACTUAL_CIL_AND_CURRENT_EDGE_TESTS_REQUIRED',rows=rows,new_current_supported=35,original_queries=0,
  qualification='Static source parity is independent of ordinary frozen compatibility. Original False/converted remains a source prediction; actual current-only diagnostics and complete compiled boundaries are separate evidence. No original oracle or native dynamic-arm execution inferred.')

if __name__=='__main__':print(json.dumps(inspect()))
