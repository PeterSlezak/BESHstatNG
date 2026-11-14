"""Independent C7 source/CIL/branch/ownership reader. Always partial NOT_READY."""
import hashlib,json,re,sys
from collections import Counter,defaultdict
from pathlib import Path
from source_views import body
from verify_composition_v1 import inspect as source_composition
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
C6='evidence/04A38/completion/w2/continuation-v6/'
W1='evidence/04A38/completion/w1/'
AUTH={} # Sealed version embeds exact authenticated proof-input hashes.
CHANGED={('BESHStatNG.UdfDataImport','TryGetTrimmedColumnObject'),('BESHStatNG.UdfDataImport','TryGetTrimmedNumericMatrixObject'),('BESHStatNG.WorksheetFunctions.AssumptionsUDFs','BOX_M')}
NEW_TARGETS={('BESHStatNG.UdfDataImport','TryGetTrimmedColumnObject'):('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38','CopyTrimmedColumnPrepared'),('BESHStatNG.UdfDataImport','TryGetTrimmedNumericMatrixObject'):('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38','CopyTrimmedMatrixPrepared'),('BESHStatNG.WorksheetFunctions.AssumptionsUDFs','BOX_M'):('BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38','TryBuildBoxCovariances')}
def need(value,message):
    if not value:raise ValueError(message)
def sha(data):return hashlib.sha256(data).hexdigest()
def semantic(row):return {k:v for k,v in row.items() if k!='raw_IL_sha256'}
def definitions(data):
    result={r['method']:r for r in data['methods']};need(len(result)==len(data['methods']),'Duplicate complete CLR method');return result
def assembly_of_method(key):return key.split(' @ ',1)[1].split(',',1)[0]
def closure(data,keys):
    methods=definitions(data);pending=list(keys);seen=set();external=set();edges=[]
    while pending:
        key=pending.pop()
        if key in seen:continue
        row=methods.get(key)
        if row is None:external.add(key);continue
        seen.add(key)
        for index,ins in enumerate(row['instructions']):
            if ins['op'] in ['call','callvirt','newobj','ldftn','ldvirtftn'] and isinstance(ins['operand'],str) and ins['operand'].startswith('METHOD:'):
                target=ins['operand'][7:];edges.append(dict(caller=key,instruction=index,op=ins['op'],target=target));pending.append(target)
    need(not any(assembly_of_method(key) in ['BESHStatNG','BESHStat.Core','BESHStat.Excel.Common'] for key in external),'Unresolved actual internal constructed/interface/compiler context')
    return dict(methods=sorted(seen),external_runtime_or_ExcelDna=sorted(external),edges=edges)
