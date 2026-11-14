"""Pre-production public primitive scalar/typed-string coverage.

The original required-argument corpus varied Object arguments. Typed Double
declarations need their own reached-body cases, rather than reflection binding
errors from sending Excel sentinels to CLR Double parameters.
"""
import hashlib
import itertools
import json
from preflight import HERE,ROOT,BASE,git,pin,save


def main():
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    assert git('rev-parse','HEAD').decode().strip()==BASE
    folder=HERE/'preedit/attempts/public-primitives-v1';folder.mkdir(exist_ok=False)
    census=json.loads((HERE/'entry-members.json').read_bytes());cases=[];parameters=[]
    values={
        'As Double':['number:-0','number:0','number:-1','number:0.25','number:1','number:2','number:10','number:20','number:NaN','number:+Inf','number:-Inf'],
        'As String':['null','blank','whitespace','text:two-sided','text:less','text:greater','text:invalid-04A38'],
        'As Integer':['int:-1','int:0','int:1','int:3'],
        'As Boolean':['false','true'],
    }
    for export in census['exports']:
        for parameter in export['arguments']:
            if parameter['type'] not in values:continue
            parameters.append(dict(export=export['effective_excel_name'],parameter=parameter))
            for culture,state in itertools.product(['','sk-SK'],values[parameter['type']]):
                c=dict(target_kind='export',target=export['effective_excel_name'],culture=culture,
                    cohort='typed_public_scalar_body',arguments={parameter['name']:state},varied_argument=parameter['name'],input_state=state,
                    source_parameter=parameter,input_factory_version='v10',
                    expected_stage='ORIGINAL_PUBLIC_MANAGED_BODY; native Excel coercion/registration NOT_RUN; guard and successful numerical outcomes accounted separately')
                c['case_id']=c['target']+'.'+hashlib.sha256(json.dumps(c,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24];cases.append(c)
    assert len(cases)==len({c['case_id'] for c in cases})
    with (folder/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as stream:
        for c in cases:stream.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    recorder=HERE/'preedit/frozen-v2/Capture38.cs'
    (folder/'Capture38.cs.txt').write_bytes(recorder.read_bytes())
    save('preedit/attempts/public-primitives-v1/specification.json',dict(status='PRE_EDIT_PUBLIC_PRIMITIVE_CANDIDATE_NOT_SEALED',rows=len(cases),parameters=parameters,
        source_commit=BASE,cases=pin((folder/'cases.jsonl').relative_to(ROOT).as_posix()),recorder=pin(recorder.relative_to(ROOT).as_posix()),generator=pin('evidence/04A38/primitive_export_cases.py'),
        chronology='BEFORE_FIRST_04A38_PRODUCT_EDIT; first valid original-v1 freeze preserved',
        limitation='These are direct managed-body inputs. No claim that live Excel marshals nonfinite Double or null String values in this form.',
        native='NOT_RUN',scientific='NOT_GOLDEN',remaining_gate='Execute both configurations and audit genuine target reachability/exact repeated results before sealing.'))
    print('Prepared',len(cases),'typed public scalar cases;',len(parameters),'independent parameters; before product edits')


if __name__=='__main__':main()
