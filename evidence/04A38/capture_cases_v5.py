"""Complete first-argument option naming and additional model/source/math cells.
Still before product edits. Source-derived input aliases, never expected results.
"""
import hashlib
import itertools
import json
import re
from preflight import HERE,pin,save

def main():
    out=HERE/'preedit/attempts/capture-v5';out.mkdir(exist_ok=False);cases=[]
    entry=json.loads((HERE/'entry-members.json').read_bytes())
    def add(c):
        c=dict(c);c.pop('case_id',None);c['input_factory_version']='v5';c['case_id']=c['target']+'.'+hashlib.sha256(json.dumps(c,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24];cases.append(c)
    for line in (HERE/'preedit/attempts/capture-v4/cases.jsonl').read_text(encoding='utf-8').splitlines():add(json.loads(line))
    parser_rows=[]
    for r in entry['members']:
        if r['kind']!='FunctionBlock' or r['exported'] or not (r['name'].startswith(('Parse','Normalize')) or r['name'] in ['GetHistogramBinRule','GetRegressionFamilyCode','GetRegressionLinkName','GetCoxTieMethod','GetFormulaAddressingMode','ResolveSampleSizeHypothesisType']):continue
        first=re.search(r'\bFunction\s+\w+\s*\(\s*(?:ByVal\s+)?(\w+)\s+As\s+(Object|String)',r['signature'])
        if not first:continue
        tokens=sorted(set(s.replace('""','"') for s in re.findall(r'"((?:[^"\n]|"")*)"',r['body_tokens'])))
        args_extra={}
        if 'analysisLabel As String' in r['signature']:args_extra['analysisLabel']='text:MMRM'
        if 'defaultValue As String' in r['signature'] and 'Optional defaultValue' not in r['signature']:args_extra['defaultValue']='text:DEFAULT'
        for culture,state in itertools.product(['','sk-SK'],['null','blank','whitespace','text:invalid-04A38','text:\uff2d\uff2c']+['text:'+t for t in tokens]):
            add(dict(target_kind='helper',type=r['containing_type'],target=r['name'],culture=culture,cohort='all_named_option_inputs',arguments={first.group(1):state,**args_extra},input_state=state,source_member=r['symbol_id'],expected_stage='ORIGINAL_OPTION_BRANCH; source arg naming independent of case ID'))
        parser_rows.append(dict(member=r['symbol_id'],parameter=first.group(1),literal_inputs=tokens))
    vector_methods=[r for r in entry['members'] if r['kind']=='FunctionBlock' and r['name'] in ['ComputeMultinomialProbabilities','CloneDoubleArray','CloneStringArray']]
    for r in vector_methods:
        m=re.search(r'\(\s*(\w+)\s*\(\)\s+As\s+(Double|String)',r['signature']);assert m
        states=['null','doubles:','doubles:1,2,3','doubles:-0,-0','doubles:NaN,+Inf,-Inf','doubles:-1,-2,-3'] if m.group(2)=='Double' else ['null','strings:dup,dup, \u03bc ,','strings:']
        for culture,state in itertools.product(['','sk-SK'],states):
            add(dict(target_kind='helper',type=r['containing_type'],target=r['name'],culture=culture,cohort='additional_typed_vector_helpers',arguments={m.group(1):state},input_state=state,expected_stage='DIRECT_ORIGINAL_VECTOR_HELPER'))
    for culture,n,p,row,intercept,beta,offset in itertools.product(['','sk-SK'],[0,1,3],[0,1,2],[-1,0,2],['true','false'],['null','doubles:','doubles:0.25,-0.5,1.5'],['null','doubles:','doubles:0.1,0.2,0.3']):
        add(dict(target_kind='helper',type='BESHStatNG.UdfRegressionOutput',target='ComputeLinearPredictor',culture=culture,cohort='independent_linear_predictor_dimensions',
                 arguments={'expandedX':f'double_matrix:{n}:{p}:ordinary','rowIndex':'int:'+str(row),'beta':beta,'includeIntercept':intercept,'offsetVals':offset},rows=n,columns=p,row_index=row,include_intercept=intercept,beta_profile=beta,offset_profile=offset,expected_stage='ORIGINAL_NUMERIC_LOOP; exact guards/errors separate'))
    for e,culture,profile in itertools.product(entry['exports'],['','sk-SK'],['header_rows','excluded_rows']):
        if not e['effective_excel_name'].startswith(('BESH.MULTI.','BESH.REGR.','BESH.SURV.COX_','BESH.PS.','BESH.PSM.')):continue
        if not any(a['name']=='handle' for a in e['arguments']):continue
        if e['effective_excel_name'].endswith(('_DROP','_CLEAR_ALL')) or e['effective_excel_name']=='BESH.PS.CLEANUP':continue
        add(dict(target_kind='export',target=e['effective_excel_name'],culture=culture,cohort='model_header_and_exclusion_identity',model_profile=profile,arguments={},expected_stage='PUBLIC_QUERY_AFTER_PROFILE_SPECIFIC_REAL_FIT; setup failures never count as queries'))
    assert len(cases)==len({c['case_id'] for c in cases})
    with (out/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
        for c in cases:f.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    (out/'Capture38.cs.txt').write_bytes((HERE/'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v5/specification.json',dict(status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED',rows=len(cases),cases=pin((out/'cases.jsonl').relative_to(HERE.parents[1]).as_posix()),generator=pin('evidence/04A38/capture_cases_v5.py'),recorder=pin('evidence/04A38/helpers/Capture38.cs'),parsers=parser_rows,
        prior_gap='Original pilot option selector matched only arg/value/s/text parameter names, omitting v/mode and similar names. This correction is before ANY product edit; all pilot originals retained.',
        metadata_control_policy=pin('evidence/04A38/preedit/attempts/capture-v3/specification.json'),
        native='NOT_RUN; wizard-false/blocked-coercion test doubles only',scientific='NOT_GOLDEN',known_gaps=['Full independent reachability/ownership/handle/copy/result audit and final freeze still required; no product edit authorized by this attempt.']))
    print('Prepared',len(cases),'v5 cases;',len(parser_rows),'option helpers; pre-edit/unsealed')

if __name__=='__main__':main()
