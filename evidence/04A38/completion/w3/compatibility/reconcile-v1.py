"""Recover completed comparison receipts after report-only path-binding error.

No recorder compilation/execution is repeated. All original/current bytes and
case occurrences are independently read back against sealed provenance.
"""
from common import *
import csv
from collections import Counter

config=sys.argv[1]
lock()
old=load(ACCEPTED/'compatibility-replacement-v1.json')
expected={r['corpus']:r for r in old['results']}
rows=[];pre=late=0
for corpus,prior in expected.items():
    folder=HERE/'compatibility/captures'/('w3-'+config+'-v1')/config/corpus
    r=load(folder/'comparison.json')
    assert r['configuration']==config and r['corpus']==corpus and r['cases']==prior['cases']
    for k in ['original','inputs','recorder','current']:
        assert pin(ROOT/r[k]['path'])==r[k],k+' BYTES_CHANGED'
    assert r['command']['exit_code']==0 and r['original_queries']==0
    # Original references must be the existing configuration-specific files;
    # A-G inputs are also configuration-specific.
    for k in ['original','inputs','recorder']:
        bound=dict(prior[k])
        if config=='Release':
            bound['path']=bound['path'].replace('Debug','Release')
            # Hash/size of Release are authenticated separately below, never
            # assumed from Debug bytes.
        assert r[k]['path']==bound['path'], 'ORIGINAL_AUTHORITY_REPLACED '+corpus
    original=(ROOT/r['original']['path']).read_bytes()
    current=(ROOT/r['current']['path']).read_bytes()
    assert original==current and r['exact'],'EXACT_BYTE_MISMATCH '+corpus
    parsed=list(csv.reader(current.decode('utf-8-sig').splitlines(),delimiter='\t'))
    header=parsed[0];body=parsed[1:]
    # Existing recorder rows include one result for each complete case.
    # Preserve full ordered row bytes; inspect identity columns from header.
    r.update(provenance='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if corpus in list('ABCDEFG') else 'PRE_EDIT_FROZEN',
             header=header,physical_rows=len(body),ordered_row_sha256=sha(current),
             exact_comparison='WHOLE_ORDERED_BYTES;INCLUDES_TYPED_RESULT_AND_ERROR_ARRAY_ENCODING',
             comparison_receipt=pin(folder/'comparison.json'))
    print(config,corpus,'cases',r['cases'],'rows',len(body),'header',header,flush=True)
    assert len(body)==r['cases'],'MISSING_OR_DUPLICATE_RESULT_ROWS'
    if r['provenance']=='PRE_EDIT_FROZEN':pre+=r['cases']
    else:late+=r['cases']
    rows.append(r)
assert len(rows)==12 and (pre,late)==(28966,458)
save('compatibility/'+config+'-comparisons-v1.json',dict(status='FRESH_CURRENT_ONLY_EXACT_COMPARISONS_READ_BACK',
    configuration=config,results=rows,cases=pre+late,PRE_EDIT_FROZEN=pre,LATE_ARCHIVED_ENTRY_SUPPLEMENT=late,
    all_exact=True,original_queries=0,source_freeze=pin(ACCEPTED/'production-test-freeze-v1.json'),
    stopped_report=pin(HERE/('execution/'+config.lower()+'-compatibility-v1/receipt.json')),
    report_defect='All12 comparisons completed and saved before final aggregate attempted to pin a W3-local freeze path. This additive reader binds the actual accepted W2 freeze. No completed identity repeated.',
    original_Release_hash_authority='Actual pre-existing configuration-specific original bytes protected by historical reference/preservation readers; no hash inferred from Debug'))
lock()
