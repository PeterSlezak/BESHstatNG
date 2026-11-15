"""Occurrence/order/typed-row readback independent of driver prose labels."""
from common40_v3 import *
import collections,csv
lock()
for config in ('Debug','Release'):
    raw=PRIVATE/('compatibility/compatibility-a40-'+config+'-v1.json');d=load(raw);assert d['new_original_queries']==0
    previous=load(ROOT/('evidence/04A39/compatibility/'+config+'-retained-v1.json'))
    rows=[];corpora=[]
    for r in d['results']:
        for key in ('current','original','inputs','recorder'):assert pin(r[key]['path'])==r[key]
        a=(ROOT/r['original']['path']).read_bytes();b=(ROOT/r['current']['path']).read_bytes();assert a==b
        text=b.decode('utf-8-sig').splitlines();header=next(csv.reader([text[0]],delimiter='\t'));parsed=list(csv.DictReader(text,delimiter='\t'))
        assert len(parsed)==r['cases']
        for line,row in zip(text[1:],parsed):
            rows.append(dict(corpus=r['corpus'],case_id=row['case_id'],input_sha256=row.get('input_sha256'),
               provenance='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in list('ABCDEFG') else 'PRE_EDIT_FROZEN',row_sha256=sha(json.dumps(row,sort_keys=True).encode()),schema=header))
        corpora.append(dict(configuration=config,corpus=r['corpus'],cases=r['cases'],exact=True,original=r['original'],current=r['current'],inputs=r['inputs'],recorder=r['recorder'],comparison='EXACT_BYTES_AND_TYPED_ROWS_IN_ORDER',current_exit=r['command']['exit_code'],original_queries=0))
    assert len(rows)==len({(r['corpus'],r['case_id']) for r in rows})==29424
    assert collections.Counter(r['provenance'] for r in rows)==dict(PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458)
    # Every serialized row/schema/order/input remains the independently retained historical identity.
    assert rows==previous['case_identities']
    save('compatibility/'+config+'-current-only-v3.json',dict(status='FRESH_CURRENT_ONLY_29424_EXACT',configuration=config,results=corpora,case_identities=rows,
        original_queries=0,driver=pin(raw),baseline=pin(ROOT/('evidence/04A39/compatibility/'+config+'-retained-v1.json')),
        inherited_driver_fields='Raw historical recorder writes deferred-category fields outside its selected configuration; these fields are not used as40 category results. Actual builds/ABI/tests have separate receipts.',
        error_array_byref_trace_semantics='All serialized result/byref/error/caught-error event columns exact; no row omissions or reordering. Original plain/repeat/trace authority retained, never executed.'))
    print(config,'29424 ordered typed case identities exact;28966+458 segregated',flush=True)
