"""Select already-executed accepted v17 regression identities; no test rerun."""
from common import *

rows=[];compiled=[]
for config in ['Debug','Release']:
    tests=load(HERE/('build-and-tests/'+config+'-identities-v1.json'))['tests']
    chosen=[r for r in tests if any(k in r['method'] for k in ['Covariance17','CovarianceClassifier17','Mmrm17NoIntercept','Median17ErrorCells'])]
    assert chosen and all(r['outcome']=='Passed' for r in chosen)
    for token in ['Covariance17Unknown','Covariance17ConversionException','Mmrm17NoIntercept','Median17ErrorCells']:
        assert any(token in r['method'] for r in chosen), 'TARGETED_REGRESSION_MISSING '+token
    rows.extend(chosen)
    methods=load(PRIVATE/config/'metadata/current.json')['methods']
    native=next(r for r in methods if r['member_name']=='NormalizeMixedModelResidualCovarianceName' and '@ BESHStatNG,' in r['method'])
    conversions=[i for i in native['instructions'] if i['op'] in ['call','callvirt'] and '.AsString(' in str(i['operand'])]
    assert len(conversions)==2,'TWO_NATIVE_CONVERSIONS_NOT_RETAINED'
    median=[r for r in methods if r['member_name']=='ComputeSurvivalMedianCi']
    assert median
    boxproof=[dict(method=r['method'],boxing=[dict(index=i,**ins) for i,ins in enumerate(r['instructions']) if ins['op']=='box']) for r in median]
    # Generic sentinel boxes are independently materialized at three outputs.
    assert any(len([i for i in r['boxing'] if 'TError' in str(i['operand']) or '!!' in str(i['operand'])])>=3 for r in boxproof),'THREE_INDEPENDENT_SENTINEL_BOXES_MISSING'
    compiled.append(dict(configuration=config,native_covariance_method=native['method'],conversion_instruction_count=len(conversions),median_boxing=boxproof,
        accepted_boundary_proof=pin(W2/'continuation-v17/conversion-and-alias-boundaries-v3.json'),
        complete_compiled_metadata=pin(PRIVATE/config/'metadata/current.json')))
save('abi-cil/targeted-regressions-v1.json',dict(status='FULL_CAMPAIGN_ACCEPTED_V17_REGRESSIONS_PASS',tests=rows,compiled=compiled,
    no_intercept_alias='Fresh full Windows adapter tests retain source array identity and null precedence; compiled boundary targets source-bound by complete closure proof.',
    coverage='Known/blank/unknown covariance tokens, stateful two-conversion and exception timing; independent error-sentinel boxing; no-intercept alias identity. No tests repeated.'))
print('Selected accepted regression occurrences',len(rows),flush=True)
