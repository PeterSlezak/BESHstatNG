"""Read-only historical reference/input/recorder/trace authority audit."""
from common import *
import concurrent.futures,csv
from collections import Counter

core=load(W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json')
prefixes=['evidence/04A38/preedit/','evidence/04A38/late-archived-entry-supplement/']
rows=[{k:r[k] for k in ['path','bytes','sha256']} for r in core['files'] if any(r['path'].startswith(p) for p in prefixes)]
def check(r):
    actual=pin(ROOT/r['path'])
    return dict(expected=r,actual=actual,exact=actual==r)
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:checked=list(pool.map(check,rows))
assert all(r['exact'] for r in checked),'ORIGINAL_REFERENCE_AUTHORITY_CHANGED'
sealed=load(W2/'continuation-v12/compatibility-packet-v1.json')
bound={(r['configuration'],r['corpus']):r for r in sealed['results']}
cross=[]
for config in ['Debug','Release']:
    current=load(HERE/('compatibility/'+config+'-comparisons-v1.json'))
    identities=[]
    for r in current['results']:
        prior=bound[config,r['corpus']]
        for k in ['original','inputs','recorder','cases']:assert r[k]==prior[k], 'SEALED_REFERENCE_REPLACED '+config+' '+r['corpus']+' '+k
        with (ROOT/r['current']['path']).open(encoding='utf-8-sig',newline='') as f:
            parsed=list(csv.DictReader(f,delimiter='\t'))
        keys=[(r['corpus'],x['case_id']) for x in parsed]
        assert len(keys)==len(set(keys))==r['cases']
        assert set(parsed[0])==({'case_id','input_sha256','operations'} if r['corpus'] in list('ABCDEFG') else {'case_id','input_sha256','outcome','phase','result','byref_arguments','error','caught_error_events'})
        identities.extend(dict(corpus=r['corpus'],case_id=x['case_id'],input_sha256=x['input_sha256'],
            provenance=r['provenance'],row_sha256=sha(json.dumps(x,sort_keys=True).encode()),schema=list(x)) for x in parsed)
        cross.append(dict(configuration=config,corpus=r['corpus'],sealed_original=r['original'],
            sealed_inputs=r['inputs'],recorder=r['recorder'],cases=r['cases'],exact=True,
            current=r['current'],provenance=r['provenance']))
    assert len(identities)==29424
    save('compatibility/'+config+'-case-identities-v2.json',dict(configuration=config,entries=identities,distinct=29424))
save('compatibility/reference-authority-audit-v2.json',dict(status='ACTUAL_HISTORICAL_REFERENCE_INPUT_RECORDER_AND_TRACE_BYTES_UNCHANGED',
    checked=checked,files=len(checked),sealed_Release_and_Debug_crosswalk=cross,
    sealed_baseline=pin(W2/'continuation-v12/compatibility-packet-v1.json'),
    original_queries=0,scope='Every reference payload under the two historical corpus roots in accepted core inventory rehashed live. Includes retained original-source captures, input, plain/repeat/trace receipts and branch witnesses present in that inventory. Protected private captures separately covered by full live union audit; unavailable external reviewer artifact not manufactured.'))
print('Historical reference live files',len(checked),'Debug and Release distinct cases29424 each; sealed references exact',flush=True)
