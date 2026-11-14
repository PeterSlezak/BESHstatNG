"""Canonical additive issue/obligation crosswalk. Original IDs and records are immutable facets."""
from common39 import *
from collections import defaultdict, Counter
import re

source_lock()
obligation_path='docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'
restriction_path='docs/architecture/04A02/native-restriction-crosswalk-v1.json'
obligations=load(ROOT/obligation_path);restriction=load(ROOT/restriction_path)
assert len(obligations)==len({r['obligation_id'] for r in obligations})==1514
restricted=set(i for form in restriction['forms'].values() for i in form['blocked_control_ids'])
assert len(restricted)==802
syntax=load(HERE/'map/current-source-syntax-v1.json')
members=defaultdict(list)
for member in syntax['members']:members[member['path']].append(member)
pins={};issues={};mapping=[]

def current_source(path):
    path=path.replace('\\','/')
    if path in pins:return pins[path]
    actual=ROOT/path
    if not actual.is_file():return dict(path=path,status='UNKNOWN_BLOCKED',reason='Historical source no longer at this exact path; original record retained.')
    pins[path]=pin(actual)
    return pins[path]

def base(identity, aliases, provenance, description, phase, owners, state):
    return dict(canonical_id=identity,aliases=sorted(set(aliases)),status=state,discovery_provenance=provenance,
        description=description,source_locations=[],reproducer=dict(status='NOT_RUN_NATIVE_OR_SCIENTIFIC',details='See immutable original named fixtures/observations; no fabricated scientific oracle.'),
        original_current_evidence=[],host_owners=owners,risk=dict(scientific='REVIEW_REQUIRED',numerical='REVIEW_REQUIRED',
            provenance='SOURCE_DEPENDENT',native_COM='RESTRICTED',privacy='INHERITED_RESTRICTIONS'),
        observed_vs_suspected=state,current_behavior='Historical/current source evidence retained; no behavior correction activated.',
        proposed_action='Named future owner investigates each distinct immutable facet; separate scientific/native authorization before observable correction.',
        authorization='INVESTIGATION_AND_BEHAVIOR_PRESERVING_RECONCILIATION_ONLY',affected_tests=[],original_obligations=[],original_controls=[],
        dependency_phase=phase,remaining_gate='P=NO;N1=NO GO;N2=NO;C2=UNMET;C3=UNAPPROVED;NOT_GOLDEN',
        responsible_future_owner=owners,re_review_trigger='Changed source, dependent phase, new native/scientific evidence or exact owner decision.',
        evidence_confidence='SOURCE_BOUND;NOT_NATIVE_OR_SCIENTIFIC_RELEASE',unknown_blocked_reason=None,
        exit_disposition_04A48=None,obligation_facets=[],report_occurrences=[])

for row in obligations:
    controls=row['linked_control_ids'];assert len(controls)==1
    control=controls[0];identity='CONTROL:'+control
    if identity not in issues:
        issues[identity]=base(identity,[control],[pin(obligation_path)],'Source-control risk bundle; distinct original contracts remain separate immutable facets.',
            sorted(set(row['earliest_approved_batches']+row['dependent_batches'])),row['owner_roles'],'UNREVIEWED_ORIGINAL_OBLIGATION')
    issue=issues[identity]
    issue['aliases']=sorted(set(issue['aliases']+[row['obligation_id'],row['original_id']]))
    issue['original_obligations'].append(row['obligation_id']);issue['original_controls']=[control]
    issue['obligation_facets'].append(dict(record_sha256=sha(json.dumps(row,sort_keys=True,ensure_ascii=False).encode()),record=row))
    source=row['source_evidence'];path=source.get('codebehind_path') or source.get('declaration_path')
    if path and not any(x['preimage']['path']==path for x in issue['source_locations']):
        issue['source_locations'].append(dict(preimage=current_source(path),historical_source_evidence=source,
            member_signature='Exact historical Roslyn member_body_pins/symbols in immutable facet; not guessed from a control name.',
            line=source.get('declaration_line'),branch=row['unsupported_contract']))
    mapping.append(dict(kind='ORIGINAL_OBLIGATION',original_id=row['obligation_id'],canonical_id=identity,
        original_record_sha256=issue['obligation_facets'][-1]['record_sha256']))
for control in sorted(restricted):
    identity='CONTROL:'+control
    if identity not in issues:
        issue=base(identity,[control],[pin(restriction_path)],'Restricted original live-control identity absent from1514 obligation linked-control set.',
            ['04A.45–48'],['Windows native-control maintainer'],'UNKNOWN_BLOCKED')
        issue['unknown_blocked_reason']='No exact obligation linkage in immutable1514 authority; do not infer or omit.'
        issue['original_controls']=[control];issues[identity]=issue
    issues[identity]['native_control_restriction']=True
    mapping.append(dict(kind='RESTRICTED_CONTROL',original_id=control,canonical_id=identity))

