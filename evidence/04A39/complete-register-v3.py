"""Enrich exact historical issue records and source contexts; do not infer native/scientific closure."""
from common39 import *
from collections import defaultdict, Counter
import re

source_lock()
register=load(HERE/'register/defects-v2.json')
issues={x['canonical_id']:x for x in register['issues']}
allpaths=git('ls-files').decode().splitlines()
basenames=defaultdict(list)
for path in allpaths:
    if path.endswith('.vb') and not path.startswith(('evidence/','TestResults/')):basenames[Path(path).name.casefold()].append(path)
sources=register['ingested_sources'];new_sources=[]
syntax=load(HERE/'map/current-source-syntax-v1.json');methods=defaultdict(list)
for m in syntax['members']:methods[m['path']].append(m)
pattern=re.compile(r'(?<![A-Za-z0-9])(?:G29-[A-Z0-9-]+|C\d{2}-[A-Z][A-Z0-9_-]*-\d{2,3}|UDF38-D\d{2}-[A-Z0-9-]+)(?![A-Za-z0-9])')

def ensure(alias,source):
    identity='ISSUE:'+alias
    if identity not in issues:
        # Retain the same required schema as existing report issues; no inherited original obligation is copied.
        sample=next(v for v in issues.values() if v['canonical_id'].startswith('ISSUE:'))
        row={k:v for k,v in sample.items() if k not in ['canonical_id','aliases']}
        row=json.loads(json.dumps(row))
        for key in ['source_locations','original_current_evidence','affected_tests','original_obligations','original_controls','obligation_facets','report_occurrences','discovery_provenance']:row[key]=[]
        row.update(canonical_id=identity,aliases=[alias],status='UNKNOWN_BLOCKED',observed_vs_suspected='HISTORICAL_REPORT_NOT_REEXECUTED',
            description='Authenticated historical candidate; detailed immutable record/context retained below.',
            current_behavior='No production correction under04A.39.',dependency_phase=['04A.40–48'],
            unknown_blocked_reason='Exact current source/member linkage awaits source-family review.',
            reproducer=dict(status='HISTORICAL_REFERENCE_ONLY;NOT_REEXECUTED'),host_owners=['Source-family maintainer'],responsible_future_owner=['Source-family maintainer'])
        issues[identity]=row
    return issues[identity]

def records(value):
    if isinstance(value,dict):
        for key in ['id','issue_id','candidate_id']:
            if isinstance(value.get(key),str) and pattern.fullmatch(value[key]):yield value[key],value
        for child in value.values():yield from records(child)
    elif isinstance(value,list):
        for child in value:yield from records(child)

for source in sources:
    path=ROOT/source['path'];text=path.read_text(encoding='utf-8-sig');lines=text.splitlines()
    for number,line in enumerate(lines,1):
        for alias in sorted(set(pattern.findall(line))):
            issue=ensure(alias,source)
            if not any(x['source']['path']==source['path'] and x['line']==number for x in issue['report_occurrences']):
                occurrence=dict(source=source,line=number,excerpt=line.strip()[:1800],excerpt_sha256=sha(line.encode()))
                issue['report_occurrences'].append(occurrence)
                register['crosswalk'].append(dict(kind='REPORT_OCCURRENCE',original_id=alias,source_path=source['path'],source_line=number,
                    occurrence=len(issue['report_occurrences'])-1,canonical_id=issue['canonical_id']))
            context='\n'.join(lines[max(0,number-9):min(len(lines),number+10)])
            issue.setdefault('source_contexts',[]).append(dict(source=source,line=number,context=context,sha256=sha(context.encode())))
    if path.suffix=='.json':
        for alias,record in records(json.loads(text)):
            issue=ensure(alias,source)
            issue.setdefault('historical_issue_records',[]).append(dict(source=source,record=record,
                record_sha256=sha(json.dumps(record,sort_keys=True,ensure_ascii=False).encode())))
            for field in ['reproducer','reproduction','behavior','reason','observation']:
                if record.get(field):issue.setdefault('historical_observations',[]).append(dict(field=field,value=record[field]))
            for field in ['owner','owners','future_owner','next_owner']:
                if record.get(field):issue.setdefault('historical_owners',[]).append(record[field])
            if record.get('original_obligation'):
                wanted=record['original_obligation'];matches=[m for m in register['crosswalk'] if m['kind']=='ORIGINAL_OBLIGATION' and m['original_id']==wanted]
                issue.setdefault('explicit_original_links',[]).append(dict(id=wanted,matches=matches,status='EXACT' if matches else 'UNKNOWN_BLOCKED;ID_RETAINED'))

