"""Correct argument bindings before any v3 target execution; expand paired matrices."""
import hashlib
import itertools
import json
from preflight import HERE,pin,save

def main():
    out=HERE/'preedit/attempts/capture-v4';out.mkdir(exist_ok=False);cases=[]
    def add(c):
        c=dict(c);c.pop('case_id',None);c['input_factory_version']='v4';c['case_id']=c['target']+'.'+hashlib.sha256(json.dumps(c,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24];cases.append(c)
    previous=HERE/'preedit/attempts/capture-v3'
    for line in (previous/'cases.jsonl').read_text(encoding='utf-8').splitlines():
        c=json.loads(line)
        if c['cohort']=='numeric_matrix_branches':continue
        add(c)
    for culture,n,p,pattern,structure in itertools.product(['','sk-SK'],[0,1,2,4],[0,1,2,3],['ordinary','zero','signed_zero','nonfinite'],['ordinary','zero','signed_zero','nonfinite']):
        add(dict(target_kind='helper',type='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs',target='ColumnSumsOfSquares',culture=culture,cohort='pattern_structure_independent',
            arguments={'pattern':f'double_matrix:{n}:{p}:{pattern}','strctre':f'double_matrix:{n}:{p}:{structure}'},rows=n,columns=p,pattern_profile=pattern,structure_profile=structure,expected_stage='ORIGINAL_PATTERN_TIMES_STRUCTURE_HELPER'))
    for culture,pattern,structure in itertools.product(['','sk-SK'],['null','double_matrix:2:3:ordinary'],['null','double_matrix:1:3:ordinary','double_matrix:2:1:ordinary']):
        add(dict(target_kind='helper',type='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs',target='ColumnSumsOfSquares',culture=culture,cohort='pattern_structure_missing_mismatched',arguments={'pattern':pattern,'strctre':structure},expected_stage='ORIGINAL_NUMERIC_GUARD_OR_EXCEPTION'))
    assert len(cases)==len({c['case_id'] for c in cases})
    with (out/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
        for c in cases:f.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    (out/'Capture38.cs.txt').write_bytes((HERE/'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v3/construction-audit.json',dict(status='INVALID_PARTIAL_SPEC_NOT_EXECUTED',error='ColumnSumsOfSquares requires pattern(,) and strctre(,), not a mat argument',product_edits=False,
        correction='Separate v4 case set uses exact original signature and independently crossed pattern/structure profiles; all v3 source/spec files retained.'))
    save('preedit/attempts/capture-v4/specification.json',dict(status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED',rows=len(cases),cases=pin((out/'cases.jsonl').relative_to(HERE.parents[1]).as_posix()),
        recorder=pin('evidence/04A38/helpers/Capture38.cs'),generator=pin('evidence/04A38/capture_cases_v4.py'),
        prior_clock_policy=pin('evidence/04A38/preedit/attempts/capture-v3/specification.json'),
        independent_matrix='culture x rows x columns x pattern profile x structure profile, plus missing/mismatched inputs; exact actual original bindings',
        native='NOT_RUN',scientific='NOT_GOLDEN',known_gaps=['Ownership and essential complete-case/categorical/handle/input-identity/copy/result coverage audits remain incomplete; no product edit authorized by this attempt.']))
    print('Prepared',len(cases),'v4 pre-edit cases; original pattern/structure semantics; not a final freeze')

if __name__=='__main__':main()
