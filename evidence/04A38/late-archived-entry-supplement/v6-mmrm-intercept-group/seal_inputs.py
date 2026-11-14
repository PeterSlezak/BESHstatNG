"""Freeze exact Approval E inputs and genuine .NET materialization before models."""
import copy
from datetime import datetime,timezone
from common import *

def main():
 folder=PRIVATE/'original-Debug-plain-v2';exe=folder/'MaterializeMmrm38.exe'
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 compiled=command('materialize-compile',[compiler('csc.exe'),'/nologo','/target:exe','/main:MaterializeMmrm38','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'MaterializeMmrm38.cs'),str(ROOT/'evidence/04A38/preedit/frozen-v2/Capture38.cs')]);assert compiled['exit_code']==0
 materialized=command('materialize-factory',[str(exe),str(folder),str(HERE/'factory-inputs.json')]);assert materialized['exit_code']==0
 assert (HERE/'factory-inputs.json').read_bytes()==(V3/'factory-inputs.json').read_bytes()==(V4/'factory-inputs.json').read_bytes()
 factory=json.loads((HERE/'factory-inputs.json').read_bytes());assert factory['execution']=='INPUT_FACTORY_ONLY_NO_MODEL_OR_QUERY_EXECUTION'
 plan=dict(source_snapshot=pin(ROOT/'evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt'),source_map=json.loads((HERE/'compile-original-Debug-trace.json').read_bytes())['instrumentation'],
  public_entry='BESH.REGR.MMRM_LSMEANS',argument_order=['handle','group','alpha'],source_member='TryGetDesignColumnValues',original_true_branch_line=1477,original_true_branch='Intercept or (Intercept) ordinal-ignore-case rejection, lines1477-1480',
  prospective_path=['public scalar name route','successful fitted name lookup with observed index','intercept condition true at1477-1478','error assignment1479 thenFalse return1480','private ByRef state propagated through public return'],
  observed_result='UNKNOWN_NOT_CAPTURED',observed_private_state='UNKNOWN_NOT_CAPTURED',private_observation='Trace-only observation of actual lookup index/name and initialized/returned ByRef name/vector/error; compare plain/repeat/trace public output bytes exactly',
  source_justification='Original factory contains 54 rows, visit1/2/3 each18; approved scalar Intercept is an actual fitted name; observe index rather than assume it. Expected reachability is prospective, not an observed output.',
  handle_normalization='Only actual fitted GUID -> HANDLE:MMRM:SESSION1. Preserve raw GUIDs separately; no other normalization permitted.')
 save('trace-plan.json',plan)
 for config in ['Debug','Release']:
  bases=[json.loads(line) for line in (V3/('inputs-'+config+'.jsonl')).read_bytes().splitlines()];assert len(bases)==2
  cases=[]
  for base in bases:
   c=copy.deepcopy(base);culture='Invariant' if c['culture']=='' else 'sk-SK'
   c['case_id']='AUTHORIZED-MMRM-INTERCEPT/'+culture+'/mmrm-lsmeans-scalar-intercept-group';c['case_kind']='mmrm-lsmeans-scalar-intercept-group';c['public_entry']='BESH.REGR.MMRM_LSMEANS'
   assert c['operations'][0]['arguments']==factory['fit_arguments']
   group=dict(kind='String',value='Intercept')
   c['operations'][1]=dict(role='PUBLIC_EXPORT',target='BESH.REGR.MMRM_LSMEANS',arguments=[dict(name='handle',value=dict(kind='HandleReference',session='fresh-case')),dict(name='group',value=group),dict(name='alpha',value=dict(kind='Nothing'))])
   c['session']['lifetime']='One real successful MMRM_FIT followed by one MMRM_LSMEANS; cache cleared before next case'
   c['session']['creation_order'][-1]='Query same genuine fitted handle with sealed group,alpha in exact public order'
   c['session']['required_fitted_visit_histogram']={'1.0':18,'2.0':18,'3.0':18}
   c['semantics']['ownership']='Fresh arrays decoded per argument/operation/case; before/after public arguments captured; private name/vector/error are trace-only ByRef observations'
   c['branch_expectation']=plan;c['expected_result']='UNKNOWN_NOT_CAPTURED';cases.append(c)
  with (HERE/('inputs-'+config+'.jsonl')).open('x',encoding='utf-8',newline='\n') as stream:
   for c in cases:stream.write(json.dumps(c,ensure_ascii=False,separators=(',',':'),allow_nan=False)+'\n')
 recorders=[]
 for config in ['Debug','Release']:
  for mode in ['plain','trace']:
   folder=PRIVATE/('original-'+config+'-'+mode+'-v2');exe=folder/'MmrmCapture38.exe'
   result=command('recorder-'+config+'-'+mode,[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'MmrmCapture38.cs')]);assert result['exit_code']==0
   recorders.append(dict(configuration=config,mode=mode,executable=library_pin(exe),command=clean(result)))
  exe=PRIVATE/('original-'+config+'-plain-v2')/'MmrmCapture38.exe'
  result=command('materialize-typed-'+config,[str(exe),'--materialize',str(HERE/('inputs-'+config+'.jsonl')),str(HERE/('materialized-'+config+'.json'))]);assert result['exit_code']==0
 save('materialization-receipt.json',dict(commands=[clean(compiled),clean(materialized)],factory_bytes_exact=True,fresh_factory=pin(HERE/'factory-inputs.json'),v3_factory=pin(V3/'factory-inputs.json'),original_case_provenance=pin(V3/'materialization-receipt.json'),recorders=recorders,
  handle_contract=plan['handle_normalization'],typed_packets=[pin(HERE/('materialized-'+c+'.json')) for c in ['Debug','Release']],cases_per_configuration=2,model_executions=0,query_executions=0,python_version=sys.version,compiler_files=[library_pin(Path(compiler(n))) for n in ['vbc.exe','csc.exe']]))
 save('input-seal.json',dict(status='INPUT_SEALED',sequence=1,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,reference_class=CLASS,cases_per_configuration=2,files=[pin(p) for p in sorted(HERE.iterdir()) if p.is_file()],original_executions=0,current_executions=0,not_preedit_frozen=True))
 print('INPUT_SEALED:2 cases/configuration, .NET typed arrays materialized, original/current model and query executions0')
if __name__=='__main__':main()