# Bounded authoritative dossiers only. Never recursively enumerate TestResults/private runs.
tracked=git('ls-files','docs/architecture','evidence','planning').decode().splitlines()
selected=[];omissions=[];occurrences=defaultdict(list)
id_pattern=re.compile(r'(?<![A-Za-z0-9])(?:C\d{2}-[A-Z][A-Z0-9_-]*-\d{2,3}|UDF38-D\d{2}-[A-Z0-9-]+)(?![A-Za-z0-9])')
for name in tracked:
    p=Path(name)
    if p.suffix.lower() not in ['.md','.json','.csv']:continue
    if name.startswith('evidence/04A38/'):
        if name not in ['evidence/04A38/udf-deferred-issues.json','evidence/04A38/completion/w3/final/unresolved-exceptions-v2.json']:continue
    elif name.startswith('evidence/'):
        if not re.match(r'evidence/04A(?:0[89]|[12]\d|3[0-7])/',name):continue
        if not re.search(r'issue|defer|candidate|restriction|crosswalk|owner|closeout|readiness|handover|obligation',p.name,re.I):continue
    elif name.startswith('docs/architecture/WindowsMigrationLedger/'):
        if p.suffix.lower()!='.md' and not re.search(r'control.*triage|risk.*explanation|deferred|disposition',p.name,re.I):continue
    elif name.startswith('planning/'):
        if p.name not in ['BESHStatNG_CrossPlatform_MasterPlan2.12.md','BESHStatNG_CrossPlatform_MasterPlan2.14.md']:continue
    elif not name.startswith('docs/architecture/'):continue
    if name==obligation_path:continue
    size=(ROOT/name).stat().st_size
    if size>16*1024*1024:
        omissions.append(dict(source=pin(name),status='UNKNOWN_BLOCKED',reason='Large historical result payload not parsed; original and original-ID authorities preserved.'))
        continue
    source_pin=pin(name);selected.append(source_pin)
    text=(ROOT/name).read_text(encoding='utf-8-sig',errors='strict')
    for number,line in enumerate(text.splitlines(),1):
        for identity in sorted(set(id_pattern.findall(line))):
            occurrences[identity].append(dict(source=source_pin,line=number,excerpt=line.strip()[:1800],excerpt_sha256=sha(line.encode())))
deferred={r['id']:r for r in load(ROOT/'evidence/04A38/udf-deferred-issues.json')['issues']}
for alias,found in sorted(occurrences.items()):
    identity='ISSUE:'+alias
    row=base(identity,[alias],[],found[0]['excerpt'],['04A.40–48'],['Source-family maintainer; project owner'],'SUSPECTED_REQUIRES_REVIEW')
    row['report_occurrences']=found
    row['discovery_provenance']=[r['source'] for r in found]
    paths=sorted(set(m for hit in found for m in re.findall(r'(?:BESHstatNG|BESHStat\.Core|BESHStat\.Excel\.Common)/[A-Za-z0-9_./-]+\.vb',hit['excerpt'])))
    for path in paths:row['source_locations'].append(dict(preimage=current_source(path),member_signature='See source-bound report occurrence; unresolved exact member remains explicit.',line=None,branch=None))
    if alias in deferred:
        detail=deferred[alias];row['description']=detail['observation'];row['dependency_phase']=[detail['batch']];row['current_behavior']=detail['observation']
        for chunk in detail['source'].split(';'):
            path=chunk.strip().split(':')[0]
            if path.endswith('.vb'):
                candidates=members.get(path,[])
                row['source_locations'].append(dict(preimage=current_source(path),members=[dict(signature=m['signature'],identity=m['symbol_id'],body_sha256=m['body_sha256'],line=m['start_line']) for m in candidates],branch=detail['source']))
    if not row['source_locations']:
        row['unknown_blocked_reason']='Report ID is authenticated; exact current member linkage needs future source-family review. No inferred native/scientific resolution.'
        row['status']='UNKNOWN_BLOCKED'
    issues[identity]=row
    for index,hit in enumerate(found):mapping.append(dict(kind='REPORT_OCCURRENCE',original_id=alias,source_path=hit['source']['path'],source_line=hit['line'],occurrence=index,canonical_id=identity))

new=[('A39-NESTED-NUMERIC-TEXT-01','UDF38-D39-TEXT-PRECISION','WindowsPairedInputAdapter.SelectNested',
      'Typed captured Double bits differ from actual legacy Core matrix after column-wise numeric-text conversion; signed zero may be lost.',
      'BESHstatNG/src/DM/WindowsPairedInputAdapter.vb','NestedCaptureAndCoreBitsRemainSeparatelyObservable'),
     ('A39-PAIRED-SOURCE-POSITION-01',None,'WindowsSpreadsheetTransport.PairedArgumentsAligned / PairedBlockInputPolicy.AlignRows',
      'Raw arrays promise positional pairs; independently acquired physical ranges require verified same identity/row bounds or explicit physical/subject-key join.',
      'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb','PhysicalIntersectionAndOrdinalPairsAreSeparateContracts'),
     ('A39-NESTED-EXTREME-ROUNDTRIP-01',None,'WindowsPairedInputAdapter.SelectNested',
      'Double.MaxValue can overflow after Framework default string formatting followed by Convert.ToDouble; retained current behavior, no hardening activated.',
      'BESHstatNG/src/DM/WindowsPairedInputAdapter.vb','NestedExtremeFiniteLegacyConversionExceptionIsRetained')]
