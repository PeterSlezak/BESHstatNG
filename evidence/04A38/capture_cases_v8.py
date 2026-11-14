"""Fill independently observed call-reachability gaps before product edits."""
import hashlib
import itertools
import json
import re
from preflight import HERE, pin, save


def main():
    out = HERE / 'preedit/attempts/capture-v8'
    out.mkdir(exist_ok=False)
    cases = []
    def add(c):
        c = dict(c)
        c.pop('case_id', None)
        c['input_factory_version'] = 'v8'
        c['case_id'] = c['target'] + '.' + hashlib.sha256(json.dumps(c, sort_keys=True, separators=(',', ':')).encode()).hexdigest()[:24]
        cases.append(c)
    for line in (HERE / 'preedit/attempts/capture-v7/cases.jsonl').read_text(encoding='utf-8').splitlines():
        add(json.loads(line))
    def helper(owner, target, arguments, cohort, **descriptor):
        for culture in ['', 'sk-SK']:
            add(dict(target_kind='helper', type='BESHStatNG.' + owner, target=target,
                     culture=culture, cohort=cohort, arguments=arguments,
                     expected_stage='DIRECT_ORIGINAL_HELPER; public shipping reachability reported separately', **descriptor))
    for target, parameter in [('TryGetLooseNumericVector','input'), ('TryGetStringVector','arg'), ('TryGetDoubleVector','arg'), ('TryGetRequiredOffsetPredictionInputs','newOffset'), ('TryGetOneWayIccGroups','input')]:
        for state in ['null','empty','missing','error','blank','text:1,2,3','text:1; 2; -0','text:1,5;2,5','text:dup;dup; ','text:NaN;+Inf;-Inf','row','column','matrix','zero_rows','zero_columns','object_cells']:
            helper('UdfDataImport',target,{parameter:state},'additional_vector_alignment_helpers',input_state=state)
    for state, required in itertools.product(['null','missing','row','column','matrix','vector:1,null,3'], ['false','true']):
        helper('UdfDataImport','TryGetInterceptOnlyPredictionInputs',{'newOffset':state,'requireOffset':required},'intercept_only_prediction',input_state=state,require_offset=required)
    for state in ['row','column','matrix','zero_rows','zero_columns','object_cells']:
        helper('UdfDataImport','TryReadStrictNumericVectorFromSingleRow',{'arr':state},'strict_single_row',input_state=state)
    for state, rows in itertools.product(['matrix_profile:4:2:text:none','matrix_profile:4:2:numeric_text:blank','zero_rows','matrix'], [0,2,3,4]):
        helper('UdfDataImport','TryGetTrimmedObjectMatrixAligned',{'input':state,'expectedRows':'int:'+str(rows),'defaultNamePrefix':'text:V'},'aligned_object_matrix',input_state=state,expected_rows=rows)
    for owner,target in [('WorksheetFunctions.MixedModelUDFs','DefaultNames'),('UdfDataImport','DefaultMmrmNames')]:
        for count,prefix in itertools.product([-1,0,1,3],['null','text:X','text: \u03bc ']):
            helper(owner,target,{'count':'int:'+str(count),'prefix':prefix},'default_names',count=count,prefix=prefix)
    for include,authored,total in itertools.product(['true','false'],[0,1,2],[0,1,3]):
        helper('WorksheetFunctions.LmmUDFs','DefaultRandomCovarianceName',{'includeRandomIntercept':include,'authoredRandomColumnCount':'int:'+str(authored),'totalRandomColumnCount':'int:'+str(total)},'random_covariance_default',intercept=include,authored=authored,total=total)
    for state,header in itertools.product(['null','doubles:','doubles:-0,1,NaN,+Inf,-Inf'], ['false','true']):
        helper('UdfRegressionOutput','BuildResidualVectorOutput',{'vec':state,'header':'text: Residual ','includeHeader':header},'residual_spill_vector',input_state=state,header=header)
    for state,labels,header in itertools.product(['null','double_matrix:0:2:ordinary','double_matrix:3:2:ordinary','double_matrix:3:2:nonfinite'],['null','strings:A','strings:A,B'],['false','true']):
        helper('UdfRegressionOutput','BuildResidualMatrixOutput',{'mat':state,'headers':labels,'includeHeader':header},'residual_spill_matrix',input_state=state,labels=labels,header=header)
    helper('UdfOutputTables','BuildSimpleNoteTable',{'label':'text: \u03bc\nLabel ','value':'text:dup'},'simple_note_spill')
    for text,index in itertools.product(['null',"text:foo('a''b', bar(x)) tail",'text:foo(unclosed','text:plain'],[-1,0,3,99]):
        helper('WorksheetFunctions.RegressionFormulaUDFs','ExtractBalancedFunctionCall',{'text':text,'startIndex':'int:'+str(index)},'formula_diagnostic_fragment',input_state=text,index=index)
    for state,t,col in itertools.product(['null','double_matrix:0:3:ordinary','double_matrix:3:3:ordinary','double_matrix:3:3:nonfinite'],['number:-1','number:0','number:10','number:NaN'],[-1,0,1,2,3]):
        helper('WorksheetFunctions.CoxUDFs','LookupStepValue',{'arr':state,'t':t,'col':'int:'+str(col)},'step_lookup',input_state=state,time=t,column=col)
    entry=json.loads((HERE/'entry-members.json').read_bytes())
    source=next(r for r in entry['members'] if r['name']=='ApplyOption' and 'PropensityScore' in r['containing_type'])
    keys=[]
    for line in source['body_tokens'].split(' Case ')[1:]:
        prefix=line.split(' o . ')[0].split(' fitOptions . ')[0].split(' EnsureCoarseningSpec')[0]
        keys.extend(re.findall(r'"([a-z]+)"',prefix))
    for key,value in itertools.product(sorted(set(keys)),['text:true','text:2','text:invalid-04A38','text:NaN']):
        helper('WorksheetFunctions.PropensityScoreUDFs','ApplyOption',{'fitOptions':'new_psm_options','key':'text:'+key,'value':value},'psm_option_mutation',key=key,value=value)
    for culture,target,at in itertools.product(['','sk-SK'],['BESH.REGR.GEE_LSMESTIMATE','BESH.REGR.MMRM_LSMESTIMATE'],['name_value','wide']):
        add(dict(target_kind='export',target=target,culture=culture,cohort='real_model_at_profile',arguments={'at':'at_'+('gee' if 'GEE' in target else 'mmrm')+'_'+at},layout=at,expected_stage='REAL_ORIGINAL_FIT_THEN_PUBLIC_AT_PROFILE_QUERY'))
    for culture,target in itertools.product(['','sk-SK'],['BESH.SSIZE.TTEST_UNPAIRED','BESH.SSIZE.PROP_INDEP']):
        add(dict(target_kind='export',target=target,culture=culture,cohort='equivalence_planning_success',arguments={'hypothesisType':'text:equivalence','margin':'number:0.3',**({'meanDifference':'number:0.05'} if 'TTEST' in target else {'controlProportion':'number:0.4','experimentalProportion':'number:0.45'})},expected_stage='PUBLIC_EQUIVALENCE_RESULT_TABLE_BRANCH'))
    assert len(cases)==len({c['case_id'] for c in cases})
    with (out/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
        for c in cases:f.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    (out/'Capture38.cs.txt').write_bytes((HERE/'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v8/specification.json',dict(status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED',rows=len(cases),
        cases=pin((out/'cases.jsonl').relative_to(HERE.parents[1]).as_posix()),generator=pin('evidence/04A38/capture_cases_v8.py'),recorder=pin('evidence/04A38/helpers/Capture38.cs'),
        source_reachability_reason='v7 reached612 of643 callable source methods. v8 adds explicit missing source-helper and public AT/equivalence branch inputs. Native-only range branches remain NOT_RUN.',
        psm_option_keys=sorted(set(keys)),chronology='BEFORE_ANY_PRODUCT_EDIT',native='NOT_RUN',scientific='NOT_GOLDEN',readiness='Independent branch coverage/ownership audit and final freeze remain required.'))
    print('Prepared',len(cases),'v8 pre-production cases; unsealed')


if __name__=='__main__':main()
