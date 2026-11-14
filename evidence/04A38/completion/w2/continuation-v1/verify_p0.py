"""Independent P0 operation/caller/exception-contract reader; never grants READY."""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;STAGE='p0-v2'
MIXED='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb';COMMON='BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb'
def need(v,m):
 if not v:raise ValueError(m)
def load(p):return json.loads(p.read_bytes())
def text(p):return p.read_text(encoding='utf-8-sig')
def tokens(s):return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',s) if not x.startswith("'")]
def member(s,name,kind='Function'):
 m=re.search(r'^([ \t]*)(?:Public|Private|Friend) (?:Shared )?'+kind+' '+name+r'\b',s,re.M);need(m is not None,'Missing member '+name)
 end=s.index('\n'+m[1]+'End '+kind,m.start())+len('\n'+m[1]+'End '+kind)
 return s[m.start():end]
def body(s,result):
 m=re.search(r'\) As '+re.escape(result)+r'\n',s);need(m is not None,'Unsupported signature')
 return s[m.end():s.rindex('\n')]
def inspect(root=ROOT):
 root=Path(root);here=root/HERE.relative_to(ROOT);entry=load(here/'entry.json');after=load(here/('source-transitions-'+STAGE+'.json'))
 def current(path):
  r=next(x for x in after['files'] if x['after']['path']==path);b=(root/r['snapshot']['path']).read_bytes();need(hashlib.sha256(b).hexdigest()==r['after']['sha256'],'P0 source snapshot changed');return b.decode('utf-8-sig').replace('\r\n','\n')
 def before(path):
  row=next(x for x in entry['source_before'] if x['live']['path']==path);b=(root/row['preimage']['path']).read_bytes();need(hashlib.sha256(b).hexdigest()==row['live']['sha256'],'P0 preimage changed');return b.decode('utf-8-sig').replace('\r\n','\n')
 native=current(MIXED);portable=current(COMMON)
 raw=(root/'evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt').read_bytes();need(hashlib.sha256(raw).hexdigest()=='596071f9fdb00fb94585e929a6899494293d2dc7ee0064faadf098b0d95dc13d','Archived original changed')
 original=raw.decode('utf-8-sig').replace('\r\n','\n')
 expected=body(member(original,'FormatMmrmAtProfileDescription'),'String')
 expected=expected.replace('MmrmLsmEstimateProfileValue','TValue').replace('regression.MixedModelPostEstimation.','Global.BESHStatNG.regression.MixedModelPostEstimation.')
 need(tokens(expected)==tokens(body(member(portable,'FormatMmrmAtProfileDescription'),'String')),'Description statement/order/formatter/literal mismatch')
 expected=body(member(before(MIXED),'TryBuildMmrmAtProfileRowMask'),'Boolean')
 for a,b in [('h Is Nothing','Not modelAvailable'),('h.FittedDesign','design()'),('h.VisitValues','visits()'),('AddressOf regression.MixedModelPostEstimation.NearlyEqual','AddressOf Global.BESHStatNG.regression.MixedModelPostEstimation.NearlyEqual'),('FormatMmrmAtProfileDescription(atProfile)','format(atProfile)')]:expected=expected.replace(a,b)
 need(tokens(expected)==tokens(body(member(portable,'TryBuildMmrmAtProfileRowMask'),'Boolean')),'Mask guard/ByRef/order/format timing changed')
 wrapper=body(member(native,'TryBuildMmrmAtProfileRowMask'),'Boolean')
 need('h IsNot Nothing, Function() h.FittedDesign, Function() h.VisitValues, atProfile' in wrapper and 'AddressOf FormatMmrmAtProfileDescription, rowMask, profileDescription, errorMessage' in wrapper,'Native context/getter/format policy wiring changed')
 # No pre-existing Common preparation body may be silently changed by P0.
 for name in ['TryGetGeeAtSpec','AddGeeAtValue','TryGetMmrmAtSpec','AddMmrmAtValue','TryGetMmrmHeaderlessNameValueAtSpec','TryGetMmrmDesignColumnValues','TryGetGeeSpec','TryGetMmrmSpec']:
  need(tokens(member(before(COMMON),name))==tokens(member(portable,name)),'Previously verified helper changed '+name)
 for field in ['Core_formatter','existing_Core_mask']:
  row=after[field];b=(root/row['path']).read_bytes();need(len(b)==row['bytes'] and hashlib.sha256(b).hexdigest()==row['sha256'],'Established Core formatting/mask policy changed')
 result=[]
 for c in ['Debug','Release']:
  meta=load(here/'inspection'/STAGE/('Inspect38-'+c+'.json'));old=load(root/'evidence/04A38/preedit'/('compiled-'+c+'.json'))
  key=lambda r:r['effective_excel_name'];need(len(meta['exports'])==268 and sorted(meta['exports'],key=key)==sorted(old['exports'],key=key),'Export contract changed')
  methods=meta['methods'];mask=next(m for m in methods if m['name']=='TryBuildMmrmAtProfileRowMask');fmt=next(m for m in methods if m['name']=='FormatMmrmAtProfileDescription')
  need(any('.UdfModelProfilePreparation38.TryBuildMmrmAtProfileRowMask(' in call['target'] for call in mask['calls']),'No actual shipping mask consumer')
  need(any('.UdfModelProfilePreparation38.FormatMmrmAtProfileDescription(' in call['target'] for call in fmt['calls']),'No actual shipping description consumer')
  need(any('.FormatMmrmAtProfileDescription(' in call['target'] and call['opcode']=='ldftn' for call in mask['calls']),'Description delegate not in production route')
  full=load(here/'inspection'/STAGE/('MethodBodies38-'+c+'.json'));portablemask=next(m for m in full if '.ExcelCommon.UdfModelProfilePreparation38.TryBuildMmrmAtProfileRowMask(' in m['method'])
  portablefmt=next(m for m in full if '.ExcelCommon.UdfModelProfilePreparation38.FormatMmrmAtProfileDescription(' in m['method'])
  need(not portablemask['type_initializer'] and not portablemask['nonliteral_static_fields'] and not portablefmt['type_initializer'] and not portablefmt['nonliteral_static_fields'],'New hidden portable static state')
  focus=load(here/('focused-'+STAGE+'-'+c+'.json'));need(all(r['command']['exit_code']==0 for r in focus['results']),'Focused validation failed')
  need([r['tests']['outcomes'] for r in focus['results'] if r['kind']=='TEST']==[{'Passed':32},{'Passed':17}],'Missing P0 tests or native route')
  result.append(dict(configuration=c,exports=268,Common_tests=32,shipping_adapter_tests=17))
 compat=load(here/('compatibility-'+STAGE+'.json'))
 for row in compat['results']:
  a=(root/row['original']['path']).read_bytes();b=(root/row['current']['path']).read_bytes();need(a==b,'Frozen comparison differs')
  need(hashlib.sha256(a).hexdigest()==row['original']['sha256'] and hashlib.sha256(b).hexdigest()==row['current']['sha256'],'Comparison digest changed')
 need(compat['totals_per_configuration']=={'Debug':29424,'Release':29424} and compat['new_original_queries']==0,'Reference cardinality/authority changed')
 return dict(status='P0_DESCRIPTION_AND_MASK_CONTRACT_PASS_NOT_BATCH_READY',original_ids=['FunctionBlock|9465ac0daaaabaa5@86828:701','FunctionBlock|e8eabf0f978b1297@83928:2886'],new_historical_proof_supported=2,prior_historical_proof_supported=47,historical_remaining_OPEN=655,configurations=result,source_operation_equivalence='Visit then insertion order; null/nonfinite/overflow preserved; lazy model access and all ByRef/guard/formatter phases match original/prior independently proved Core loop',new_original_queries=0,science='NOT_GOLDEN',native='NOT_RUN')
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except (ValueError,OSError,KeyError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
