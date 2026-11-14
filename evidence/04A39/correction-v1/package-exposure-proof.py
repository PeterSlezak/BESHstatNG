from common import *
inventory=load(PRIVATE/'previous-package-inventories.json')
cache=load(PRIVATE/'focused-cache.json')['rows']
v6={r['path']:r for r in inventory['v6']};codex={r['path']:r for r in inventory['codex']}
hits=[]
for row in cache:
    if row['findings'] and row['path'] in v6:
        assert v6[row['path']]['sha256']==row['sha256']
        assert row['path'] not in codex
        hits.append(dict(source_id='SOURCE-'+sha(row['path'].encode())[:16],sha256=row['sha256'],scanner_occurrences=len(row['findings'])))
save('publication/v6-exposure-proof.json',dict(exact_raw_files_in_v6=len(hits),known_scanner_occurrences=sum(r['scanner_occurrences'] for r in hits),files=hits,
    codex_exact_raw_files=0,scope='Exact byte/hash crosswalk to actual4479-payload v6 ZIP and fresh focused scanner findings; not a new exhaustive v6 content privacy scan.',
    disposition='V6_NOT_APPROVED_FOR_REPUBLICATION;KEEP_AUTHORITATIVE_ARCHIVE_UNCHANGED;REPLACE_WORKFLOW_WITH_FAIL_CLOSED_STAGE_GATE'))
print('Actual v6 contains',len(hits),'known raw files;',sum(r['scanner_occurrences'] for r in hits),'source-bound finding occurrences; Codex0 such raw files',flush=True)
