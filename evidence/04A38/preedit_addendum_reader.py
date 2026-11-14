"""Independent reader for genuine before-production additional coverage.

No Route A exception. This extends the preserved first freeze while every
production source still has entry bytes; it never refreshes expected values.
"""
from collections import Counter,defaultdict
import csv
import hashlib
import json
from pathlib import Path
import preedit_reader as base

FIRST_RECEIPT='9f9a55337311bf37e010dcc078cb2ff8a8384122c1b105c4593732e90e13d165'
EXPECTED={
    'public-options-v1':(2508,'2d3705fffe38f46081e18c6218977b6402d38ef9ee0a882fd5f28e24a6c47b52'),
    'public-primitives-v1':(226,'0bc33bbf358d1abf79b93ffe6afbfd54f93c2d07d9c034f95686d83046c22b3b'),
    'model-inputs-v2':(882,'3107a6d7b8116beb62f37665ed5d59f6df0cf3c1d2743cf5c28ddf59eaed0e58'),
    'math-edges-v1':(618,'faba4ad9a633d88625388b627354d5cea916fcf60ba05e1e7ac6e06c8234b6d6'),
}


def inspect(root):
    here=root/'evidence/04A38'
    receipt=(here/'preedit/frozen-v2/receipt.json').read_bytes()
    base.require(base.digest(receipt)==FIRST_RECEIPT,'First valid freeze changed')
    first=json.loads(receipt)
    base.require(first['product_edits_at_seal'] is False and first['late_capture_exception'] is False,'Wrong first-freeze chronology')
    for row in first['files']:base.check_pin(root,row)
    census=json.loads((here/'entry-members.json').read_bytes())
    by_export={e['effective_excel_name']:e for e in census['exports']}
    callable_by_name=defaultdict(set)
    for member in census['members']:
        if member['kind'] in base.CALLABLE:callable_by_name[(member['containing_type'],member['name'])].add(member['symbol_id'])
    summary={}
    for version,(count,anchor) in EXPECTED.items():
        folder=here/'preedit/attempts'/version
        specification=json.loads((folder/'specification.json').read_bytes())
        raw=base.check_pin(root,specification['cases'])
        cases=[json.loads(s) for s in raw.decode('utf-8').splitlines()]
        base.require(len(cases)==count==len({c['case_id'] for c in cases}),version+' case cardinality/identity lost')
        base.require(specification['source_commit']==base.BASE and 'BEFORE_FIRST_04A38_PRODUCT_EDIT' in specification['chronology'],version+' has wrong provenance')
        base.check_pin(root,specification['recorder'])
        for c in cases:
            payload=dict(c);identity=payload.pop('case_id')
            base.require(identity==payload['target']+'.'+base.digest(json.dumps(payload,sort_keys=True,separators=(',',':')).encode())[:24],'Descriptor/case identity changed')
            base.require(c['culture'] in base.CULTURES and c['expected_stage'],'Missing culture/phase')
        configs={}
        for config in ['Debug','Release']:
            paths=[folder/('capture-'+config+'-'+name+'.tsv') for name in ['original-v1','repeat-v2','trace-v1']]
            contents=[p.read_bytes() for p in paths]
            base.require(base.digest(contents[0])==anchor,version+' immutable output changed')
            base.require(contents[0]==contents[1]==contents[2],version+' repeat or coverage instrumentation changes outcomes')
            rows=base.read_rows(paths[0])
            traces=[json.loads(s) for s in Path(str(paths[2])+'.reachability.jsonl').read_text(encoding='utf-8').splitlines()]
            base.require([r['case_id'] for r in rows]==[r['case_id'] for r in traces]==[c['case_id'] for c in cases],'Missing/reordered/duplicate cases')
            for c,r,t in zip(cases,rows,traces):
                base.require('OPAQUE:' not in r['result']+r['byref_arguments'],'Unobserved typed payload')
                base.require(r['phase'] not in ['BUILD_INPUT','SETUP_FIT_SEED'],'Fixture failed before source dispatch')
                if c['target_kind']=='export':ids={by_export[c['target']]['source_symbol_id']}
                else:ids=callable_by_name[(c['type'],c['target'])]
                base.require(bool(ids.intersection(t['reached'])),'Target source body was never reached')
                if version=='model-inputs-v2':
                    base.require(r['outcome'] in ['TRANSACTION_FIT_ACCEPTED','TRANSACTION_FIT_REJECTED'],'Unexpected fit/query fixture exception')
                    if r['outcome']=='TRANSACTION_FIT_ACCEPTED':
                        for query in c['query_exports']:
                            base.require(by_export[query['name']]['source_symbol_id'] in t['reached'],'An accepted fit never reached a requested query')
            outcomes=dict(Counter(r['outcome'] for r in rows))
            for name in ['original-v1','repeat-v2','trace-v1']:
                execution=json.loads((folder/('execution-'+config+'-'+name+'.json')).read_bytes())
                base.require(execution['rows']==count and execution['outcomes']==outcomes and execution['native_attempts']==0,'Execution accounting/native violation')
                base.require(execution['scientific']=='NOT_GOLDEN' and 'NOT_RUN' in execution['native'],'A denied gate was promoted')
            configs[config]=dict(rows=count,sha256=anchor,bytes=len(contents[0]),outcomes=outcomes,repeat_exact=True,trace_exact=True,native_attempts=0)
        summary[version]=configs
    # Shipping corroboration counts are a subset, never added to corpus totals.
    actual={}
    for config in ['Debug','Release']:
        folder=here/'preedit/attempts'/('shipping-helpers-'+config+'-v2')
        rows=base.read_rows(folder/'original.tsv')
        originals={r['case_id']:r for r in base.read_rows(here/'preedit/frozen-v2'/('original-'+config+'.tsv'))}
        base.require(len(rows)==10752 and all(r==originals[r['case_id']] for r in rows),'Actual shipping helper corroboration failed')
        execution=json.loads((folder/'execution.json').read_bytes())
        base.require(execution['native_attempts'] is None and 'PRIVATE_HELPERS_ONLY' in execution['native_guard'],'Shipping helper run falsely claims a native test-double counter')
        actual[config]=dict(exact_rows=10752,sha256=base.digest((folder/'original.tsv').read_bytes()),native='NOT_RUN; source/input whitelist; no native execution claim')
    return dict(status='PRE_EDIT_ADDITIONAL_COVERAGE_CONTROLS_PASS; NOT_IMPLEMENTATION_READY',source_commit=base.BASE,
        first_valid_freeze_sha256=FIRST_RECEIPT,first_rows_per_configuration=24732,
        additional_rows_per_configuration=sum(c for c,_ in EXPECTED.values()),total_rows_per_configuration=24732+sum(c for c,_ in EXPECTED.values()),
        corpora=summary,actual_shipping_private_helper_subset=actual,
        chronology='ALL_CAPTURES_BEFORE_FIRST_04A38_PRODUCT_EDIT; no late-capture exception',
        native='NATIVE_EXCEL_NOT_RUN',scientific='NOT_GOLDEN',production_ready=False)


if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[2]);parser.add_argument('--output',type=Path);args=parser.parse_args()
    try:
        result=inspect(args.root)
        if args.output:
            with args.output.open('x',encoding='utf-8',newline='\n') as stream:json.dump(result,stream,indent=2);stream.write('\n')
        print(json.dumps(dict(status=result['status'],rows_per_configuration=result['total_rows_per_configuration'])))
    except (base.InvalidReference,OSError,KeyError,ValueError) as error:
        print(json.dumps(dict(status='NOT_READY_04A38',reference_error=str(error))));raise SystemExit(1)