for issue in issues.values():
    if not issue['canonical_id'].startswith('ISSUE:'):continue
    text=json.dumps(issue.get('source_contexts',[])+issue.get('historical_issue_records',[]),ensure_ascii=False)
    paths=set(re.findall(r'(?:BESHstatNG|BESHStat\.Core|BESHStat\.Excel\.Common)(?:\.Tests|\.Test)?/[A-Za-z0-9_./-]+\.vb',text))
    for basename in set(re.findall(r'\b[A-Za-z0-9_]+\.vb\b',text)):
        matches=basenames.get(basename.casefold(),[])
        if len(matches)==1:paths.add(matches[0])
    for path in sorted(paths):
        if not (ROOT/path).is_file():continue
        if not any(x.get('preimage',{}).get('path')==path for x in issue['source_locations']):
            candidates=methods.get(path,[])
            named=[m for m in candidates if re.search(r'(?<!\w)'+re.escape(m['name'])+r'(?!\w)',text,re.I)]
            issue['source_locations'].append(dict(preimage=pin(path),linkage='EXACT_PATH_OR_UNIQUE_BASENAME_IN_AUTHENTICATED_REPORT_CONTEXT',
                members=[dict(identity=m['symbol_id'],signature=m['signature'],body_sha256=m['body_sha256'],line=m['start_line']) for m in named],
                unknown_member_reason=None if named else 'Report binds this source file, but no unique current member is independently resolved; historical record retained.'))
    if issue['status']=='UNKNOWN_BLOCKED' and issue['source_locations']:
        issue['status']='HISTORICALLY_REPORTED_RESTRICTED'
        issue['unknown_blocked_reason']='Current exact member linkage may remain unverified; report/file identities are authenticated. No scientific/native resolution inferred.'

# Exact source-backed duplicate:17 no-label implementation and18 dormant activation concern the same unapplied defect.
a=issues['ISSUE:C17-CA-NOLABEL-INDEX-01'];b=issues.pop('ISSUE:C18-CA-NOLABEL-ACTIVATION-01')
assert any('unlabel' in json.dumps(v).lower() for v in b['source_contexts'])
a['aliases']=sorted(set(a['aliases']+b['aliases']))
for key in ['report_occurrences','source_locations','source_contexts','historical_issue_records','historical_observations','historical_owners']:
    a.setdefault(key,[]);a[key]+=b.get(key,[])
a['deduplication_proof']='C17 no-label index and C18 dormant unlabelled mapping explicitly refer to the same original j→j-1 defect; activation restriction remains a separate retained facet.'
for row in register['crosswalk']:
    if row['canonical_id']==b['canonical_id']:row['canonical_id']=a['canonical_id']
register['issues']=[issues[k] for k in sorted(issues)]
register['version']=3;register['schema']='BESHSTAT_CANONICAL_DEFECT_OBLIGATION_REGISTER_04A39_V3'
register['previous']=pin(HERE/'register/defects-v2.json')
register['counts'].update(canonical_entries=len(issues),report_ids=len({x['original_id'] for x in register['crosswalk'] if x['kind']=='REPORT_OCCURRENCE'}),
    report_occurrences=sum(x['kind']=='REPORT_OCCURRENCE' for x in register['crosswalk']),
    statuses=dict(Counter(x['status'] for x in issues.values())))
register['unresolved_linkage_policy']='File/report/member uncertainties remain explicit. Original1514 facets never discharged, even where source software was extracted or a later active correction was owner-approved.'
save('register/defects-v3.json',register)
save('register/current-v1.json',dict(canonical=pin(HERE/'register/defects-v3.json'),schema=register['schema'],
    version=3,supersedes=pin(HERE/'register/defects-v2.json'),reason='Complete structured historical records, G29 candidates, report contexts and explicit source-backed C17/C18 alias; no original records removed.'))
lines=['# Canonical defect and obligations register','',
    'Current authority: [defects-v3.json](defects-v3.json), selected by [current-v1.json](current-v1.json). Versions2/3 are retained. '+
    'All1514 original obligations and802 restricted controls map exactly once. Distinct contracts remain immutable facets. No04A.48 exit disposition is granted.','',
    '| Canonical ID | Status | Obligations | Aliases |','|---|---|---:|---|']
for key,row in sorted(issues.items()):lines.append('| '+key+' | '+row['status']+' | '+str(len(row['original_obligations']))+' | '+', '.join(row['aliases'])+' |')
(HERE/'register/index-v3.md').write_text('\n'.join(lines)+'\n',encoding='utf-8',newline='\n')
print(json.dumps(register['counts']),flush=True)