for alias,merge_alias,member,description,path,test in new:
    identity='ISSUE:'+merge_alias if merge_alias else 'ISSUE:'+alias
    if identity in issues:
        row=issues[identity];row['aliases']=sorted(set(row['aliases']+[alias]));row['description']+=' '+description
    else:
        row=base(identity,[alias],[],description,['04A.39 investigation','04A.40–41 scientific review','04A.45–48 native'],['Common input policy / Windows acquisition / scientific reviewer'],'OBSERVED_RESTRICTED');issues[identity]=row
    row['status']='OBSERVED_RESTRICTED';row['observed_vs_suspected']='OBSERVED_HOST_FREE_ONLY'
    row['source_locations'].append(dict(preimage=current_source(path),member_signature=member,members=[dict(signature=m['signature'],identity=m['symbol_id'],line=m['start_line'],body_sha256=m['body_sha256']) for m in members[path]]))
    row['reproducer']=dict(driver=pin(HERE/'fixtures/Observe39v2.cs'),frozen=pin(HERE/'fixtures/frozen-input-manifest-v2.json'),reference='04A39_CURRENT_PRE_CHANGE;HISTORICAL_REFERENCE_UNAVAILABLE;NOT_GOLDEN')
    row['affected_tests'].append(test);row['original_current_evidence'].append(pin(HERE/'fixtures/before-v2.json'))
    row['unknown_blocked_reason']=None
    mapping.append(dict(kind='NEW_04A39_OBSERVATION',original_id=alias,canonical_id=identity))

for omission in omissions:
    identity='UNKNOWN_BLOCKED:'+sha(omission['source']['path'].encode())[:20]
    row=base(identity,[],[omission['source']],omission['reason'],['04A.48'],['Historical evidence custodian'],'UNKNOWN_BLOCKED')
    row['unknown_blocked_reason']=omission['reason'];issues[identity]=row
    mapping.append(dict(kind='BOUNDED_INGESTION_LIMIT',original_id=omission['source']['path'],canonical_id=identity))

register=dict(schema='BESHSTAT_CANONICAL_DEFECT_OBLIGATION_REGISTER_04A39_V1',version=1,authority='Single current canonical register; immutable historical ledgers remain authorities for their original facts.',
    merged_main=MAIN,gates=GATES,issues=[issues[k] for k in sorted(issues)],crosswalk=mapping,
    authority_pins=[pin(obligation_path),pin(restriction_path)],ingested_sources=selected,current_source_pins=list(pins.values()),
    ingestion_limits=omissions,counts=dict(original_obligations=1514,restricted_controls=802,distinct_original_linked_controls=927,
        canonical_entries=len(issues),report_ids=len(occurrences),report_occurrences=sum(map(len,occurrences.values())),new_observations=len(new)),
    deduplication='Exact repeated report IDs share one canonical issue; control bundles retain each original obligation/unsupported contract as a separately hashed immutable facet. Similar text or phase alone never merges different IDs.',
    no_discharge=True,exit_dispositions_reserved_for_04A48=True,original_populations_overlap_not_additive=True)
save('register/defects-v1.json',register)
save('register/ingestion-scope-v1.json',dict(sources=selected,limits=omissions,pattern=id_pattern.pattern,
    control_and_obligation_scope='All1514 exact original IDs and802 restricted controls from immutable authorities.',
    dossiers='Bounded tracked authority/report files under docs/architecture,04A08–37 and selected04A38. Original raw multiGB/failed-attempt logs not parsed; references retained.',
    unknowns='Member links unresolved in authenticated historical reports remain UNKNOWN_BLOCKED; never asserted closed.'))
index=['# Canonical04A.39 defect and obligation index','',
       'Authority: [defects-v1.json](defects-v1.json). Original controls/obligations overlap; counts are not additive. '+
       'No obligation is discharged and no04A.48 exit disposition is assigned. Distinct risk contracts remain immutable facets.','',
       '| Canonical identity | Status | Original obligations | Report occurrences |','|---|---|---:|---:|']
for identity,row in sorted(issues.items()):index.append(f'| {identity} | {row["status"]} | {len(row["original_obligations"])} | {len(row["report_occurrences"])} |')
(HERE/'register/index-v1.md').write_text('\n'.join(index)+'\n',encoding='utf-8',newline='\n')
print(json.dumps(register['counts']),flush=True)
