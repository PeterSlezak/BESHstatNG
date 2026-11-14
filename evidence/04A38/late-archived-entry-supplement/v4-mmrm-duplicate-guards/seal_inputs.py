"""Materialize precisely two guard kinds by two cultures; freeze before models."""
import copy
from datetime import datetime,timezone
from common import *

def main():
 folder=PRIVATE/'original-Debug-plain-v2';exe=folder/'MaterializeMmrm38.exe'
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 compiled=command('materialize-compile',[compiler('csc.exe'),'/nologo','/target:exe','/main:MaterializeMmrm38','/reference:System.Core.dll','/reference:System.Web.Extensions.dll',
      '/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'MaterializeMmrm38.cs'),str(ROOT/'evidence/04A38/preedit/frozen-v2/Capture38.cs')]);assert compiled['exit_code']==0
 materialized=command('materialize-inputs',[str(exe),str(folder),str(HERE/'factory-inputs.json')]);assert materialized['exit_code']==0
 assert (HERE/'factory-inputs.json').read_bytes()==(V3/'factory-inputs.json').read_bytes(),'Original factory or spec differs from successful v3 capture'
 factory=json.loads((HERE/'factory-inputs.json').read_bytes());assert factory['execution']=='INPUT_FACTORY_ONLY_NO_MODEL_OR_QUERY_EXECUTION'
 variants={
  'duplicate-visit-time':dict(settings=[('visit','3FF0000000000000'),('time','4000000000000000')],guard_member='AddMmrmLsmEstimateAtValue',guard_line=726,
      true_branch='If atProfile.VisitSpecified Then',required_events=['selection.visit','selection.visit_value','visit.previously_specified','visit.normalized_dispatch','guard.duplicate_visit_time']),
  'duplicate-fitted-column':dict(settings=[('treatment_active','3FF0000000000000'),('TREATMENT_ACTIVE','4000000000000000')],guard_member='AddMmrmLsmEstimateAtValue',guard_line=744,
      true_branch='If existing.ColumnIndex = idx Then',required_events=['design.resolved_index','selection.design','design.existing_count','design.existing_index','guard.duplicate_design'])}
 maps=json.loads((HERE/'compile-original-Debug-trace.json').read_bytes())['instrumentation']
 save('trace-plan.json',dict(source_snapshot=pin(ROOT/'evidence/04A38/preedit/source/UDFdataImport/UdfDataImportMixedModel.vb.txt'),source_map=maps,case_kinds=variants,
      common_required_events=['spec.accepted','parser.entry','headerless.attempted','header.name_col','header.value_col','branch.headered_name_value','headered.row','helper.entry','helper.value'],
      result_expectation='UNKNOWN_NOT_CAPTURED; observe actual public result/exception, never infer from the source error literal',
      forbidden_witnesses=['branch.headerless_success','branch.wide','profile.formed','profile.applied','report.returned'],
      true_branch_probe_rule='guard event is injected immediately AFTER the original If...Then line, inside that true arm; first accepted setting must precede it'))
 for config in ['Debug','Release']:
  original=[json.loads(line) for line in (V3/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
  assert len(original)==2
  cases=[]
  for base in original:
   culture='Invariant' if base['culture']=='' else 'sk-SK'
   for suffix,variant in variants.items():
    case=copy.deepcopy(base);case['case_id']='AUTHORIZED-MMRM-DUPLICATE/'+culture+'/'+suffix
    case['reference_class']=CLASS;case['case_kind']=suffix;case['branch_expectation']=variant;case['expected_result']='UNKNOWN_NOT_CAPTURED'
    assert case['operations'][0]['arguments']==factory['fit_arguments']
    args=case['operations'][1]['arguments'];assert [a['name'] for a in args]==['handle','spec','alpha','at'] and args[1]['value']==factory['spec'] and args[2]['value']=={'kind':'Nothing'}
    values=[dict(kind='String',value='Name'),dict(kind='String',value='Value')]
    for name,bits in variant['settings']:values += [dict(kind='String',value=name),dict(kind='Double',bits=bits)]
    args[3]['value']=dict(kind='Array',element_type='System.Object',rank=2,lengths=[3,2],lower_bounds=[0,0],upper_bounds=[2,1],values=values)
    case['semantics']['ownership']='Fresh Object[,] per decoded argument/operation/case; no sharing of arrays between cases. Public parameters are not ByRef; full before/after argument values captured.'
    cases.append(case)
  assert len(cases)==len({c['case_id'] for c in cases})==4
  with (HERE/('inputs-'+config+'.jsonl')).open('x',encoding='utf-8',newline='\n') as stream:
   for case in cases:stream.write(json.dumps(case,ensure_ascii=False,separators=(',',':'),allow_nan=False)+'\n')
 recorders=[]
 for config in ['Debug','Release']:
  for mode in ['plain','trace']:
   folder=PRIVATE/('original-'+config+'-'+mode+'-v2');exe=folder/'MmrmCapture38.exe'
   result=command('recorder-'+config+'-'+mode,[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll',
       '/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'MmrmCapture38.cs')]);assert result['exit_code']==0
   recorders.append(dict(configuration=config,mode=mode,executable=library_pin(exe),command=clean(result)))
 save('materialization-receipt.json',dict(commands=[clean(compiled),clean(materialized)],fresh_factory=pin(HERE/'factory-inputs.json'),v3_factory=pin(V3/'factory-inputs.json'),factory_bytes_exact=True,
      original_case_provenance=pin(V3/'materialization-receipt.json'),original_public_argument_order=['handle','spec','alpha','at'],recorders=recorders,
      handle_contract='Raw actual GUID retained; only the predeclared HANDLE:MMRM:SESSION1 identity is symbolized; no other normalization',
      compiler_files=[library_pin(Path(compiler(n))) for n in ['vbc.exe','csc.exe']],python_version=sys.version,cases_per_configuration=4,
      source_fixture=pin(ROOT/'evidence/04A38/preedit/native-safety/FixtureInputs38.vb'),input_corrections=[],model_executions=0,query_executions=0))
 files=[pin(p) for p in sorted(HERE.iterdir()) if p.is_file()]
 save('input-seal.json',dict(status='INPUT_SEALED',sequence=1,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,reference_class=CLASS,
      cases_per_configuration=4,files=files,original_executions=0,current_executions=0,not_preedit_frozen=True))
 print('INPUT_SEALED: exactly4 approved typed cases/configuration; fresh original factory matches v3 bytes; no model or query execution')
if __name__=='__main__':main()
