from common import *
historical=json.loads((HERE/'historical-ownership.json').read_bytes())['members'];current=json.loads((HERE/'current-ownership.json').read_bytes());schedule=json.loads((W1/'w2-implementation-schedule.json').read_bytes())['sources']
matrix={r['canonical_original_id']:r for r in json.loads((W1/'member-behavior-evidence-target-matrix-v3.json').read_bytes())['members']}
sources=[]
for row in schedule:
 pending=[dict(original_id=r['canonical_original_id'],name=r['name'],current_ids=r['current_ids'],original_start_line=matrix[r['canonical_original_id']]['original_start_line'],original_end_line=matrix[r['canonical_original_id']]['original_end_line']) for r in historical if r['path']==row['path'] and not r['proof']]
 sources.append(dict(path=row['path'],wave=row['wave'],priority=row['priority'],current_source=pin(ROOT/row['path']),OPEN=len(pending),members=pending,responsibilities=row['responsibilities']))
extras=[r for r in historical if r['path'] not in {r['path'] for r in schedule}]
priority=[r for r in historical if r['name'] in ['TryParseLeveneCenter','TryParseSymmetryMethod','PRTRNG','PRTRNG_TAIL','F_PDF'] and not r['proof']]
save('remaining-work-queue.json',dict(status='SOURCE_BACKED_REMAINING_W2_AFTER_G_NOT_A_NEW_CENSUS_OR_AUTHORITY_REQUEST',sources=sources,extra_historical_callback_identities=extras,first_concrete_review=priority,common_policy_dependency='BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb::EqualText;current null-versus-ignorable control nonpass',support_OPEN=sum(not r['already_in_primary_population'] for r in current['support']),statements_inline='All15961 current fragments have explicit parent/status in current-ownership.json; original W1 branch/statement matrices preserved. No unreviewed fragment omitted or falsely closed.',owner_decision_needed=False,new_original_queries=0))
with (HERE/'publication-closeout-addendum-v4.md').open('x',encoding='utf-8') as f:f.write('''# Publication closeout addendum — NOT_READY_04A38

The current partial packet uses publication-manifest-v4.json and verify_publication_v2.py. Pass the manifest path explicitly:

`<PYTHON> -B evidence/04A38/completion/w2/continuation-v1/verify_publication_v2.py evidence/04A38/completion/w2/continuation-v1/publication-manifest-v4.json`

The isolated-copy command is check_public_copy_v2.py with the same argument. The external export receipt contains observed archive bytes/SHA-256/entry count/CRC and final live Git checks; no ZIP identity is invented inside the ZIP that is hashed.

Manifest v2 was rejected before ZIP creation because the original privacy-adversary source contains a fictional EXAMPLE_PRIVATE home-path example. Its bytes and failed reader remain unchanged. New reader v2 permits only that exact reviewed source hash/fictional marker in that one file. Generated leaks in other payloads and any modification of the reviewed source still fail. No actual owner/home path is whitelisted and no statistical/source/trace bytes were redacted.

The first revised publication test used a reassigned module constant but left Python's already-bound default manifest argument at v1. Its mutation-context failures are preserved and are not accepted coverage. Corrected test_publication_v3.py passes the selected v3 manifest explicitly; its15 conditions include generated privacy leakage and mutation of the reviewed synthetic source. Later v4 adds these actual receipts without altering the v3 or older manifests.

All other ownership/implementation limitations in owner-review and NEXT_WAVE_HANDOVER remain:60 historical supported/644 OPEN,303 support declarations unresolved, W2 IN_PROGRESS and W3 NOT_STARTED. The latest P1 body adverse suite has15 actual conditions, P0 has9, and current publication controls have15. No new original reference, native/scientific run, Git publication, owner acceptance or04A.39 work occurred.
''')
print('Remaining exact IDs/families and publication scope recorded')
