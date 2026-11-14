"""Independent deterministic canonical-register verifier; no writes or third-party packages."""
import collections
import csv
import hashlib
import json
from pathlib import Path
import re
import sys

def digest(b): return hashlib.sha256(b).hexdigest()
def load(p): return json.loads(Path(p).read_bytes())


def verify(root, document, check_files=True):
    root=Path(root);d=document
    issues={x['canonical_id']:x for x in d['issues']}
    assert len(issues)==len(d['issues']), 'DUPLICATE_CANONICAL_ID'
    aliases={}
    for identity,row in issues.items():
        assert row['exit_disposition_04A48'] is None, 'PREMATURE_EXIT_DISPOSITION'
        assert row['status'] not in ['FIXED_AND_VERIFIED','RETAINED_RESTRICTED','DEFERRED_OWNER_APPROVED'], 'RESERVED_04A48_STATUS'
        assert row['remaining_gate']=='P=NO;N1=NO GO;N2=NO;C2=UNMET;C3=UNAPPROVED;NOT_GOLDEN', 'GATE_RELEASE'
        for field in ['discovery_provenance','source_locations','reproducer','original_current_evidence','host_owners','risk',
                      'observed_vs_suspected','current_behavior','proposed_action','authorization','affected_tests',
                      'original_obligations','original_controls','dependency_phase','responsible_future_owner','re_review_trigger',
                      'evidence_confidence','unknown_blocked_reason','obligation_facets','report_occurrences']:
            assert field in row, 'MISSING_REQUIRED_FIELD '+field
        if row['status']=='UNKNOWN_BLOCKED':assert row['unknown_blocked_reason'], 'UNKNOWN_WITHOUT_REASON'
        for alias in row['aliases']:
            assert alias not in aliases or aliases[alias]==identity, 'ALIAS_MULTI_OWNER'
            aliases[alias]=identity
    authority=load(root/'docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json')
    expected={x['obligation_id']:x for x in authority}
    assert len(expected)==1514
    actual=[]
    for identity,row in issues.items():
        assert collections.Counter(row['original_obligations'])==collections.Counter(x['record']['obligation_id'] for x in row['obligation_facets']), 'LOST_OBLIGATION_FACET'
        for facet in row['obligation_facets']:
            old=facet['record'];key=old['obligation_id']
            assert key in expected and old==expected[key], 'CHANGED_ORIGINAL_RECORD'
            assert digest(json.dumps(old,sort_keys=True,ensure_ascii=False).encode())==facet['record_sha256'], 'CHANGED_FACET_HASH'
            actual.append((key,identity))
    assert collections.Counter(x[0] for x in actual)==collections.Counter(expected.keys()), 'MISSING_OR_DUPLICATE_OBLIGATION'
    by_kind=collections.defaultdict(list)
    for row in d['crosswalk']:
        assert row['canonical_id'] in issues, 'UNRESOLVED_CANONICAL_MAPPING'
        by_kind[row['kind']].append(row)
    assert collections.Counter((x['original_id'],x['canonical_id']) for x in by_kind['ORIGINAL_OBLIGATION'])==collections.Counter(actual), 'BAD_OBLIGATION_CROSSWALK'
    with (root/'docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv').open(encoding='utf-8-sig',newline='') as stream:
        controls={x['control_id'] for x in csv.DictReader(stream) if x['is_04A02_goldens_blocker']=='true'}
    assert len(controls)==802
    assert collections.Counter(x['original_id'] for x in by_kind['RESTRICTED_CONTROL'])==collections.Counter(controls), 'MISSING_OR_DUPLICATE_RESTRICTED_CONTROL'
    for mapping in by_kind['RESTRICTED_CONTROL']:
        assert mapping['original_id'] in issues[mapping['canonical_id']]['original_controls'], 'CONTROL_WRONG_CANONICAL_OWNER'
    occurrences=[]
    for identity,row in issues.items():
        for hit in row['report_occurrences']:
            assert hit['line']>0 and hit['excerpt_sha256'], 'BAD_REPORT_PROVENANCE'
            # A context may mention several different IDs; preserve the mapping ID independently below.
            occurrences.append((identity,hit['source']['path'],hit['line']))
    assert collections.Counter((x['canonical_id'],x['source_path'],x['source_line']) for x in by_kind['REPORT_OCCURRENCE'])==collections.Counter(occurrences), 'LOST_REPORT_OCCURRENCE'
    assert d['no_discharge'] is True and d['exit_dispositions_reserved_for_04A48'] is True
    if check_files:
        for row in d['authority_pins']+d['ingested_sources']+d['current_source_pins']:
            path=root/row['path'];b=path.read_bytes()
            assert len(b)==row['bytes'] and digest(b)==row['sha256'], 'SOURCE_AUTHORITY_CHANGED '+row['path']
        for row in issues.values():
            for candidate in row.get('current_declaration_candidates',[]):
                p=candidate['source_preimage'];b=(root/p['path']).read_bytes()
                assert len(b)==p['bytes'] and digest(b)==p['sha256'], 'CURRENT_DECLARATION_SOURCE_CHANGED'
                assert digest(candidate['signature'].encode())==candidate['header_sha256'], 'CURRENT_SIGNATURE_CHANGED'
            for location in row['source_locations']:
                p=location['preimage']
                if p.get('sha256'):
                    b=(root/p['path']).read_bytes();assert len(b)==p['bytes'] and digest(b)==p['sha256'], 'ISSUE_SOURCE_CHANGED'
    return dict(canonical_entries=len(issues),original_obligations=len(expected),restricted_controls=len(controls),
        report_occurrences=len(occurrences),statuses=dict(collections.Counter(x['status'] for x in issues.values())),
        no_discharge=True,unknown_entries=sum(x['status']=='UNKNOWN_BLOCKED' for x in issues.values()))


if __name__=='__main__':
    root=Path(sys.argv[1]) if len(sys.argv)>1 else Path(__file__).resolve().parents[2]
    document=load(root/'evidence/04A39/register/defects-v5.json')
    print(json.dumps(verify(root,document),sort_keys=True))
