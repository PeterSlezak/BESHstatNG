"""Retain W3 compatibility only after source-bound per-config compiled equality; no execution claim."""
from common39 import *
from collections import Counter
import csv

source_lock();allrows=[]
for config in ['Debug','Release']:
    metadata=load(HERE/('abi-cil/'+config+'-comparison-'+('v3' if config=='Debug' else 'v2')+'.json'))
    assert metadata['exact_exports'] and not metadata['normalized_method_changes']
    assert metadata.get('types_contract_exact',metadata['types_exact'])
    source=W3/('compatibility/'+config+'-comparisons-v1.json');old=load(source)
    ids=load(W3/('compatibility/'+config+'-case-identities-v2.json'))
    assert ids['distinct']==len(ids['entries'])==29424
    assert len({(r['corpus'],r['case_id']) for r in ids['entries']})==29424
    rows=[];actualrows={}
    for corpus in old['results']:
        for field in ['original','current','inputs','recorder']:
            assert pin(corpus[field]['path'])==corpus[field], 'RETAINED_COMPATIBILITY_BYTE_CHANGED'
        original=ROOT/corpus['original']['path'];current=ROOT/corpus['current']['path']
        assert original.read_bytes()==current.read_bytes()
        # Preserve physical serialized order; quoted TSV rows may contain line breaks.
        with current.open(encoding='utf-8-sig',newline='') as stream:table=list(csv.reader(stream,delimiter='\t'))
        assert len(table)-1==corpus['cases']
        row=dict(corpus=corpus['corpus'],configuration=config,cases=corpus['cases'],original=corpus['original'],
            current=corpus['current'],inputs=corpus['inputs'],provenance=corpus['provenance'],
            header=table[0],comparison='RETAINED_W3_BYTE_EXACT;NOT_FRESH_04A39_EXECUTION',exact=True,
            ordered_rows_sha256=sha(json.dumps(table[1:],ensure_ascii=False,separators=(',',':')).encode()))
        rows.append(row);allrows.append(row)
    assert len(rows)==12 and sum(r['cases'] for r in rows)==29424
    assert old['PRE_EDIT_FROZEN']==28966 and old['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458
    save('compatibility/'+config+'-retained-v1.json',dict(status='RETAINED_IDENTICAL_SOURCE_BOUND_W3_EVIDENCE',
        configuration=config,cases=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,
        original_queries=0,current_queries_04A39=0,results=rows,case_identities=ids['entries'],
        W3_receipt=pin(source),compiled_gate=pin(HERE/('abi-cil/'+config+'-comparison-'+('v3' if config=='Debug' else 'v2')+'.json')),
        original_reference_authority=pin(W3/'compatibility/reference-authority-audit-v2.json'),
        retained_driver_reporting_nonzero='W3 current recorder executions completed exact comparisons; aggregate reporting path errors retained, never reclassified successful driver exits.',
        reuse_basis='No production source or UDF semantic changes; exact39 preimage lock, per-configuration compiled instruction/signature/export equality and77 reproduced current fixtures. Source newline reconciliation separately owner-authorized.'))
lines=['# Retained frozen compatibility; no fresh04A39 comparator execution','',
    '| Configuration | Corpus | Cases | Provenance | Exact retained bytes |','|---|---|---:|---|---|']
for row in allrows:lines.append('| '+row['configuration']+' | '+row['corpus']+' | '+str(row['cases'])+' | '+str(row['provenance'])+' | yes |')
(HERE/'compatibility/corpus-table-v1.md').write_text('\n'.join(lines)+'\n',encoding='utf-8',newline='\n')
print('Retained12 corpora29424 cases/config; no original/current DLL execution',flush=True)
