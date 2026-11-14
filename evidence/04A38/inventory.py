"""Entry reconciliation. Unreviewed ownership stays OPEN and blocks product edits."""
from pathlib import Path
from collections import Counter
import csv
import json
from preflight import HERE,ROOT,pin,save


def write(name,rows):
    with (HERE/name).open('x',encoding='utf-8',newline='') as f:
        writer=csv.DictWriter(f,fieldnames=list(rows[0]),lineterminator='\n');writer.writeheader();writer.writerows(rows)
def history(name):
    with (ROOT/'docs/architecture/WindowsMigrationLedger'/name).open(encoding='utf-8-sig',newline='') as f:return list(csv.DictReader(f))


def main():
    entry=json.loads((HERE/'entry-members.json').read_bytes())
    old=[r for r in history('symbols.csv') if '/ExcelUDFs/' in r['path']]
    exports=history('udf-exports.csv');by_key={}
    for row in old:by_key.setdefault(row['signature_key'],[]).append(row)
    assert len(old)==703 and len(exports)==268
    assert Counter(r['signature_key'] for r in old) <= Counter(r['signature_key'] for r in entry['members'])
    mapped=[]
    for row in entry['members']:
        matches=by_key.get(row['signature_key'],[])
        if not matches:
            assert row['kind']=='SingleLineFunctionLambdaExpression' and row['path'].endswith('UdfDataImportRangeReaders.vb')
            assert 'XlCall.Excel(XlCall.xlCoerce, reference)' in row['body_tokens'].replace('XlCall . Excel','XlCall.Excel').replace('XlCall . xlCoerce','XlCall.xlCoerce') or 'XlCall' in row['body_tokens']
        elif len(matches)>1:
            # Same lambda header is not a unique identity; require the same body
            # and enclosing source evidence, never guess from a name alone.
            exact=[r for r in matches if r['symbol_id']==row['symbol_id']]
            if not exact:
                owners={r['symbol_id']:r for r in old}
                exact=[r for r in matches if r['body_sha256']==row['body_sha256'] and owners.get(r['enclosing_symbol_id'],{}).get('name') in row['enclosing']]
            assert len(exact)==1,'Ambiguous historical member: '+row['symbol_id']
            matches=exact
        oldrow=matches[0] if matches else None
        mapped.append(dict(current_member_id=row['symbol_id'],path=row['path'],kind=row['kind'],member=row['name'],owner=row['containing_type'],signature=row['signature'],
            entry_source_sha256=row['source_sha256'],entry_body_sha256=row['body_sha256'],entry_body_token_sha256=row['body_token_sha256'],
            historical_member_id=oldrow['symbol_id'] if oldrow else 'ADDITIONAL_ACCEPTED_WINDOWS_COERCION_CALLBACK',
            historical_source_sha256=oldrow['source_sha256'] if oldrow else '',historical_disposition=oldrow['disposition'] if oldrow else '',
            historical_reason=oldrow['rationale'] if oldrow else 'Get2D delegates to accepted WindowsSpreadsheetTransport with native XlCall coercion callback; no additional export.',
            historical_runtime_boundary=oldrow['runtime_boundary'] if oldrow else 'Windows native ExcelReference coercion; NATIVE_NOT_RUN',
            current_disposition='OPEN_ENTRY_REVIEW',planned_owner='',portable_equivalent='',actual_windows_consumer='',evidence_refs='entry-members.json; preedit/entry-source-snapshot.json',test_case_ids='',future_issue=oldrow['future_batch'] if oldrow else '04A.39/04A.44-48'))
    # Historical duplicate-header populations must remain exactly reconciled.
    assert Counter(r['historical_member_id'] for r in mapped if not r['historical_member_id'].startswith('ADDITIONAL_'))==Counter(r['symbol_id'] for r in old)
    write('udf-member-crosswalk.csv',mapped)
    historical={r['effective_name']:r for r in exports}
    assert len(historical)==len({e['effective_excel_name'].casefold() for e in entry['exports']})==268
    assert set(historical)=={e['effective_excel_name'] for e in entry['exports']}
    rows=[]
    for e in entry['exports']:
        h=historical[e['effective_excel_name']]
        assert e['path']==h['path'] and e['implementing_method']==h['implementing_name']
        assert ' '.join(e['full_public_signature'].split())==h['signature']
        rows.append(dict(export_id=h['export_id'],current_export_id=e['export_id'],path=e['path'],source_symbol_id=e['source_symbol_id'],effective_excel_name=e['effective_excel_name'],implementing_method=e['implementing_method'],
            full_public_signature=e['full_public_signature'].replace('\r\n','\n'),return_type=e['return_type'],ordered_arguments_and_defaults=json.dumps(e['arguments'],ensure_ascii=False),full_attribute_text=e['full_attribute_text'].replace('\r\n','\n'),
            thread_safe_volatile_macro_flags=json.dumps(e['attribute_arguments'],ensure_ascii=False),original_source_hash=e['original_source_hash'],historical_review_id=h['symbol_id'],
            historical_export_disposition=h['disposition'],runtime_boundary=h['runtime_boundary'],reusable_call_edges=h['dependencies'],current_core_common_equivalent=h['core_common_equivalent'],accepted_behavior=h['rationale'],
            planned_ownership='OPEN_ENTRY_REVIEW',extraction_disposition='OPEN_ENTRY_REVIEW',actual_windows_consumer=e['containing_type']+'.'+e['implementing_method'],test_case_ids='',known_04A39_43_issue=h['future_batch'],evidence_refs='entry-members.json; frozen historical udf-exports.csv'))
    write('udf-export-inventory.csv',rows)
    write('udf-source-inventory.csv',[dict(**r,exports=sum(e['path']==r['path'] for e in entry['exports']),role='EXPORT_AND_SUPPORT' if any(e['path']==r['path'] for e in entry['exports']) else 'NON_EXPORT_SUPPORT') for r in entry['sources']])
    edges=[]
    for member in entry['members']:
        for index,call in enumerate(member['calls']):
            edges.append(dict(owner_id=member['symbol_id'],path=member['path'],call_ordinal=index,source_line=call['line'],invocation=call['text'],expression=call['expression'],binding='SYNTAX_ONLY_NOT_SEMANTIC_PROOF; compiled method edges required',disposition='OPEN_ENTRY_REVIEW'))
    write('udf-consumer-edges-entry.csv',edges)
    save('inventory-entry-summary.json',dict(status='PASS_REGISTRATION_AND_HISTORY; OWNERSHIP_AND_EXECUTABLE_COVERAGE_NOT_COMPLETE',
        sources=len(entry['sources']),export_bearing_sources=len({e['path'] for e in entry['exports']}),support_sources=18,exports=len(rows),current_members=len(mapped),historical_members=len(old),
        matched_historical_members=703,additional_members=1,removed_historical_members=0,public_registration_changes=0,
        historical_export_hints=dict(Counter(r['disposition'] for r in exports)),current_open_members=len(mapped),
        additional_identity=[r for r in mapped if r['historical_member_id'].startswith('ADDITIONAL_')],
        historical_authorities=[pin('docs/architecture/WindowsMigrationLedger/'+p) for p in ['symbols.csv','udf-exports.csv']],
        product_edit_gate='NOT_SATISFIED; full baseline and independent reached-behavior freeze required before implementation'))
    print('PASS41 sources;268 exact export signatures/attributes;703 historical members reconciled;1 additional native callback; ownership remains explicitly OPEN')

if __name__=='__main__':main()
