"""Materialize .NET factory inputs, approve no extra cases, then freeze bytes."""
import json,sys
from datetime import datetime,timezone
from pathlib import Path
from inspect_entry import ROOT,HERE,BASE,CLASS,pin,save
from build_original import PRIVATE,compiler,library_pin
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m,PRIVATE as RUNROOT
from baseline_report import clean

def typed(value):
    if isinstance(value,str):return dict(kind='String',value=value)
    assert type(value) is float
    import struct
    return dict(kind='Double',bits=struct.pack('>d',value).hex().upper())

def main():
    folder=PRIVATE/'original-Debug-plain';exe=folder/'MaterializeGee38.exe'
    facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
    runner=m.Runner(RUNROOT)
    args=[compiler('csc.exe'),'/nologo','/target:exe','/main:MaterializeGee38','/reference:System.Core.dll','/reference:System.Web.Extensions.dll',
          '/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'MaterializeGee38.cs'),
          str(ROOT/'evidence/04A38/preedit/frozen-v2/Capture38.cs')]
    compiled=runner.command('gee38-v2-materialize-compile',args);assert compiled['exit_code']==0
    executed=runner.command('gee38-v2-materialize-inputs',[str(exe),str(folder),str(HERE/'factory-inputs.json')]);assert executed['exit_code']==0
    factory=json.loads((HERE/'factory-inputs.json').read_bytes());assert factory['execution']=='INPUT_FACTORY_ONLY_NO_MODEL_OR_QUERY_EXECUTION'
    proposal=json.loads((ROOT/'evidence/04A38/late-archived-entry-supplement/v1/proposed-gee-profile-decision.json').read_bytes())
    assert len(proposal['cases'])==12
    trace=json.loads((HERE/'compile-original-Debug-trace.json').read_bytes())['instrumentation']
    # Expected semantic milestones are fixed from original source, not outputs.
    plan={
      'wide-time':dict(required=['parser.wide=True','selection.time=time','selection.time_value=D3FF0000000000000','profile.formed=True','profile.applied=True'],classification='POSITIVE_PROFILE_SELECTION'),
      'name-value-time':dict(required=['parser.name_value=True','helper.entry=time','selection.time=time','selection.time_value=D3FF0000000000000','profile.formed=True','profile.applied=True'],classification='POSITIVE_PROFILE_SELECTION'),
      'name-value-visit':dict(required=['parser.name_value=True','helper.entry=visit','selection.time=visit','selection.time_value=D3FF0000000000000','profile.formed=True','profile.applied=True'],classification='POSITIVE_PROFILE_SELECTION'),
      'duplicate-time-visit':dict(required=['parser.name_value=True','helper.entry=time','selection.time=time','helper.entry=visit','guard.duplicate_time=visit'],classification='INTENDED_GUARD'),
      'duplicate-design':dict(required=['parser.name_value=True','helper.entry=X1','selection.design=X1','helper.entry=x1','guard.duplicate_design=X1'],classification='INTENDED_GUARD'),
      'wide-multiple-rows':dict(required=['parser.wide=True','guard.wide_multiple_rows=True'],classification='INTENDED_GUARD')}
    save('trace-plan.json',dict(source_commit=BASE,source_snapshot=pin(ROOT/'evidence/04A38/preedit/source/Regression/GeeUDFs.vb.txt'),
         source_map=trace,case_kinds=plan,parameter_names=['Intercept','X1','X2'],
         positive_definition='Parser and helper reached; selected time/visit; formed and applied profile. A later statistical error is recorded separately from a returned report.',
         guard_definition='The specific intended source guard must execute; no profile formed/applied afterwards.',
         forbidden_early_errors=['at header minimum-row error','invalid handle','missing fitted design column X1']))
    for config in ['Debug','Release']:
        rows=[]
        for proposed in proposal['cases']:
            at=proposed['at'];r,c=at['lengths']
            assert at['rank']==2 and at['lower_bounds']==[0,0]
            arr=dict(kind='Array',element_type='System.Object',rank=2,lengths=[r,c],lower_bounds=[0,0],upper_bounds=[r-1,c-1],
                     values=[typed(v) for row in at['values'] for v in row])
            defaults={p['name']:p['value'] for p in factory['query_defaults']}
            values=[dict(kind='HandleReference',session='fresh-case'),factory['spec'],defaults['covarianceType'],defaults['alpha'],arr,defaults['scale']]
            arguments=[dict(name=n,value=v) for n,v in zip(proposed['argument_order'],values)]
            rows.append(dict(case_id=proposed['case_id'],culture='' if proposed['culture']=='Invariant' else 'sk-SK',configuration=config,
                public_entry=proposed['public_entry'],reference_class=CLASS,
                session=dict(lifetime='One successful real GEE_FIT followed by one GEE_LSMESTIMATE; fresh native cache per case',
                             creation_order=['clear private GEE cache','fit sealed inputs','verify actual cached model and X1 design identity','query sealed arguments'],
                             model_factory='Original frozen Capture38.PublicArgs(GEE_FIT, seed=true), ordinary profile; real_model_at_profile',
                             successful_prior_witness='BESH.REGR.GEE_LSMESTIMATE.b096886bae568b3f4bf601dd',
                             expected_parameter_names=['Intercept','X1','X2'],handle='Actual successful fit return; no synthetic handle'),
                semantics=dict(null='Nothing',missing='Only explicitly typed Nothing defaults; no ExcelMissing cells in these approved inputs',
                               blank='No blank at cells',excel_error='No input error cells',public_byref_parameters=[],
                               byref_observation='All arguments captured before and after invocation',float_encoding='Exact IEEE754 64-bit hexadecimal'),
                operations=[dict(role='PUBLIC_EXPORT',target='BESH.REGR.GEE_FIT',arguments=factory['fit_arguments']),
                            dict(role='PUBLIC_EXPORT',target='BESH.REGR.GEE_LSMESTIMATE',arguments=arguments)],
                expected_source_branch=plan[proposed['case_id'].split('/')[-1]],expected_result='UNKNOWN_NOT_CAPTURED'))
        assert len({row['case_id'] for row in rows})==12
        with (HERE/('inputs-'+config+'.jsonl')).open('x',encoding='utf-8',newline='\n') as stream:
            for row in rows:stream.write(json.dumps(row,ensure_ascii=False,separators=(',',':'),allow_nan=False)+'\n')
    save('materialization-receipt.json',dict(commands=[clean(compiled),clean(executed)],executable=library_pin(exe),
         sources=[pin(HERE/'MaterializeGee38.cs'),pin(ROOT/'evidence/04A38/preedit/frozen-v2/Capture38.cs')],model_executions=0,query_executions=0,
         proposed_input_corrections=[],design_name='X1 preserved exactly; original default raw predictor naming and BuildParameterNames source',
         handle_encoding='SESSION1 symbol bound to actual GUID handle with raw sidecar retained; no statistical normalization'))
    files=[p for p in sorted(HERE.iterdir()) if p.is_file()]
    save('input-seal.json',dict(status='INPUT_SEALED',sequence=1,sealed_utc=datetime.now(timezone.utc).isoformat(),reference_class=CLASS,
         source_commit=BASE,cases_per_configuration=12,cultures=['Invariant','sk-SK'],case_kinds=6,
         files=[pin(p) for p in files],original_reference_executions=0,current_comparisons=0,
         not_preedit_frozen=True,toolchain='VS18 Roslyn; Framework4.8; exact compiler hashes in compilation receipts'))
    print('INPUT_SEALED:12 approved typed cases per configuration; no model/query execution')

if __name__=='__main__':main()
