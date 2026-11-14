"""Correct report-only linkage of the two new observations, using their existing executed Roslyn proof."""
from common39 import *
from collections import Counter

d=load(HERE/'register/defects-v4.json');changes=[]
for row in d['issues']:
    if row['canonical_id'] not in ['ISSUE:A39-NESTED-EXTREME-ROUNDTRIP-01','ISSUE:A39-PAIRED-SOURCE-POSITION-01']:continue
    assert row['status']=='UNKNOWN_BLOCKED' and row['reproducer']['frozen']
    names=['SelectNested'] if 'EXTREME' in row['canonical_id'] else ['PairedArgumentsAligned','AlignRows']
    candidates=[]
    for location in row['source_locations']:
        for member in location.get('members',[]):
            if any(name+'(' in member['signature'] for name in names):
                candidates.append(dict(path=location['preimage']['path'],kind='EXECUTED_ROSLYN_MEMBER',
                    member=next(name for name in names if name+'(' in member['signature']),signature=member['signature'],
                    line=member['line'],header_sha256=sha(member['signature'].encode()),source_preimage=location['preimage'],
                    body_sha256=member['body_sha256'],identity=member['identity'],lexical_encoding='EXECUTED_ROSLYN_SYNTAX'))
    assert candidates
    changes.append(dict(id=row['canonical_id'],reason='New observations have executed Roslyn member and77-case fixture proof, not historical prose contexts. Generic historical-context index incorrectly overwrote their linkage status.',proof=candidates))
    row['current_declaration_candidates']=candidates;row['status']='OBSERVED_RESTRICTED';row['observed_vs_suspected']='OBSERVED_HOST_FREE_ONLY'
    row['unknown_blocked_reason']=None;row['declaration_linkage_scope']='EXISTING_EXECUTED_ROSLYN_AND_CURRENT_FIXTURE_PROOF;NO_PRODUCTION_CHANGE'
d['version']=5;d['schema']='BESHSTAT_CANONICAL_DEFECT_OBLIGATION_REGISTER_04A39_V5';d['previous']=pin(HERE/'register/defects-v4.json')
d['counts']['statuses']=dict(Counter(x['status'] for x in d['issues']))
d['counts']['issue_entries_with_current_declaration_candidates']=sum(bool(x.get('current_declaration_candidates')) for x in d['issues'] if x['canonical_id'].startswith('ISSUE:'))
save('register/defects-v5.json',d)
save('register/observation-linkage-correction-v1.json',dict(changes=changes,previous=pin(HERE/'register/defects-v4.json'),
    assertion_preserving=True,source_or_tests_changed=False,historical_obligations_changed=False))
save('register/current-v3.json',dict(canonical=pin(HERE/'register/defects-v5.json'),schema=d['schema'],version=5,
    supersedes=pin(HERE/'register/current-v2.json'),reason='Retain already executed new-observation proof; do not require historical report text for newly observed cases.'))
unknown=[dict(id=x['canonical_id'],reason=x['unknown_blocked_reason'],reports=x['report_occurrences']) for x in d['issues'] if x['status']=='UNKNOWN_BLOCKED']
save('register/unresolved-linkages-v2.json',dict(entries=unknown,
    qualification='Zero unmapped identities does not adjudicate scientific/native fault branches. Original restrictions and explicit bounded declaration candidates remain open for future review.'))
text=(HERE/'register/index-v4.md').read_text().replace('current-v2.json','current-v3.json').replace('defects-v4.json','defects-v5.json')
for row in changes:
    text=text.replace('| '+row['id']+' | UNKNOWN_BLOCKED |','| '+row['id']+' | OBSERVED_RESTRICTED |')
(HERE/'register/index-v5.md').write_text(text,encoding='utf-8',newline='\n')
print(d['counts'],flush=True)
