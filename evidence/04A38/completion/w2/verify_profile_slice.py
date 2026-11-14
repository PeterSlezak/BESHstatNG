"""Independent source-operation, CLR-export and frozen-result verifier.

No migration producer or statistical implementation is imported. This bounded
reader deliberately cannot grant whole-batch READY or close unreviewed families.
"""
import base64,csv,hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
W='evidence/04A38/completion/w2/'
G='evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide/'
PREVIOUS=G+'publication-manifest-closeout-v1.json'
COMMON='BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb'
CORE='BESHStat.Core/src/WorksheetFunctions/UdfProfilePreparation38.vb'
PATHS={'Gee':'BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb','Mmrm':'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMixedModel.vb'}
MIXED='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb'

def need(v,m):
 if not v:raise ValueError(m)
def load(p):return json.loads(p.read_bytes())
def digest(b):return hashlib.sha256(b).hexdigest()
def tokens(s):
 return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',s) if not x.startswith("'")]
def block(text,name,kind='Function'):
 pattern=r'^([ \t]*)(?:Private|Public|Friend) (?:Shared )?'+kind+' '+name+r'\b'
 m=re.search(pattern,text,re.M);need(m is not None,'Missing source member '+name)
 end=text.index('\n'+m[1]+'End '+kind,m.start())+len('\n'+m[1]+'End '+kind)
 return text[m.start():end]
def body(s):
 m=re.search(r'\) As Boolean\n',s);need(m is not None,'Unrecognized Boolean method boundary')
 return s[m.end():s.rindex('\n')]
def pincheck(root,row):
 data=(root/row['path']).read_bytes();need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Changed bytes '+row['path'])
def text(root,p):return (root/p).read_text(encoding='utf-8-sig')