def headers(universe,closed_support):
    result={}
    for support in universe['supports']:
        if support['support_id'] not in closed_support or support['kind']!='ModuleBlock':continue
        for f in universe['fragments']:
            if f['member_id'] is None and f['path']==support['path'] and f['kind'] in ['ModuleStatement','EndModuleStatement'] and support['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=support['span_start']+support['span_length']:
                result[f['fragment_id']]=support['support_id']
    return result

def compute(root=ROOT,overrides=None,check_ledgers=True):
    root=Path(root);overrides=overrides or {};cache={}
    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()
    def load(path):
        if path not in cache:cache[path]=json.loads(raw(path))
        return cache[path]
    def check(row):
        data=raw(row['path']);need((len(data),sha(data))==(row['bytes'],row['sha256']),'Source/evidence pin mismatch '+row['path'])
    for path,digest in AUTH.items():need(sha(raw(PREFIX+path))==digest,'Authenticated C7 input changed '+path)
    inputs=load(PREFIX+'proof-inputs-v1.json');contracts=load(PREFIX+'whole-body-contracts-v1.json')['contracts']
    transitions=load(PREFIX+'source-transitions-final-v1.json')['files'];projected=dict(overrides)
    need(len(transitions)==7 and len({r['path'] for r in transitions})==7,'Overlapping/missing source transition')
    for t in transitions:
        for key in ['after','preimage','snapshot']:check(t[key])
        need(raw(t['after']['path'])==raw(t['snapshot']['path']),'Current after-image mismatch')
        need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Forged exact entry preimage')
        projected[t['path']]=raw(t['preimage']['path'])
    # Re-execute the prior fail-closed proof against the authenticated C7 entry view.
    sys.path.append(str(root/C6))
    from verify_checkpoint_v5 import compute as c6_compute
    oldproof=c6_compute(root,projected)
    need(oldproof['historical']==dict(total=704,closed=248,OPEN=456),'C6 inherited historical proof changed')
    current=load(PREFIX+inputs['current_universe']);original=load(W1+'original-universe.json')
    oldcurrent=load(C6+'current-ownership-v1.json');oldhistory=load(C6+'historical-ownership-v1.json')['members']
    members={n['symbol_id']:n for n in current['members']};originals={n['symbol_id']:n for n in original['members']}
    need(len(members)==len(current['members'])==724 and len(oldhistory)==704,'Current/historical census identity multiplicity')
    for source in current['sources']:check(source)
    bykey=defaultdict(list);byowner=defaultdict(list)
    for n in current['members']:
        bykey[n['signature_key']].append(n)
        if n['parent_member'] is None:byowner[n['owner'],n['name']].append(n)
    inherited=set()
    for before in oldcurrent['members']:
        if before['review_status'].startswith('OPEN'):continue
        candidates=[n for n in bykey[before['signature_key']] if n['body_token_sha256']==before['body_token_sha256'] and (n['parent_member'] or '').split('@')[0]==(before['parent_member'] or '').split('@')[0]]
        need(len(candidates)==1,'Changed/lost inherited complete source body');inherited.add(candidates[0]['symbol_id'])
    history_by_id={h['canonical_original_id']:h for h in oldhistory}
    reviewed={c['current_id'] for c in contracts};need(len(reviewed)==len(contracts)==59,'Overlapping/missing full contracts')
    newhistory=set();newcurrent=set();snapshots={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
    for c in contracts:
        n=members[c['current_id']];need(c['current_record']==n,'Full current body identity differs')
        check(c['current_source']);text,view=body(raw(n['path']),n)
        need((text,view)==(c['current_whole_body'],c['current_source_view']),'Full current source/UTF16 view differs')
        need(c['new_support']==(c['current_id'] not in inherited),'Invented new/current closure')
        if c['new_support']:newcurrent.add(c['current_id'])
        if c['original_id'] is not None:
            h=history_by_id[c['original_id']];old=originals[h['git_original_id']]
            need(c['original_record']==old and c['original_source']==snapshots[h['path']],'Canonical original/full source mapping differs')
            check(c['original_source']);text,view=body(raw(c['original_source']['path']),old)
            need((text,view)==(c['original_whole_body'],c['original_source_view']),'Original full body/source hash/view differs')
            need(c['new_support']==(not bool(h['proof'])),'Reopened/forged historical closure')
            if c['new_support']:newhistory.add(c['original_id'])
        else:need((n['name']=='IsBinaryCell38' and c['new_support']) or (not c['new_support'] and c['current_id'] in inherited),'Invented historical/current-only identity')
        need(len(c['review'])>150,'Missing concrete whole-parent boundary review')
        for key,universe,parent in [('current_fragments',current,n['symbol_id']),('original_fragments',original,h['git_original_id'] if c['original_id'] else None)]:
            need(c[key]==[f for f in universe['fragments'] if parent is not None and f['member_id']==parent],'Missing individual statement/branch/inline fragment')
    need(newhistory==set(inputs['new_historical_ids']) and len(newhistory)==26,'Canonical newly supported set differs')
    priority=load(C6+'remaining-work-queue-v1.json')['first_members'];need(inputs['priority']==priority and len(priority)==24,'Changed precise handover queue')
    need({r['original_id'] for r in priority}<=newhistory,'Priority canonical body remains unsupported')
    closed=inherited|reviewed;callbacks=load(PREFIX+'callback-proofs-v1.json')['rows']
    need(len(callbacks)==len({r['current_id'] for r in callbacks})==16,'Callback multiplicity differs')
    closed.update(r['current_id'] for r in callbacks)
    need(set(inputs['reviewed_current'])==reviewed|{r['current_id'] for r in callbacks},'Current-only or callback omitted')
    contexts={};retention=[]
    for config in ['Debug','Release']:
        now=load(PREFIX+inputs['metadata_prefix']+'current-'+config+'.json');before=load(PREFIX+inputs['metadata_prefix']+'original-'+config+'.json');old=load(C6+'metadata-full/families-v2/current-'+config+'.json')
        need(now['metadata_only'] and before['metadata_only'] and before['original_semantic_calls']==0,'Original execution authority exceeded')
        methods=definitions(now);prior=definitions(old);original_methods=definitions(before);types={t['type']:t for t in now['types']}
        changed=set()
        for key,row in prior.items():
            need(key in methods,'Lost retained complete C6 CIL method')
            if semantic(row)!=semantic(methods[key]):changed.add((row['declaring_type'],row['member_name']))
        need(changed==CHANGED,'Unreviewed complete C6 CIL transition')
        for t in old['types']:need(types.get(t['type'])==t,'Changed retained C6 field/closure/type context')
        core_original={k:r for k,r in original_methods.items() if assembly_of_method(k)=='BESHStat.Core'}
        need(all(k in methods and semantic(r)==semantic(methods[k]) for k,r in core_original.items()),'Original Core engine/constructed-method drift')
        for t in before['types']:
            if t['assembly']=='BESHStat.Core':need(types.get(t['type'])==t,'Original Core field/closure context drift')
        for (owner,name),(target_owner,target_name) in NEW_TARGETS.items():
            caller=[r for r in methods.values() if (r['declaring_type'],r['member_name'])==(owner,name)];target=[r for r in methods.values() if (r['declaring_type'],r['member_name'])==(target_owner,target_name)]
            need(len(caller)==len(target)==1 and sum(i['op']=='call' and i['operand']=='METHOD:'+target[0]['method'] for i in caller[0]['instructions'])==1,'Changed actual Windows portable consumer')
        wrapper=[r for r in methods.values() if r['declaring_type']=='BESHStatNG.Matrix.Matrix' and r['member_name']=='MatCovar']
        need(len(wrapper)==1,'MatCovar facade missing');target='METHOD:'+next(r['method'] for r in methods.values() if r['declaring_type']=='BESHStatNG.Matrix.MatrixStatisticsCore' and r['member_name']=='SampleCovariance')
        need(sum(i['op']=='call' and i['operand']==target for i in wrapper[0]['instructions'])==1,'Changed MatCovar engine target')
        root_keys=[]
        for c in contracts:
            bound=next(r for r in c['compiled'] if r['configuration']==config)
            expected=[r['method'] for r in methods.values() if (r['declaring_type'],r['member_name'])==(c['current_record']['owner'],c['current_record']['name'])]
            need(bound['roots']==expected,'Wrong overload/arity/array-rank compiled root')
            graph=closure(now,expected);need(bound['transitive']==graph,'Missing transitive branch/callback/catch dependency')
            candidates=sorted({n['symbol_id'] for key in graph['methods'] for n in byowner.get((methods[key]['declaring_type'],methods[key]['member_name']),[])})
            need(bound['source_dependencies']==candidates and set(candidates)<=closed,'OPEN native source dependency under claimed parent')
            if c['current_id'] in inputs['root_current_ids']:root_keys.extend(expected)
        root_members=[members[key] for key in inputs['root_current_ids']]
        root_keys=[r['method'] for r in methods.values() if any((r['declaring_type'],r['member_name'])==(n['owner'],n['name']) for n in root_members)]
        need(load(PREFIX+'transitive-compiled-context-v1.json')['configurations'][config]==closure(now,root_keys),'Root transitive proof differs')
        for row in callbacks:
            n=members[row['current_id']];need(row['current_record']==n and n['parent_member']==row['parent_id'] and row['parent_id'] in reviewed,'Orphaned callback')
            check(row['source']);text,view=body(raw(n['path']),n);need((text,view)==(row['whole_body'],row['source_view']),'Changed callback body')
            binding=next(r for r in row['configurations'] if r['configuration']==config);parent=next(c for c in contracts if c['current_id']==row['parent_id']);bound=next(r for r in parent['compiled'] if r['configuration']==config)
            need(binding['parent_method'] in bound['roots'],'Wrong callback caller')
            siblings=sorted([m for m in current['members'] if m['parent_member']==row['parent_id']],key=lambda r:r['span_start'])
            targets=[i['operand'][7:] for i in methods[binding['parent_method']]['instructions'] if i['op']=='ldftn' and i['operand'][7:] in methods and methods[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
            need(len(targets)==len(siblings) and targets[siblings.index(n)]==binding['target'],'Lost real callback/source ordinal')
            need(binding['complete_method']==methods[binding['target']] and binding['declaring_type']==types[methods[binding['target']]['declaring_type']],'Changed complete callback/fields/instructions')
        need({r['current_id'] for r in callbacks}=={n['symbol_id'] for n in current['members'] if n['parent_member'] in reviewed},'Missing source callback')
        actual=load(PREFIX+'inspection/packet-v1/Inspect38-'+config+'.json')['exports'];frozen=load('evidence/04A38/preedit/compiled-'+config+'.json')['exports']
        need(len(actual)==268 and sorted(actual,key=lambda r:r['effective_excel_name'])==sorted(frozen,key=lambda r:r['effective_excel_name']),'Changed public export/default/attribute')
        # If retained compile artifacts are available, bind their exact bytes too.
        for a in now['assemblies']:
            library=root/'.test-rationalization-04A38.tmp/completion-w2-cont7/packet-v1'/config/'lib'/a['file_name']
            if library.is_file():need((len(library.read_bytes()),sha(library.read_bytes()))==(a['bytes'],a['sha256']),'Compiled current DLL bytes differ')
        contexts[config]=now;retention.append(dict(configuration=config,retained_C6_methods=len(prior)-3,changed=3,original_Core_methods=len(core_original),original_Core_types=sum(t['assembly']=='BESHStat.Core' for t in before['types']),exports=268,compiler_aliases=[]))
    dispositions=[]
    for h in oldhistory:
        keys={r.split('@')[0] for r in h['current_ids']}|{h['git_original_id'].split('@')[0]}
        mapped=sorted({n['symbol_id'] for key in keys for n in bykey.get(key,[])})
        supported=bool(h['proof']) or h['canonical_original_id'] in newhistory
        if supported:need(set(mapped)<=closed,'Lost supported canonical current mapping')
        dispositions.append(dict(id=h['canonical_original_id'],current_ids=mapped,supported=supported))
    old_support={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in oldcurrent['support']};support=[]
    for s in current['supports']:
        old=old_support[s['path'],s['kind'],s['owner'],s['declaration']];supported=old['fully_reconciled'];children=[]
        if s['kind']=='ModuleBlock' and s['declaration'].split()[-1]=='AssumptionsUDFs':
            children=[n['symbol_id'] for n in current['members'] if n['path']==s['path'] and s['span_start']<=n['span_start'] and n['span_start']+n['span_length']<=s['span_start']+s['span_length']]
            need(children and set(children)<=closed,'Assumptions container has an OPEN child')
            for config in contexts:
                t=next(t for t in contexts[config]['types'] if t['type']=='BESHStatNG.WorksheetFunctions.AssumptionsUDFs')
                need(t['fields']==[],'Mutable stateless module context changed')
            supported=True
        support.append(dict(id=s['support_id'],supported=supported,distinct=not s['already_in_primary_population'],children=children))
    closed_support={s['id'] for s in support if s['supported']};current_headers=headers(current,closed_support)
    closed_support_keys={(s['path'],s['kind'],s['owner'],s['declaration']) for s in current['supports'] if s['support_id'] in closed_support}
    original_headers=headers(original,{s['support_id'] for s in original['supports'] if (s['path'],s['kind'],s['owner'],s['declaration']) in closed_support_keys})
    original_closed={h['git_original_id'] for h,d in zip(oldhistory,dispositions) if d['supported']}
    def count(fragments,covered):return dict(total=len(fragments),branches=sum(f['is_branch'] for f in fragments),inline=sum(f['is_inline'] for f in fragments),covered=sum(covered(f) for f in fragments),branches_covered=sum(f['is_branch'] and covered(f) for f in fragments),inline_covered=sum(f['is_inline'] and covered(f) for f in fragments))
    fragments=dict(current=count(current['fragments'],lambda f:f['member_id'] in closed or f['fragment_id'] in current_headers),original=count(original['fragments'],lambda f:f['member_id'] in original_closed or f['fragment_id'] in original_headers))
    hs=dict(total=704,closed=sum(d['supported'] for d in dispositions),OPEN=sum(not d['supported'] for d in dispositions));cs=dict(total=724,closed=len(closed),OPEN=724-len(closed));ss=dict(raw=len(support),distinct=sum(s['distinct'] for s in support),distinct_closed=sum(s['distinct'] and s['supported'] for s in support),distinct_OPEN=sum(s['distinct'] and not s['supported'] for s in support))
    need(hs['OPEN']>0 and cs['OPEN']>0 and ss['distinct']==303 and ss['distinct_OPEN']>0,'Unsupported READY/inventory change')
    totals=Counter();comparisons=load(PREFIX+'compatibility-packet-v1.json')['results'];oldcomparisons={(r['configuration'],r['corpus']):r for r in load(C6+'compatibility-families-v2.json')['results']}
    for r in comparisons:
        previous=oldcomparisons[r['configuration'],r['corpus']]
        need(all(r[k]==previous[k] for k in ['original','inputs','recorder','cases']),'Changed original reference/input/recorder chronology')
        for key in ['current','original','inputs','recorder']:check(r[key])
        need(raw(r['current']['path'])==raw(r['original']['path']) and r['exact'],'Current frozen compatibility mismatch')
        need(len([s for s in raw(r['inputs']['path']).splitlines() if s.strip()])==r['cases'],'Changed frozen input population')
        totals[r['configuration'],'late' if r['corpus'] in list('ABCDEFG') else 'preedit']+=r['cases']
    need(len(comparisons)==24 and dict(totals)=={(c,p):n for c in ['Debug','Release'] for p,n in [('preedit',28966),('late',458)]},'Frozen population/provenance mismatch')
    composition=source_composition(root,overrides)
    result=dict(status='INDEPENDENT_C7_PARTIAL_SOURCE_CIL_BRANCH_FROZEN_PASS_NOT_READY',historical=hs,current=cs,support=ss,fragments=fragments,newly_supported_historical=sorted(newhistory),newly_supported_current=sorted(newcurrent),closed_current=sorted(closed),reviewed_current=sorted(reviewed),callbacks=sorted(r['current_id'] for r in callbacks),historical_dispositions=dispositions,support_dispositions=support,header_fragments=current_headers,original_header_fragments=original_headers,compiled_retention=retention,source_composition=composition,W2='IN_PROGRESS',W3='NOT_STARTED',original_queries=0)
    if check_ledgers:
        summary=load(PREFIX+'ownership-summary-v1.json');hist=load(PREFIX+'historical-ownership-v1.json')['members'];cur=load(PREFIX+'current-ownership-v1.json')
        need(summary['status']=='NOT_READY_04A38' and summary['W2']=='IN_PROGRESS' and summary['W3']=='NOT_STARTED','False READY/W3 promotion')
        need(all(summary[k]==result[k] for k in ['historical','current','support','fragments']),'Forged ownership/fragment totals')
        need(len(hist)==704 and [r['canonical_original_id'] for r in hist]==[r['id'] for r in dispositions],'Canonical historical set/order altered')
        for r,d,old in zip(hist,dispositions,oldhistory):
            need(r['current_ids']==d['current_ids'] and bool(r['proof'])==d['supported'],'Unsupported historical closure')
            if old['proof']:need(r['proof']==old['proof'],'Inherited proof authority rewritten')
            need(all(r[k]==v for k,v in old.items() if k not in ['current_ids','proof','disposition','partial_implementation']),'Historical identity/source lineage changed')
        need(Counter(r['symbol_id'] for r in cur['members'])==Counter(members.keys()),'Current identities missing/duplicated')
        need({r['symbol_id'] for r in cur['members'] if not r['review_status'].startswith('OPEN')}==closed,'Current OPEN body falsely closed')
        need(all(all(r[k]==v for k,v in members[r['symbol_id']].items()) for r in cur['members']),'Current source metadata altered')
        need(Counter(r['support_id'] for r in cur['support'])==Counter(s['support_id'] for s in current['supports']),'Missing support declaration')
        need({r['support_id'] for r in cur['support'] if r['fully_reconciled']}==closed_support,'Unsupported support closure')
        support_source={r['support_id']:r for r in current['supports']}
        need(all(all(r[k]==v for k,v in support_source[r['support_id']].items()) for r in cur['support']),'Support source/span/field declaration altered')
        need(Counter(r['fragment_id'] for r in cur['fragments'])==Counter(f['fragment_id'] for f in current['fragments']),'Fragment missing/duplicated')
        fragment_source={r['fragment_id']:r for r in current['fragments']}
        for f in cur['fragments']:
            need(all(f[k]==v for k,v in fragment_source[f['fragment_id']].items()),'Fragment statement/branch/inline source altered')
            need((f['review_status']=='COVERED_BY_REVIEWED_BODY_OR_DECLARATION')==(f['member_id'] in closed or f['fragment_id'] in current_headers),'Branch/inline status forged')
        queue=load(PREFIX+'remaining-work-queue-v1.json')
        need(Counter(r['original_id'] for r in queue['members'])==Counter(r['id'] for r in dispositions if not r['supported']),'OPEN canonical queue differs')
        need(Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(set(members)-closed),'OPEN current queue differs')
        old_rows=load(PREFIX+'original-fragment-parent-crosswalk-v1.json')['rows'];need(Counter(r['fragment_id'] for r in old_rows)==Counter(f['fragment_id'] for f in original['fragments']),'Original fragments missing/duplicate')
        omap={f['fragment_id']:f for f in original['fragments']}
        for r in old_rows:
            f=omap[r['fragment_id']];need(r['parent']==f['member_id'] and r['is_branch']==f['is_branch'] and r['is_inline']==f['is_inline'],'Original fragment source mapping changed')
            need((r['status']=='COVERED_BY_PROVEN_BODY_OR_DECLARATION')==(f['member_id'] in original_closed or f['fragment_id'] in original_headers),'Original branch/inline coverage forged')
    return result

if __name__=='__main__':
    try:
        result=compute();print(json.dumps({k:result[k] for k in ['status','historical','current','support','fragments','W2','W3']}))
    except Exception as error:
        print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))));raise SystemExit(1)
