"""Method/branch/consumer map from actual current Roslyn syntax, not W2 closure totals."""
from common39 import *
from collections import Counter, defaultdict
import re

source_lock()
data=load(HERE/'map/current-source-syntax-v1.json')
old=load(ROOT/'evidence/04A38/entry-members.json')
public=lambda r: (r['effective_excel_name'],r['header_tokens'])
assert Counter(map(public,data['exports']))==Counter(map(public,old['exports']))
members=data['members'];by_name=defaultdict(list)
for row in members:by_name[row['name'].casefold()].append(row)
incoming=defaultdict(list);edges=[]
for row in members:
    for ordinal,call in enumerate(row['calls']):
        name=re.sub(r'\(Of.*','',call['expression'].split('.')[-1]).strip().casefold()
        candidates=by_name.get(name,[])
        edge=dict(caller=row['symbol_id'],ordinal=ordinal,line=call['line'],expression=call['expression'],text=call['text'],
                  candidate_targets=[x['symbol_id'] for x in candidates],binding='SYNTAX_CANDIDATES; COMPILED_EDGES_SEPARATELY_REQUIRED')
        edges.append(edge)
        for target in candidates:incoming[target['symbol_id']].append(dict(caller=row['symbol_id'],line=call['line'],expression=call['expression']))
focus=[m for m in members if '/UDFdataImport/' in m['path'] or m['path'].endswith('/ExcelArgModules.vb')]
policies=[]
for m in focus:
    body=m['body_tokens'];portable=[c for c in m['calls'] if '.ExcelCommon.' in c['expression'] or '.WorksheetCompatibility.' in c['expression']]
    host=[c for c in m['calls'] if any(t in c['expression'] for t in ['XlCall','WindowsSpreadsheetTransport','ExcelDna','TryGetCachedHandle','cache.'])]
    context=' '.join(m['enclosing'])+' '+m['containing_type']+' '+body
    if 'UdfCacheHelpers' in context: owner='WINDOWS_SESSION_CACHE';reason='Handles, dictionary ownership/lifetime and concurrency remain04A.43 Windows scope.'
    elif portable: owner='COMMON_POLICY_WITH_WINDOWS_SENTINEL_OR_ACQUISITION';reason='Already calls accepted Core/Common implementation; retain surrounding host policy, callbacks and ByRef sequencing.'
    elif host or any(t in body for t in ['ExcelReference','ExcelError','ExcelEmpty','ExcelMissing','Get2D','TryGetDouble','CellToTrimmedText']):
        owner='WINDOWS_ARGUMENT_BOUNDARY';reason='Excel sentinel, range identity, exception/ByRef or host callback composition; no equivalent universal policy is inferred.'
    else:owner='WINDOWS_COMPOSITION_REVIEWED_RETAINED';reason='Small composition of existing helpers or callback; extracting it would add redundant wrapper/API without a new shared policy.'
    behavior=[]
    for statement in m['statements']:
        text=statement['tokens']
        tags=[tag for pattern,tag in [(r'\bByRef\b','BYREF_STATE'),(r'\bReturn\b','RETURN_PRECEDENCE'),
            (r'\bThrow\b|\bCatch\b|\bTry\b','EXCEPTION_ORDER'),(r'\bIf\b|\bCase\b|\bWhile\b|\bFor\b','BRANCH_OR_ENUMERATION'),
            (r'\bClone\b','CLONE_ALIAS'),(r'Culture|ToString|ToDouble|Parse','CULTURE_CONVERSION'),
            (r'Excel|Get2D|Range|Coerce','HOST_VALUE_OR_REFERENCE')] if re.search(pattern,text,re.I)]
        if tags:behavior.append(dict(statement,contracts=tags))
    policies.append(dict(identity=m['symbol_id'],path=m['path'],member=m['name'],signature=m['signature'],
        source_sha256=m['source_sha256'],body_sha256=m['body_sha256'],token_sha256=m['body_token_sha256'],
        line_span=[m['start_line'],m['end_line']],span_utf16=[m['span_start'],m['span_length']],kind=m['kind'],
        byref_parameters=re.findall(r'ByRef\s+\w+',m['signature'],re.I),owner=owner,decision='REUSE_EXISTING_POLICY_OR_RETAIN_HOST_BOUNDARY',reason=reason,
        common_calls=portable,host_calls=host,all_calls=m['calls'],incoming_syntax_candidates=incoming[m['symbol_id']],
        assignments=m['assignments'],catches=m['catches'],enclosing=m['enclosing'],branches_and_behavior=behavior,
        production_change=False,scientific_native_restriction='NOT_GOLDEN;NO_ACTIVATION;CACHE_04A43;NATIVE_04A45_48'))
save('map/method-policy-map-v1.json',dict(source=pin(HERE/'map/current-source-syntax-v1.json'),
    sources=data['sources'],focus_source_count=len({m['path'] for m in focus}),focus_members=len(focus),
    focus_kinds=dict(Counter(m['kind'] for m in focus)),all_mapped_members=len(members),methods=policies,
    decisions=dict(Counter(x['owner'] for x in policies)),exports=268,export_signature_multiset_exact=True,
    extraction_decision='No production extraction: accepted38 already owns portable policies. New duplicate parser/APIs/wrapper churn intentionally avoided.',
    syntax_limit='Names can be overloaded/ambiguous; syntax candidates are not asserted compiled binding. Shipping metadata supplies exact compiled call targets.'))
save('map/caller-consumer-edges-v1.json',dict(edges=edges,scope='Explicit101-source syntax domain; external callers found separately by frozen compiled metadata'))
index=['# 04A.39 method policy index','',f'{len(focus)} current member/lambda/initializer identities in {len({m["path"] for m in focus})} focused source files. '+
       'Every branch/statement, ByRef parameter, assignment and callback remains in the machine map. No production code changed.','',
       '| Member | Source line | Owner |','|---|---|---|']
for x in policies:index.append('| '+x['member']+' | '+x['path']+':'+str(x['line_span'][0])+' | '+x['owner']+' |')
(HERE/'map/index-v1.md').write_text('\n'.join(index)+'\n',encoding='utf-8',newline='\n')
print('Mapped',len(focus),'focus members;',len(members),'total;',len(edges),'syntax edges',flush=True)