def inspect(root=ROOT):
 root=Path(root);before=lambda p:text(root,W+'source-before/'+p)
 common=text(root,COMMON);old_matrix=load(root/'evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members']
 targets=[]
 for family,meta in [('Gee','GEEUDFsMetadata38'),('Mmrm','UdfDataImportMetadata38')]:
  old=text(root,W+'source-before/'+PATHS[family]);current=text(root,PATHS[family])
  for oldname,newname,category in [('TryGet'+family+'LsmEstimateAtSpec','TryGet'+family+'AtSpec','parser'),('TryGet'+family+'LsmEstimateSpec','TryGet'+family+'Spec','spec'),('Add'+family+'LsmEstimateAtValue','Add'+family+'AtValue','selection')]:
   original=block(old,oldname);expected=body(original)
   if category in ['parser','spec']:
    expected=expected[expected.index('Dim rows As Integer'):]
    prefix=body(original)[:body(original).index('Dim rows As Integer')]
    actual=body(block(current,oldname))
    need(tokens(actual[:actual.index('Return Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38')])==tokens(prefix),'Host initialization/acquisition/error prefix changed '+oldname)
    need('AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell' in actual and 'AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText' in actual and 'AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble' in actual,'Host cell policy altered')
    need('Function() h.FixedEffectNames' in actual if family=='Mmrm' else 'arr, parameterNames,' in actual,'Model names acquired differently')
   for prefix in ['Global.BESHStatNG.WorksheetFunctions.','']:
    for a,b in [('ExcelArgPredicates.IsBlankCell','isBlank'),('ExcelArgReaders.CellToTrimmedText','cellText'),('ExcelArgNumeric.TryGetFiniteDouble','tryFinite')]:expected=expected.replace(prefix+a,b)
   for a,b in [('Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile','TProfile'),('Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateComponent','TComponent'),('Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue','TValue'),('GeeLsmEstimateAtProfile','TProfile'),('GeeLsmEstimateComponent','TComponent'),('GeeLsmEstimateProfileValue','TValue')]:expected=expected.replace(a,b)
   for name in ['FindHeaderIndex','FindDesignColumnIndex','NormalizeDesignColumnName']:expected=expected.replace(family+name+'(',meta+'.'+family+name+'(')
   expected=expected.replace('Add'+family+'LsmEstimateAtValue(','Add'+family+'AtValue(Of TProfile, TValue)(')
   if family=='Mmrm':
    expected=expected.replace('h.FixedEffectNames','names()').replace('parsed, h,','parsed, names,')
    expected=expected.replace('TryGetMmrmHeaderlessNameValueAtSpec(arr, h, headerlessAt, errorMessage, attemptedHeaderlessAt)','TryGetMmrmHeaderlessNameValueAtSpec(Of TProfile, TValue)(arr, modelAvailable, names, headerlessAt, isBlank, cellText, tryFinite, errorMessage, attemptedHeaderlessAt)')
   need(tokens(expected)==tokens(body(block(common,newname))),'Full operation/literal/order mismatch '+oldname)
   matches=[r for r in old_matrix if r['name']==oldname and r['path']==PATHS[family]];need(len(matches)==1 and matches[0]['body_tokens_unchanged'] is True,'Original/W1 behavioral identity not established')
   targets.append(dict(canonical_original_id=matches[0]['canonical_original_id'],name=oldname,portable=newname,status='SOURCE_OPERATIONS_AND_PUBLIC_COMPATIBILITY_VERIFIED'))
 # Headerless grammar: explicit model-availability Boolean replaces only the
 # original null handle test; native wrapper supplies h IsNot Nothing.
 old=before(PATHS['Mmrm']);expected=body(block(old,'TryGetMmrmHeaderlessNameValueAtSpec'))
 expected=expected.replace('h Is Nothing','Not modelAvailable').replace('New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateAtProfile()','New TProfile()')
 for a,b in [('Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell','isBlank'),('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText','cellText'),('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble','tryFinite'),('AddMmrmLsmEstimateAtValue(probe, h,','AddMmrmAtValue(Of TProfile, TValue)(probe, names,'),('AddMmrmLsmEstimateAtValue(atProfile, h,','AddMmrmAtValue(Of TProfile, TValue)(atProfile, names,')]:expected=expected.replace(a,b)
 need(tokens(expected)==tokens(body(block(common,'TryGetMmrmHeaderlessNameValueAtSpec'))),'Headerless probe/attempt/ByRef order changed')
 targets.append(dict(canonical_original_id=next(r['canonical_original_id'] for r in old_matrix if r['name']=='TryGetMmrmHeaderlessNameValueAtSpec'),name='TryGetMmrmHeaderlessNameValueAtSpec',portable='TryGetMmrmHeaderlessNameValueAtSpec',status='SOURCE_OPERATIONS_AND_PUBLIC_COMPATIBILITY_VERIFIED'))
 # Scalar grouping: keep actual handle/context guard and three ByRef resets.
 original=body(block(before(MIXED),'TryGetDesignColumnValues'));split=original.index('If h.FixedEffectNames Is Nothing');expected=original[split:]
 expected=expected.replace('h.FixedEffectNames','names').replace('h.FittedDesign','design').replace('FindDesignColumnIndex(','MixedModelUDFsMetadata38.FindDesignColumnIndex(')
 need(tokens(expected)==tokens(body(block(common,'TryGetMmrmDesignColumnValues'))),'Grouping role/index/copy/error changed')
 actual=body(block(text(root,MIXED),'TryGetDesignColumnValues'));need(tokens(actual[:actual.index('Return Global.BESHStatNG.ExcelCommon.')])==tokens(original[:split]),'Grouping native context/ByRef reset changed')
 targets.append(dict(canonical_original_id=next(r['canonical_original_id'] for r in old_matrix if r['name']=='TryGetDesignColumnValues'),name='TryGetDesignColumnValues',portable='TryGetMmrmDesignColumnValues',status='SOURCE_OPERATIONS_AND_PUBLIC_COMPATIBILITY_VERIFIED'))
 # Mask: exact loop migration, including caller-owned partial ByRef writes.
 original=body(block(before(MIXED),'TryBuildMmrmAtProfileRowMask'));start=original.index('For i As Integer = 0 To n - 1');end=original.index('If matchedCount <= 0 Then',start);loop=original[start:end]
 for a,b in [('atProfile.VisitSpecified','timeSpecified'),('atProfile.VisitValue','targetTime'),('h.VisitValues','timeValues'),('h.FittedDesign','design'),('atProfile.ProfileValues','profiles'),('MmrmLsmEstimateProfileValue','T'),('profileValue.ColumnIndex','column(profileValue)'),('profileValue.Value','value(profileValue)'),('regression.MixedModelPostEstimation.NearlyEqual','equal')]:loop=loop.replace(a,b)
 core=text(root,CORE);fill=block(core,'FillRowMask','Sub');actual_loop=fill[fill.index('For i As Integer = 0 To n - 1'):fill.rindex('End Sub')]
 need(tokens(loop)==tokens(actual_loop),'Row mask source/order/predicate changed')
 host=body(block(text(root,MIXED),'TryBuildMmrmAtProfileRowMask'))
 need(tokens(original[:start])==tokens(host[:host.index('Global.BESHStatNG.WorksheetCompatibility.UdfProfilePreparation38.FillRowMask')]),'Mask allocation or precondition changed')
 need(tokens(original[end:])==tokens(host[host.index('If matchedCount <= 0 Then'):]),'Mask guard/error/profile description changed')
 need(tokens(core.replace(fill,''))==tokens(before(CORE)),'Existing Core design averaging/statistical operations changed')
 need('ExcelDna' not in common and 'Microsoft.VisualBasic' not in common and 'Option Strict On' in common,'Portable host dependency or weak typing')
 for family in ['Gee','Mmrm']:
  name='Time' if family=='Gee' else 'Visit'
  need('Public Property '+name+'Specified As Boolean = False' in common and 'Public Property '+name+'Value As Double = Double.NaN' in common,'AT initialization semantics changed')
 # Recompute result equality; do not trust the producer's exact=True fields.
 comparisons=[]
 for config in ['Debug','Release']:
  count=0
  for row in load(root/(W+'preedit-compatibility.json'))['results']:
   if row['configuration']!=config:continue
   pincheck(root,row['original']);pincheck(root,row['current']);need((root/row['original']['path']).read_bytes()==(root/row['current']['path']).read_bytes(),'Pre-edit mismatch')
   count+=row['unique_cases']
  need(count==28966,'Missing genuine pre-edit cases')
  for row in load(root/(W+'profile-compatibility-v3-'+config+'.json'))['results']:
   pincheck(root,row['original']);pincheck(root,row['current']);a=root/row['original']['path'];b=root/row['current']['path'];need(a.read_bytes()==b.read_bytes(),'Late profile mismatch')
   left=load(Path(str(a)+'.sessions.json'));right=load(Path(str(b)+'.sessions.json'))
   strip=lambda s:{k:v for k,v in s.items() if k!='raw_handle'}
   need([strip(s) for s in left]==[strip(s) for s in right],'Actual fitted/session/design state differs')
   need(load(b.parent/'execution.json')['native_attempts']==0,'Native attempt')
  original=load(root/('evidence/04A38/preedit/compiled-'+config+'.json'));current=load(root/(W+'compiled-'+config+'-profiles-v1.json'))
  key=lambda e:e['effective_excel_name'];need(len(current['exports'])==268 and sorted(original['exports'],key=key)==sorted(current['exports'],key=key),'Export ABI/attributes/defaults drift')
  methods=current['methods'];edges=[dict(caller=r['method'],target=c['target']) for r in methods for c in r['calls'] if ('UdfModelProfilePreparation38.' in c['target'] or 'UdfProfilePreparation38.FillRowMask' in c['target']) and '.ctor' not in c['target']]
  need(len(edges)==6,'Missing actual Windows semantic consumer edge')
  comparisons.append(dict(configuration=config,genuine_preedit_exact=28966,late_B_through_G_exact=26,exports=268,semantic_direct_edges=edges))
 return dict(status='PROFILE_SLICE_VERIFIED_NOT_BATCH_READY',historical_whole_bodies_closed=8,historical_mask_body_reviewed_but_description_dependency_open=1,historical_mandatory_open=696,targets=targets,mask_dependency='FormatMmrmAtProfileDescription remains a reusable Windows helper; not HOST_ONLY',comparisons=comparisons)

if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except (OSError,ValueError,KeyError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
