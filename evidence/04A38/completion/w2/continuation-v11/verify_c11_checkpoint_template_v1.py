"""Independent fail-closed C11 source/ownership/constructed-CIL reader. Never READY."""
import hashlib
import json
from pathlib import Path
import posixpath
import re
import sys
from collections import Counter
from source_views import body,view
from proof_kernel_v3 import need,map_current,bind,definitions,semantic,closure
from verify_composition_v2 import inspect as composition,tokens,PHASES,unwrap

ROOT=Path(__file__).resolve().parents[5]
HERE=Path(__file__).resolve().parent
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
OLD='evidence/04A38/completion/w2/continuation-v10/'
W1='evidence/04A38/completion/w1/'
AUTH={}

def compute(root=ROOT,overrides=None,check_validation=True):
    root=Path(root);overrides={posixpath.normpath(k):v for k,v in (overrides or {}).items()};cache={}
    def raw(path):
        path=posixpath.normpath(path)
        if path not in cache:cache[path]=overrides[path] if path in overrides else (root/path).read_bytes()
        return cache[path]
    load=lambda path:json.loads(raw(path))
    def check(row):
        data=raw(row['path'])
        need((len(data),hashlib.sha256(data).hexdigest())==(row['bytes'],row['sha256']),'Changed authority bytes '+row['path'])
    for path,digest in AUTH.items():need(hashlib.sha256(raw(path)).hexdigest()==digest,'Sealed C11 input changed '+path)
    trans=load(PREFIX+'source-transitions-final-v1.json')['files']
    need(len(trans)==len({t['path'] for t in trans})==4,'Missing/ambiguous source transition')
    projected=dict(overrides)
    for t in trans:
        check(t['after']);check(t['snapshot'])
        need(raw(t['path'])==raw(t['snapshot']['path']),'Source successor/snapshot mismatch')
        if t['before']:
            check(t['preimage']);need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Wrong immediate preimage')
            projected[t['path']]=raw(t['preimage']['path'])
        else:need(t['entry_state']=='ABSENT','New source lacks absent-entry authority')
    sys.path.insert(0,str(root/OLD))
    from verify_c10_checkpoint_v1 import compute as prior_compute
    prior_proof=prior_compute(root,projected)
    need(prior_proof['historical']==dict(total=704,closed=369,OPEN=335) and prior_proof['current']==dict(total=726,closed=399,OPEN=327),'Prior sealed ownership drift')
    u=load(PREFIX+'current-universe-packet-v2.json');old=load(OLD+'current-ownership-v2.json')
    oldh=load(OLD+'historical-ownership-v2.json')['members'];original=load(W1+'original-universe.json')
    mapping=map_current(old['members'],u['members']);members={n['symbol_id']:n for n in u['members']}
    oldm={n['symbol_id']:n for n in old['members']};reverse={n['symbol_id']:k for k,n in mapping.items()}
    need(len(members)==len(oldm)==726,'Native declaration addition/removal')
    inherited={mapping[n['symbol_id']]['symbol_id'] for n in old['members'] if not n['review_status'].startswith('OPEN')}
    need(len(inherited)==399 and all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN')),'Inherited body changed')
    om={n['symbol_id']:n for n in original['members']}
    shapes={r['symbol_id']:r for r in load(PREFIX+'declaration-shapes-v2.json')['rows']}
    cs=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];callbacks=load(PREFIX+'callback-proofs-v1.json')['rows']
    pc=load(PREFIX+'portable-api-contracts-v1.json')['contracts'];pu=load(PREFIX+'portable-universe-v1.json')
    inputs=load(PREFIX+'proof-inputs-v1.json');diag=load(PREFIX+'cohort-diagnostic-v2.json')
    need(inputs['original_queries']==diag['original_queries']==0,'Unauthorized original-reference activity')
    need(len(cs)==86 and len(callbacks)==10,'Missing complete parent/callback dependency')
    need(len({c['current_id'] for c in cs+callbacks})==96,'Overlapping body contract')
    source_snapshots={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
    oldby={h['canonical_original_id']:h for h in oldh};newhist={};newcurrent=set();contexts={};graphs={};retention=[]
    for c in cs+callbacks:
        n=members[c['current_id']];need(c['current_record']==n,'Wrong current source identity')
        text,policy=body(raw(n['path']),n)
        need(text==c.get('current_whole_body',c.get('whole_body')) and policy==c.get('current_source_view',c.get('source_view')),'Forged actual current body/span')
        check(c.get('current_source',c.get('source')))
        if c['original_id']:
            h=oldby[c['original_id']];o=om[h['git_original_id']]
            need(c['original_record']==o and c['original_source']==source_snapshots[o['path']],'Wrong original source lineage')
            before,normalization=body(raw(c['original_source']['path']),o)
            need(before==c['original_whole_body'] and normalization==c['original_source_view'],'Original full body/span/normalization changed')
        expected=oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')
        need(c['new_support']==expected,'Invented new/retained body disposition')
        if expected:
            need(c['original_id'] is not None and c['original_id'] not in newhist,'Missing/overlapping original closure')
            newhist[c['original_id']]=c;newcurrent.add(n['symbol_id'])
    need(len(newhist)==len(newcurrent)==33 and sum(c['new_support'] for c in callbacks)==2,'New exact closure cohort changed')
    need(set(inputs['new_historical_ids'])==set(newhist) and set(inputs['new_current_supported'])==newcurrent,'Wrong closure authority set')
    constructed=load(PREFIX+'constructed-owner-proofs-v1.json')
    for config in ['Debug','Release']:
        meta=load(PREFIX+'metadata-all/packet-v2-context-v5/current-'+config+'.json')
        before=load(OLD+'metadata-all/packet-v2-context-v5/current-'+config+'.json')
        orig=load(PREFIX+'metadata-all/packet-v2-context-v5/original-'+config+'.json')
        need(raw(PREFIX+'metadata-all/packet-v2-context-v5/original-'+config+'.json')==raw(OLD+'metadata-all/packet-v2-context-v5/original-'+config+'.json'),'Original recorded CIL changed')
        need(meta['metadata_only'] and orig['metadata_only'] and orig['original_semantic_calls']==0,'Original execution provenance changed')
        defs=definitions(meta);bd=definitions(before);od=definitions(orig)
        changed={(m['declaring_type'],m['member_name']) for key,m in bd.items() if key in defs and semantic(m)!=semantic(defs[key])}
        need(set(bd)<=set(defs) and changed=={('BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs',n) for n in PHASES},'Unexpected compiled transition')
        additions=[defs[k] for k in set(defs)-set(bd)]
        need(len(additions)==5 and {(m['declaring_type'],m['member_name']) for m in additions}=={('BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38',v[0]) for v in PHASES.values()},'Unexpected new compiled API')
        roots=[bind(members[k],meta,shapes)['method'] for k in inputs['roots']]
        graph=closure(meta,roots);need(graph==diag['graphs'][config],'Actual constructed dependency graph changed')
        graphs[config]=graph;need(len(graph['methods'])==901,'Missing actual constructed dependency')
        ownernames={(defs[k]['declaring_type'],defs[k]['member_name']) for k in graph['methods']}
        required={n['symbol_id'] for n in u['members'] if n['parent_member'] is None and (n['owner'],n['name']) in ownernames}
        need(required=={c['current_id'] for c in cs},'Missing complete native body dependency')
        ret=load(PREFIX+'return-metadata-'+config+'-v2.json')['methods']
        for c in cs:
            n=members[c['current_id']];method=bind(n,meta,shapes)
            entry=next(b for b in c['compiled'] if b['configuration']==config)
            need(entry['method']==method['method'] and entry['source_parameters']==shapes[n['symbol_id']] and entry['transitive']==closure(meta,[method['method']]),'Wrong actual parent/ByRef/rank/CIL closure')
            need(entry['return_declaration'] in ret and (entry['return_declaration']['declaring_type'],entry['return_declaration']['member_name'],entry['return_declaration']['parameter_types'])==(method['declaring_type'],method['member_name'],method['parameter_types']),'Missing actual parent return type')
            if c['original_id']:
                bm=entry['original_method'];need(bm['method'] in od and bm==od[bm['method']] and (bm['declaring_type'],bm['member_name'],bm['parameter_types'])==(method['declaring_type'],method['member_name'],method['parameter_types']),'Original method binding forged')
                archived=load('evidence/04A38/preedit/compiled-'+config+'.json')['methods']
                need(entry['original_return'] in archived and entry['original_return']['return_type']==entry['return_declaration']['return_type'],'Original compiled return boundary changed')
        expected_callbacks={n['symbol_id'] for n in u['members'] if n['parent_member'] in required}
        pending=set(expected_callbacks)
        while pending:
            parent=pending.pop();children={n['symbol_id'] for n in u['members'] if n['parent_member']==parent}-expected_callbacks
            expected_callbacks|=children;pending|=children
        need(expected_callbacks=={c['current_id'] for c in callbacks},'Missing real child callback')
        callback_methods={}
        for c in callbacks:
            n=members[c['current_id']];parent=members[n['parent_member']]
            pkey=bind(parent,meta,shapes)['method'] if parent['parent_member'] is None else callback_methods[parent['symbol_id']]
            siblings=sorted([v for v in u['members'] if v['parent_member']==parent['symbol_id']],key=lambda v:v['span_start'])
            targets=[x['operand'][7:] for x in defs[pkey]['instructions'] if x['op']=='ldftn' and x['operand'][7:] in defs and defs[x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
            need(len(targets)==len(siblings),'Callback source/actual ldftn mismatch')
            index=next(i for i,v in enumerate(siblings) if v['symbol_id']==n['symbol_id']);method=defs[targets[index]]
            entry=next(v for v in c['configurations'] if v['configuration']==config)
            need(entry['parent_method']==pkey and entry['target']==method['method'] and entry['complete_method']==method,'Wrong actual callback target')
            need(entry['return_declaration'] in ret and entry['return_declaration']['declaring_type']==method['declaring_type'] and entry['return_declaration']['member_name']==method['member_name'],'Callback return type forged')
            need(entry['declaring_type']==next(t for t in meta['types'] if t['type']==method['declaring_type']) and entry['constructors']==[m for m in meta['methods'] if m['declaring_type']==method['declaring_type'] and m['member_name'] in ['.ctor','.cctor']],'Callback fields/constructor missing')
            if c['original_id']:
                o=c['original_record'];op=om[o['parent_member']]
                need(op['signature_key']==parent['signature_key'],'Historical callback relocated to wrong parent')
                oshapes=dict(shapes);oshapes[op['symbol_id']]=shapes[parent['symbol_id']]
                p=bind(op,orig,oshapes);sibs=sorted([v for v in original['members'] if v['parent_member']==op['symbol_id']],key=lambda v:v['span_start'])
                ots=[x['operand'][7:] for x in p['instructions'] if x['op']=='ldftn' and x['operand'][7:] in od and od[x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
                need(len(ots)==len(sibs),'Original callback actual target missing')
                oldmethod=od[ots[next(i for i,v in enumerate(sibs) if v['symbol_id']==o['symbol_id'])]]
                need(entry['original_binding']['target']==oldmethod and entry['original_binding']['parent']==p['method'],'Original callback binding forged')
                need(tokens(c['whole_body'])==tokens(c['original_whole_body']),'Original/current callback source changed')
            callback_methods[n['symbol_id']]=method['method']
        for p in pc:
            n=p['source_record'];need(n in pu['members'],'Unknown portable source identity')
            text,policy=body(raw(n['path']),n);need(text==p['whole_body'] and policy==p['source_view'],'Portable full body changed')
            method=bind(n,meta,shapes);entry=next(v for v in p['compiled'] if v['configuration']==config)
            need(entry['method']==method and entry['context']==next(t for t in meta['types'] if t['type']==n['owner']) and not entry['context']['fields'],'Portable CIL/field context missing')
            need(n['name']=='.ctor' or entry['return_declaration'] in ret,'Portable actual return declaration missing')
        types={t['type']:t for t in meta['types'] if t['type']};pt={t['type']:t for t in before['types'] if t['type']}
        context=next(v for v in constructed['configurations'] if v['configuration']==config)
        need({r['owner'] for r in context['contexts']}=={defs[k]['declaring_type'] for k in graph['methods']},'Constructed owner omitted')
        for r in context['contexts']:
            need(r['type']==types[r['owner']]==r['prior_type']==pt[r['owner']],'Constructed fields/types changed')
            ctors=[m for m in meta['methods'] if m['declaring_type']==r['owner'] and m['member_name'] in ['.ctor','.cctor']]
            need(r['constructors_and_initializers']==ctors and all(semantic(m)==semantic(bd[m['method']]) for m in ctors),'Constructed initializer omitted/changed')
        exports=load(PREFIX+'inspection/packet-v2/Inspect38-'+config+'.json')['exports']
        need(len(exports)==268 and sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(load('evidence/04A38/preedit/compiled-'+config+'.json')['exports'],key=lambda r:r['effective_excel_name']),'Export ABI/default/help/attribute change')
        contexts[config]=meta;retention.append(dict(configuration=config,reached=len(graph['methods']),owners=len(context['contexts']),exports=268))
    source_proof=composition(root,overrides)
    from verify_host_contexts_v1 import inspect as host_inspect
    host_inspect(load,raw,PREFIX,original,u,contexts)
    closed=inherited|{c['current_id'] for c in cs+callbacks}
    history=load(PREFIX+'historical-ownership-v2.json')['members'];current=load(PREFIX+'current-ownership-v2.json')
    need([h['canonical_original_id'] for h in history]==[h['canonical_original_id'] for h in oldh] and len(history)==704,'Historical universe changed')
    for h,before in zip(history,oldh):
        if h['canonical_original_id'] in newhist:
            need(not before['proof'] and h['current_ids']==[newhist[h['canonical_original_id']]['current_id']] and h['proof']['current_id']==h['current_ids'][0],'False or ambiguous new closure')
        else:need(h['proof']==before['proof'] and h['current_ids']==[mapping[k]['symbol_id'] for k in before['current_ids']],'Retained historical authority changed')
    need({n['symbol_id'] for n in current['members']}==set(members),'Current universe differs')
    for n in current['members']:need((not n['review_status'].startswith('OPEN'))==(n['symbol_id'] in closed),'Wrong current OPEN disposition')
    from verify_census_v1 import inspect as census
    summary=census(load,PREFIX,original,u,old,current,history,closed,newhist,contexts)
    need(len(load(PREFIX+'new-closures-final-v1.json')['rows'])==33,'Wrong new-closures ledger')
    queue=load(PREFIX+'remaining-work-queue-v1.json');expected=[r for r in load(OLD+'remaining-work-queue-v1.json')['members'] if r['original_id'] not in newhist]
    need(queue['members']==expected and queue['first_members']==expected[:24] and Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(set(members)-closed),'OPEN queue/order/complement forged')
    ready=load(PREFIX+'readiness-v1.json')
    need((ready['status'],ready['W2'],ready['W3'],ready['P'],ready['N1'],ready['N2'],ready['C2'],ready['C3'],ready['science'],ready['phase04A39'])==('NOT_READY_04A38','IN_PROGRESS','NOT_STARTED','NO','NO GO','NO','UNMET','UNAPPROVED','NOT_GOLDEN','BLOCKED'),'Unsupported readiness/stage promotion')
    comparisons=load(PREFIX+'compatibility-packet-v2.json')['results'];prior={(r['configuration'],r['corpus']):r for r in load(OLD+'compatibility-packet-v2.json')['results']};seen=set();totals=Counter()
    for r in comparisons:
        key=(r['configuration'],r['corpus']);need(key not in seen and key in prior,'Duplicate/unknown compatibility corpus');seen.add(key)
        need(all(r[k]==prior[key][k] for k in ['original','inputs','recorder','cases']),'Original/input/recorder provenance changed')
        for name in ['original','inputs','recorder','current']:check(r[name])
        need(r['exact'] and raw(r['original']['path'])==raw(r['current']['path']),'Fresh current frozen mismatch')
        need(r['current']['path'].startswith(PREFIX+'captures/packet-v2/'),'Saved old current result substituted')
        need(len([line for line in raw(r['inputs']['path']).splitlines() if line.strip()])==r['cases'],'Compatibility case count changed')
        totals[r['configuration'],'late' if r['corpus'] in list('ABCDEFG') else 'preedit']+=r['cases']
    need(seen==set(prior) and len(comparisons)==24 and dict(totals)=={(c,p):n for c in ['Debug','Release'] for p,n in [('preedit',28966),('late',458)]},'Incomplete compatibility/provenance population')
    if check_validation:
        from verify_validation_v1 import inspect as validation
        summary['validation']=validation(load,raw,PREFIX)
    summary.update(compiled_retention=retention,source_composition=source_proof,original_queries=0,
        reader_sealed=bool(AUTH),qualification='Static/managed compatibility only. W3/native/scientific acceptance remains unmet.')
    return summary

if __name__=='__main__':
    try:print(json.dumps(compute(check_validation='--static' not in sys.argv)))
    except Exception as error:
        print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))))
        raise SystemExit(1)
