"""Bind deferred report identifiers to actual current declarations, preserving ambiguity explicitly."""
from common39 import *
from collections import Counter
import re

source_lock()
d=load(HERE/'register/defects-v3.json');files=load(HERE/'entry/source-preimages-v1.json')['files'];declarations=[]
for row in files:
    path=row['path']
    if not path.endswith('.vb') or not path.startswith(('BESHstatNG/','BESHStat.Core/','BESHStat.Excel.Common/')):continue
    lines=(ROOT/path).read_text(encoding='utf-8-sig').splitlines()
    for i,line in enumerate(lines):
        if line.lstrip().startswith("'"):continue
        match=re.match(r'\s*(?:(?:Public|Private|Friend|Protected|Shared|Partial|Overrides|Overloads|NotOverridable|Overridable|Async|Iterator|MustOverride|NotInheritable|MustInherit|Default|ReadOnly|WriteOnly|Shadows)\s+)*(Class|Module|Sub|Function|Property)\s+(\w+)',line,re.I)
        if not match:continue
        kind,name=match.groups()
        if name in ['New','Compute','compute','Fit','Plot','GetData','ToString','Equals','Dispose','GetEnumerator']:continue
        if len(name)<6:continue
        signature=line.strip();j=i
        while j+1<len(lines) and (signature.endswith('_') or signature.count('(')>signature.count(')')) and j-i<24:
            j+=1;signature+='\n'+lines[j].strip()
        declarations.append(dict(path=path,kind=kind,member=name,signature=signature,line=i+1,header_sha256=sha(signature.encode()),source_preimage=row))
linked=0
for issue in d['issues']:
    if not issue['canonical_id'].startswith('ISSUE:'):continue
    # Whole source dossier is an authenticated context, not a claim that every declaration it names is the faulty branch.
    context='\n'.join(x['context'] for x in issue.get('source_contexts',[]))+'\n'+json.dumps(issue.get('historical_issue_records',[]),ensure_ascii=False)
    direct=[r for r in declarations if re.search(r'(?<![\w])'+re.escape(r['member'])+r'(?![\w])',context)]
    if not direct:
        paths={x['source']['path'] for x in issue['report_occurrences']}
        context='\n'.join((ROOT/p).read_text(encoding='utf-8-sig') for p in sorted(paths))
        direct=[r for r in declarations if re.search(r'(?<![\w])'+re.escape(r['member'])+r'(?![\w])',context)]
        scope='WHOLE_AUTHENTICATED_DOSSIER;CANDIDATE_DECLARATIONS_NOT_FAULT_ADJUDICATION'
    else:scope='EXACT_ISSUE_CONTEXT_IDENTIFIERS;CANDIDATE_DECLARATIONS_NOT_SEMANTIC_BINDING'
    issue['current_declaration_candidates']=direct
    issue['declaration_linkage_scope']=scope
    if direct:
        linked+=1
        if issue['status']=='UNKNOWN_BLOCKED':issue['status']='HISTORICALLY_REPORTED_RESTRICTED'
        issue['unknown_blocked_reason']='Exact historical report and current declarations are pinned. Which candidate branch is scientifically/native faulty is unadjudicated; source-identical behavior remains restricted.'
    else:
        issue['status']='UNKNOWN_BLOCKED';issue['unknown_blocked_reason']='No literal current declaration identifier can be established from this historical issue context/dossier. Immutable ID and source report retained for future owner review.'
d['version']=4;d['schema']='BESHSTAT_CANONICAL_DEFECT_OBLIGATION_REGISTER_04A39_V4'
d['previous']=pin(HERE/'register/defects-v3.json')
d['counts']['statuses']=dict(Counter(x['status'] for x in d['issues']))
d['counts']['issue_entries_with_current_declaration_candidates']=linked
save('register/defects-v4.json',d)
save('register/current-v2.json',dict(canonical=pin(HERE/'register/defects-v4.json'),schema=d['schema'],version=4,
    supersedes=pin(HERE/'register/current-v1.json'),reason='Add exact current declaration candidates, signatures, source hashes and lines from authenticated report identifiers. Ambiguous scientific/native ownership remains explicit.'))
unknown=[dict(id=x['canonical_id'],reason=x['unknown_blocked_reason'],reports=x['report_occurrences']) for x in d['issues'] if x['status']=='UNKNOWN_BLOCKED']
save('register/unresolved-linkages-v1.json',dict(entries=unknown,native_scientific_issue_status='All historical restrictions remain open; this is a traceability register, not final04A48 exit.'))
lines=['# Current04A.39 canonical register','',
    'Authority: [current-v2.json](current-v2.json) → [defects-v4.json](defects-v4.json). Earlier versions remain retained. '+
    'All1514 original obligations and802 restricted controls map exactly once. Current declarations are source-bound candidates; no native/scientific discharge or04A.48 exit disposition is granted.','',
    '| Canonical ID | Status | Original obligations | Aliases |','|---|---|---:|---|']
for row in d['issues']:lines.append('| '+row['canonical_id']+' | '+row['status']+' | '+str(len(row['original_obligations']))+' | '+', '.join(row['aliases'])+' |')
(HERE/'register/index-v4.md').write_text('\n'.join(lines)+'\n',encoding='utf-8',newline='\n')
print(json.dumps(d['counts']),flush=True)
