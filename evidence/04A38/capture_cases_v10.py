"""Explicit row permutation, duplicate labels and additional native value kinds.

Named parameters are independent. This is still a pre-production candidate;
there is no permission for a late reference after editing product source.
"""
import hashlib
import itertools
import json
from preflight import HERE, pin, save


def main():
    out=HERE/'preedit/attempts/capture-v10';out.mkdir(exist_ok=False);cases=[]
    def add(c):
        c=dict(c);c.pop('case_id',None);c['input_factory_version']='v10'
        if c['cohort']=='threshold_shape_value_policy':
            c['expected_stage']='ORIGINAL_THRESHOLD_BODY; each declared shape carries its declared value profile'
        c['case_id']=c['target']+'.'+hashlib.sha256(json.dumps(c,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24]
        cases.append(c)
    for line in (HERE/'preedit/attempts/capture-v9/cases.jsonl').read_text(encoding='utf-8').splitlines():add(json.loads(line))
    entry=json.loads((HERE/'entry-members.json').read_bytes())
    for e,culture in itertools.product(entry['exports'],['','sk-SK']):
        name=e['effective_excel_name']
        if not name.startswith(('BESH.MULTI.','BESH.REGR.','BESH.SURV.COX_','BESH.PS.','BESH.PSM.')):continue
        if name.endswith('_FIT') or (any(a['name']=='handle' for a in e['arguments']) and not name.endswith(('_DROP','_CLEAR_ALL')) and name!='BESH.PS.CLEANUP'):
            profiles=['reverse_rows','duplicate_labels'] if name.endswith('_FIT') else ['reverse_rows']
            for profile in profiles:
                add(dict(target_kind='export',target=name,culture=culture,cohort='explicit_model_row_and_label_identity',model_profile=profile,arguments={},
                    expected_stage='PUBLIC_ORIGINAL_FIT_OR_QUERY; duplicate label rejection stays an original negative case; reversed rows never imply physical worksheet coordinates'))
    scalar_pairs={}
    for c in cases:
        if c['cohort']=='scalar_and_native_marker_boundary':
            scalar_pairs[(c['type'],c['target'])]=next(iter(c['arguments']))
    for (owner,target),parameter in scalar_pairs.items():
        for culture,state in itertools.product(['','sk-SK'],['error_na','error_num','error_ref','single:-0','single:1.25','single:NaN','single:+Inf','single:-Inf','date:2000-01-02']):
            add(dict(target_kind='helper',type=owner,target=target,culture=culture,cohort='additional_native_value_kinds',arguments={parameter:state},input_state=state,
                expected_stage='DIRECT_ORIGINAL_MANAGED_HELPER; no Excel registration/marshaling execution claim'))
    assert len(cases)==len({c['case_id'] for c in cases})
    with (out/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
        for c in cases:f.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    (out/'Capture38.cs.txt').write_bytes((HERE/'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v10/specification.json',dict(status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED',rows=len(cases),
        cases=pin((out/'cases.jsonl').relative_to(HERE.parents[1]).as_posix()),generator=pin('evidence/04A38/capture_cases_v10.py'),recorder=pin('evidence/04A38/helpers/Capture38.cs'),
        independent_dimensions=['culture','export/helper','argument identity/type','row/column counts','header','missing/error gap','paired lengths and independent gaps','numeric bit profiles','options','matrix pattern and structure','row permutation','duplicate variable/category labels','handle comparer/type/lifecycle'],
        qualification='Duplicate-label cases call the real fitting export directly; a fit rejection is recorded as a negative case, not successful model-query coverage. All existing v9 originals and attempts remain immutable.',
        chronology='BEFORE_ANY_PRODUCT_EDIT',native='NOT_RUN',scientific='NOT_GOLDEN',
        readiness='Final source/coverage/ownership audit still required before product edits.'))
    print('Prepared',len(cases),'v10 pre-production cases; unsealed')


if __name__=='__main__':main()
