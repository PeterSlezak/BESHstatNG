"""Complete the observed callable reachability matrix, still pre-production."""
import hashlib
import itertools
import json
from preflight import HERE, pin, save


def main():
    out=HERE/'preedit/attempts/capture-v9';out.mkdir(exist_ok=False);cases=[]
    def add(c):
        c=dict(c);c.pop('case_id',None);c['input_factory_version']='v9'
        c['case_id']=c['target']+'.'+hashlib.sha256(json.dumps(c,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24]
        cases.append(c)
    for line in (HERE/'preedit/attempts/capture-v8/cases.jsonl').read_text(encoding='utf-8').splitlines():add(json.loads(line))
    for culture,profile in itertools.product(['','sk-SK'],['at_mmrm_name_value','at_mmrm_wide','text:treatment_active','column','matrix']):
        add(dict(target_kind='export',target='BESH.REGR.MMRM_LSMEANS',culture=culture,
                 cohort='observed_design_lsmeans_profiles',arguments={'group':profile},profile=profile,
                 expected_stage='PUBLIC_ORIGINAL_LSMEANS_RANGE_OR_NAME_PROFILE; no physical worksheet identity inferred'))
    assert len(cases)==len({c['case_id'] for c in cases})
    with (out/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
        for c in cases:f.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    (out/'Capture38.cs.txt').write_bytes((HERE/'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v9/specification.json',dict(status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED',rows=len(cases),
        cases=pin((out/'cases.jsonl').relative_to(HERE.parents[1]).as_posix()),generator=pin('evidence/04A38/capture_cases_v9.py'),recorder=pin('evidence/04A38/helpers/Capture38.cs'),
        correction='v8 showed640/643 callable entries. Add public observed-design MMRM LSMEANS profiles for remaining3 methods; serialize typed cache KeyValuePair properties rather than OPAQUE.',
        chronology='BEFORE_ANY_PRODUCT_EDIT',native='NOT_RUN',scientific='NOT_GOLDEN',
        readiness='Requires exact uninstrumented Debug/Release originals, same-config repeatability, trace neutrality, independent essential-cell audit and ownership review before a final freeze.'))
    print('Prepared',len(cases),'v9 pre-production cases; unsealed')


if __name__=='__main__':main()
