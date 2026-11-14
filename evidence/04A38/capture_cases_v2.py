"""Pre-product fixture correction and orthogonal import/output extensions.
Retains every pilot file; this is still an unsealed pre-edit execution attempt.
"""
import hashlib
import itertools
import json
import re
from pathlib import Path
from preflight import HERE,ROOT,pin,save

def main():
    previous=HERE/'preedit/attempts/capture-v1';out=HERE/'preedit/attempts/capture-v2';out.mkdir(exist_ok=False)
    entry=json.loads((HERE/'entry-members.json').read_bytes());exports={r['effective_excel_name']:r for r in entry['exports']}
    rows=[];excluded=[]
    def add(row):
        row=dict(row);row.pop('case_id',None);row['input_factory_version']='v2';row['case_id']=row['target']+'.'+hashlib.sha256(json.dumps(row,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24];rows.append(row)
    for line in (previous/'cases.jsonl').read_text(encoding='utf-8').splitlines():
        c=json.loads(line)
        if c['cohort']=='required_argument_states':
            a=next(a for a in exports[c['target']]['arguments'] if a['name']==c['varied_argument'])
            if a['type']=='As String' and c['input_state'] not in ['null','blank','whitespace'] and not c['input_state'].startswith('text:'):
                excluded.append(dict(case_id=c['case_id'],reason='Managed reflection binding is not native Excel typed-string coercion; native marshaling is source-only NOT_RUN. Original pilot result retained.'));continue
        add(c)
    for e,culture in itertools.product(entry['exports'],['','sk-SK']):
        names={a['name'] for a in e['arguments']}
        if 'includeHeader' in names:
            for flag in ['true','false','empty','missing']:
                add(dict(target_kind='export',target=e['effective_excel_name'],culture=culture,cohort='spill_header_option',arguments={'includeHeader':flag},input_state=flag,expected_stage='PUBLIC_QUERY_BODY; errors and unavailable outputs counted separately'))
        if e['effective_excel_name'] in ['BESH.REGR.GLM_PRED','BESH.REGR.GLMNB_PRED','BESH.REGR.ZIP_PRED','BESH.REGR.GEE_PRED']:
            arg='newCountX' if 'ZIP_' in e['effective_excel_name'] else 'newX'
            for state in ['matrix_profile:3:2:none:clean','matrix_profile:3:1:none:clean','zero_rows']:
                add(dict(target_kind='export',target=e['effective_excel_name'],culture=culture,cohort='explicit_prediction_matrix',arguments={arg:state},input_state=state,expected_stage='PUBLIC_PREDICTION_BODY'))
    # Full single-input import matrix: each dimension/first-row interpretation/
    # internal gap independent. These invoke original Windows pure array paths.
    names=['TryReadNumericColumn','TryReadNumericMatrix','TryReadGroupedNumericColumns','TryReadIndependentNumericColumns','ExtractNumericColumnIgnoringNonNumeric','ExtractCompleteNumericMatrixCompleteCases']
    for name,culture,n,p,header,gap in itertools.product(names,['','sk-SK'],[0,1,2,4],[0,1,2,3],['none','text','numeric_text','blank'],['clean','blank','error','NaN']):
        member=next(m for m in entry['members'] if m['name']==name and m['kind']=='FunctionBlock')
        first=re.search(r'\bFunction\s+\w+\s*\(\s*(\w+)\s+As',member['signature']);assert first,name
        state=f'matrix_profile:{n}:{p}:{header}:{gap}'
        add(dict(target_kind='helper',type=member['containing_type'],target=name,culture=culture,cohort='independent_import_shape_header_gap',arguments={first.group(1):state},rows=n,columns=p,header=header,gap=gap,expected_stage='ORIGINAL_ARRAY_IMPORT_BODY; no native references'))
    for name,culture,nx,ny,header,gx,gy in itertools.product(['TryReadPairedNumericColumns','ExtractPairedNumericColumnsIgnoringNonNumeric'],['','sk-SK'],[1,2,4],[1,2,4],['none','text','numeric_text'],['clean','blank'],['clean','second_gap']):
        member=next(m for m in entry['members'] if m['name']==name and m['kind']=='FunctionBlock')
        inputs=re.findall(r'(\w+)\s+As Object',member['signature']);assert len(inputs)>=2,name
        args={inputs[0]:f'matrix_profile:{nx}:1:{header}:{gx}',inputs[1]:f'matrix_profile:{ny}:1:{header}:{gy}'}
        add(dict(target_kind='helper',type=member['containing_type'],target=name,culture=culture,cohort='independent_paired_lengths_gaps',arguments=args,x_rows=nx,y_rows=ny,header=header,x_gap=gx,y_gap=gy,expected_stage='ORIGINAL_PAIRED_ARRAY_BODY; legacy source/position semantics retained'))
    for culture,shape,state in itertools.product(['','sk-SK'],['column','row','matrix'],['ordinary','blank','duplicates','near_duplicates','invalid']):
        vector={'ordinary':'vector:0.75,0.25,0.5','blank':'vector:null,null','duplicates':'vector:0.5,0.5,0.25','near_duplicates':'vector:0.25,0.2500000000001,0.2500000000002','invalid':'vector:0.25,1.1'}[state]
        add(dict(target_kind='helper',type='BESHStatNG.UdfDataImport',target='TryGetOptionalProbabilityThresholds',culture=culture,cohort='threshold_shape_value_policy',arguments={'arg':vector if shape=='column' else shape},shape=shape,value_profile=state,expected_stage='ORIGINAL_THRESHOLD_BODY; only column profile currently carries varied values'))
    # Above non-column threshold witnesses intentionally remain declared as a
    # coverage gap until explicit row/matrix value decoding is supplied. Do not
    # mistake an unchanged generic row for a full orthogonal value crossing.
    assert len(rows)==len({r['case_id'] for r in rows})
    with (out/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
        for r in rows:f.write(json.dumps(r,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    (out/'Capture38.cs.txt').write_bytes((HERE/'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v2/specification.json',dict(status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED',rows=len(rows),cases=pin((out/'cases.jsonl').relative_to(ROOT).as_posix()),generator=pin('evidence/04A38/capture_cases_v2.py'),recorder=pin('evidence/04A38/helpers/Capture38.cs'),
        predecessor=pin((previous/'specification.json').relative_to(ROOT).as_posix()),excluded_native_typed_binding_cases=excluded,
        retained_legacy_error_candidate='FA_FACTORABILITY writes six diagnostic rows into a five-body-row allocation; original IndexOutOfRange preserved, never corrected here. Register for04A42.',
        known_matrix_gaps=['Threshold value-profile changes are not yet encoded in non-column shapes; independent audit must reject a final freeze until corrected.','Member ownership remains OPEN; output/numeric/categorical/cache coverage extensions still required.'],
        corrections='Pure synthetic INPUT factory only: typed Double/String arguments, binary GEE/classification response, aligned BoxM rows, explicit ordinal/nested/trend/contrast/spec input profiles. No production edit or expected-value replacement.',native='NOT_RUN',scientific='NOT_GOLDEN'))
    print('Prepared',len(rows),'pre-edit v2 cases; declared coverage gaps remain; no final freeze')

if __name__=='__main__':main()
