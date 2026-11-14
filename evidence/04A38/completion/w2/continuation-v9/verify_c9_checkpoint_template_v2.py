"""Independent C9 full-source/CIL/dependency reader. It can only report NOT_READY."""
import json,hashlib,sys,re
from pathlib import Path
from collections import Counter,defaultdict
from source_views import body
from proof_kernel_v1 import need,semantic,definitions,closure,map_current,bind
from verify_c9_composition_v1 import inspect as composition
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;PREFIX=HERE.relative_to(ROOT).as_posix()+'/';OLD='evidence/04A38/completion/w2/continuation-v8/';W1='evidence/04A38/completion/w1/'
AUTH={}
CHANGED={('BESHStatNG.WorksheetFunctions.AgreementMethodUDFs',n) for n in ['PASSINGBABLOK_COEF','DEMING_COEF','BLANDALTMAN_STATS','ICC_RC']}|{('BESHStatNG.WorksheetFunctions.ContingencyTableUDFs',n) for n in ['CT_CHI2','CT_NOMINAL_ASSOC','CT_FISHER_2X2','CT_FFH_EXACT']}|{('BESHStatNG.regression.BinaryClassificationReporting',n) for n in ['BuildBinaryCrosstabUdfOutput','BuildThresholdTableUdfOutput','BuildCalibrationTableUdfOutput','BuildBrierScoreUdfOutput']}
def sha(data):return hashlib.sha256(data).hexdigest()
def compute(root=ROOT,overrides=None,check_validation=True):
    root=Path(root);overrides=overrides or {};cache={}
    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()
    def load(path):
        if path not in cache:cache[path]=json.loads(raw(path))
        return cache[path]
    def check(row):
        data=raw(row['path']);need((len(data),sha(data))==(row['bytes'],row['sha256']),'Source/evidence byte pin mismatch '+row['path'])
    for path,digest in AUTH.items():need(sha(raw(PREFIX+path))==digest,'Authenticated C9 proof input changed '+path)
    inputs=load(PREFIX+'proof-inputs-v1.json');need(inputs['original_queries']==0,'Unauthorized original reference activity')
    transitions=load(PREFIX+'source-transitions-final-v1.json')['files'];need(len(transitions)==len({t['path'] for t in transitions})==8,'Missing/overlapping source transitions');projected=dict(overrides)
    for t in transitions:
        check(t['after']);check(t['snapshot']);need(raw(t['path'])==raw(t['snapshot']['path']),'After-image mismatch')
        if t['before'] is not None:
            check(t['preimage']);need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Immediate source preimage forged');projected[t['path']]=raw(t['preimage']['path'])
        else:need(t['entry_state']=='ABSENT' and t['path'] in {r['path'] for r in load(PREFIX+'implementation-plan-v1.json')['new_sources']},'Unauthenticated new source')
    # C8 publication import version4 explicitly source-pins the intended C7 helper.
    sys.path.insert(0,str(root/OLD));from verify_c8_publication_v4 import compute as prior_compute
    prior_proof=prior_compute(root,projected);need(prior_proof['historical']==dict(total=704,closed=298,OPEN=406),'C8 historical proof drift')
    u=load(PREFIX+inputs['current_universe']);oldcur=load(OLD+'current-ownership-v2.json');oldh=load(OLD+'historical-ownership-v2.json')['members'];original=load(W1+'original-universe.json');mapping=map_current(oldcur['members'],u['members']);reverse={n['symbol_id']:k for k,n in mapping.items()};oldm={n['symbol_id']:n for n in oldcur['members']};members={n['symbol_id']:n for n in u['members']};om={n['symbol_id']:n for n in original['members']}
    need(len(members)==len(u['members'])==726 and len(oldh)==704,'Primary source identity population changed')
    for row in u['sources']:check(row)
    shapesdoc=load(PREFIX+'declaration-shapes-v1.json');shapes={r['symbol_id']:r for r in shapesdoc['rows']}
    for row in shapesdoc['sources']:check(row)
    inherited={mapping[n['symbol_id']]['symbol_id'] for n in oldcur['members'] if not n['review_status'].startswith('OPEN')}
    need(all(members[k]['body_sha256']==oldm[reverse[k]]['body_sha256'] for k in inherited),'Inherited accepted source body altered')
    cs=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];callbacks=load(PREFIX+'callback-proofs-v1.json')['rows'];portable=load(PREFIX+'portable-api-contracts-v1.json')['contracts'];pu=load(PREFIX+'portable-universe-v1.json')
    need(len(cs)==len({c['current_id'] for c in cs})==99 and len(callbacks)==len({c['current_id'] for c in callbacks})==18,'Parent/callback missing or duplicated')
    need({c['current_id'] for c in cs+callbacks}==set(inputs['reviewed_current']),'Required dependency/callback omitted')
    snapshots={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
    hby={h['canonical_original_id']:h for h in oldh};newh=set();newcurrent=set()
    for c in cs+callbacks:
        n=members[c['current_id']];need(n==c['current_record'],'Complete current source record changed');source=c.get('current_source',c.get('source'));check(source);text,view=body(raw(n['path']),n);need((text,view)==(c.get('current_whole_body',c.get('whole_body')),c.get('current_source_view',c.get('source_view'))),'Complete current UTF16 body/view mismatch')
        expected_new=oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN');need(c['new_support']==expected_new,'Existing body falsely recounted or reopened')
        if c['new_support']:newcurrent.add(n['symbol_id'])
        if c['original_id']:
            h=hby[c['original_id']];o=om[h['git_original_id']];need(c['original_record']==o and c['original_source']==snapshots[o['path']],'Canonical original source mapping changed');check(c['original_source']);text,view=body(raw(c['original_source']['path']),o);need((text,view)==(c['original_whole_body'],c['original_source_view']),'Original full source/body/span changed')
            if c['new_support']:need(not h['proof'],'Recounted accepted historical body');newh.add(c['original_id'])
        else:need(n['parent_member'] is not None,'Invented historical parent identity')
    need(newh==set(inputs['new_historical_ids']) and newcurrent==set(inputs['new_current_supported']),'Unsupported closure set')
    need(len(newh)==45 and len(newcurrent)==49,'Unexpected unreviewed source closure population')
    need(len(portable)==len(pu['members'])==15 and {c['source_record']['symbol_id'] for c in portable}=={n['symbol_id'] for n in pu['members']},'Portable declaration missing/hidden')
    for row in pu['sources']:check(row)
    contexts={};retention=[]
    for config in ['Debug','Release']:
        meta=load(PREFIX+inputs['metadata_prefix']+'current-'+config+'.json');before=load(PREFIX+inputs['metadata_prefix']+'original-'+config+'.json');oldmeta=load(OLD+'metadata-all/packet-v1-context-v3/current-'+config+'.json');need(meta['metadata_only'] and before['metadata_only'] and before['original_semantic_calls']==0,'Original execution authority changed')
        defs=definitions(meta);olddefs=definitions(oldmeta);originaldefs=definitions(before);changed=set()
        for key,m in olddefs.items():
            need(key in defs,'Lost original C8 compiled member measurement')
            if semantic(m)!=semantic(defs[key]):changed.add((m['declaring_type'],m['member_name']))
        need(changed==CHANGED,'Unexpected C8 to C9 compiled body transition')
        types={t['type']:t for t in meta['types'] if t['type'] is not None}
        for t in oldmeta['types']:
            if t['type'] is None:continue
            measured=types.get(t['type'])
            if t['type']=='BESHStatNG.psmData+_Closure$__':
                need({f['name'] for f in t['fields']}=={'$I','$I84-0'} and all(f['is_static'] for f in t['fields']),'Unrelated scoped cache context changed');need(dict(t,fields=sorted(t['fields'],key=lambda f:f['name']))==dict(measured,fields=sorted(measured['fields'],key=lambda f:f['name'])),'Unrelated static-cache semantics changed')
            else:need(measured==t,'Retained type/field/closure context changed')
        oldnone=Counter(json.dumps(t,sort_keys=True) for t in oldmeta['types'] if t['type'] is None);nownone=Counter(json.dumps(t,sort_keys=True) for t in meta['types'] if t['type'] is None);need(not oldnone-nownone,'Lost unused open-constructed raw context')
        core={k:m for k,m in originaldefs.items() if k.split(' @ ',1)[1].split(',',1)[0]=='BESHStat.Core'};need(all(k in defs and semantic(m)==semantic(defs[k]) for k,m in core.items()),'Original Core method or constructed context changed')
        for t in before['types']:
            if t['assembly']=='BESHStat.Core' and t['type'] is not None:need(types[t['type']]==t,'Original Core field/ctor context changed')
        returns=load(PREFIX+'return-metadata-'+config+'-v1.json')['methods']
        for c in cs:
            n=members[c['current_id']];m=bind(n,meta,shapes);record=next(r for r in c['compiled'] if r['configuration']==config);need(record['method']==m['method'] and record['source_parameters']==shapes[n['symbol_id']],'Wrong full declared/constructed parent handle');need(record['original_method']==originaldefs[record['original_method']['method']],'Forged original compiled member')
            need(record['transitive']==closure(meta,[m['method']]),'Incomplete transitive internal/inline/callback graph')
            ret=[r for r in returns if (r['declaring_type'],r['member_name'],r['parameter_types'])==(m['declaring_type'],m['member_name'],m['parameter_types'])];need(ret==[record['return_declaration']],'Wrong return type binding')
        for c in callbacks:
            b=next(r for r in c['configurations'] if r['configuration']==config);need(b['complete_method']==defs[b['target']] and b['declaring_type']==types[defs[b['target']]['declaring_type']],'Invented callback target/field context');parent=members[c['parent_id']]
            if parent['parent_member'] is None:pkey=bind(parent,meta,shapes)['method']
            else:pkey=next(r['target'] for p in callbacks if p['current_id']==parent['symbol_id'] for r in p['configurations'] if r['configuration']==config)
            need(b['parent_method']==pkey and any(i['op']=='ldftn' and i['operand']=='METHOD:'+b['target'] for i in defs[pkey]['instructions']),'Missing real immediate ldftn callback consumer')
            ctors=[m for m in meta['methods'] if m['declaring_type']==defs[b['target']]['declaring_type'] and m['member_name'] in ['.ctor','.cctor']];need(b['constructors']==ctors,'Callback ctor/cctor missing')
        for c in portable:
            n=c['source_record'];need(n in pu['members'],'Invented portable source declaration');text,view=body(raw(n['path']),n);need((text,view)==(c['whole_body'],c['source_view']),'Portable complete source differs');m=bind(n,meta,shapes);b=next(b for b in c['compiled'] if b['configuration']==config);need(b['method']==m and b['context']==types[n['owner']] and not b['context']['fields'],'Portable field/ctor/method context differs')
        roots=[bind(members[k],meta,shapes)['method'] for k in inputs['roots']];graph=closure(meta,roots);need(graph==load(PREFIX+'transitive-compiled-context-v1.json')['configurations'][config],'Whole cohort dependency graph changed')
        need(not any(isinstance(i['operand'],str) and i['operand'].startswith('FIELD:.') for k in graph['methods'] for i in defs[k]['instructions']),'Unconstructed reached field owner')
        original_exports=load('evidence/04A38/preedit/compiled-'+config+'.json')['exports'];exports=load(PREFIX+'inspection/packet-v1/Inspect38-'+config+'.json')['exports'];need(len(exports)==268 and sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(original_exports,key=lambda r:r['effective_excel_name']),'Public Excel-DNA ABI/metadata/default drift')
        build=load(PREFIX+'build-packet-v1-'+config+'.json');need(build['all_exit_zero'] and all(r['exit_code']==0 for r in build['commands']),'Current offline compilation failed')
        for a in meta['assemblies']:
            path='.test-rationalization-04A38.tmp/completion-w2-cont9/packet-v1/'+config+'/lib/'+a['file_name']
            need(a['file_name'] in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll'],'Unknown compiled source artifact')
            if (root/'.git').exists():need((root/path).is_file(),'Required live compiled consumer artifact missing')
            if (root/path).is_file():check(dict(path=path,bytes=a['bytes'],sha256=a['sha256']))
        contexts[config]=meta;retention.append(dict(configuration=config,retained_C8_methods=len(olddefs)-12,changed_existing=12,original_Core_methods=len(core),reached_methods=len(graph['methods']),all_reached_owners_concrete=True,new_metadata_rows=len(defs)-len(olddefs),new_source_methods=15,newly_measured_existing_scope=len(defs)-len(olddefs)-15,unused_open_constructed_before=sum(oldnone.values()),unused_open_constructed_after=sum(nownone.values()),exports=268))
    closed=inherited|{c['current_id'] for c in cs+callbacks};history=load(PREFIX+'historical-ownership-v2.json')['members'];current=load(PREFIX+'current-ownership-v2.json');need(len(history)==704 and [h['canonical_original_id'] for h in history]==[h['canonical_original_id'] for h in oldh],'Historical canonical identity set/order changed')
    for h,old in zip(history,oldh):
        expected=bool(old['proof']) or old['canonical_original_id'] in newh;need(bool(h['proof'])==expected and h['current_ids']==[mapping[k]['symbol_id'] for k in old['current_ids']],'Unsupported historical disposition')
        if old['proof']:need(h['proof']==old['proof'],'Inherited proof rewritten')
        need(all(h[k]==v for k,v in old.items() if k not in ['current_ids','proof','disposition']),'Historical source lineage altered')
    need(Counter(n['symbol_id'] for n in current['members'])==Counter(members.keys()),'Current identity duplicate/missing');need({n['symbol_id'] for n in current['members'] if not n['review_status'].startswith('OPEN')}==closed,'Unsupported current closure')
    for n in current['members']:need(all(n[k]==v for k,v in members[n['symbol_id']].items()),'Current source metadata altered')
    oldsup={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in oldcur['support']};support=current['support'];need(Counter(s['support_id'] for s in support)==Counter(s['support_id'] for s in u['supports']),'Support source declaration missing')
    source_support={s['support_id']:s for s in u['supports']};newmodules={'BESHStatNG.WorksheetFunctions.AgreementMethodUDFs','BESHStatNG.WorksheetFunctions.ClassificationReportUDFs','BESHStatNG.UDFhelpers'}
    for s in support:
        need(all(s[k]==v for k,v in source_support[s['support_id']].items()),'Support source/span/field changed');old=oldsup[s['path'],s['kind'],s['owner'],s['declaration']];expected=old['fully_reconciled']
        if not expected and s['kind']=='ModuleBlock':
            name=re.search(r'\bModule\s+(\w+)',s['declaration'])[1];owner=s['owner'].rstrip('.')+'.'+name;children=[n['symbol_id'] for n in u['members'] if n['owner']==owner]
            if owner in newmodules:
                need(children and all(k in closed for k in children),'Unsupported module child body');need(all(not next(t for t in meta['types'] if t['type']==owner)['fields'] for meta in contexts.values()),'Module mutable field/initializer unresolved');expected=True
        need(s['fully_reconciled']==expected,'False support/container closure')
    def headers(universe,keys):
        return {f['fragment_id'] for s in universe['supports'] if (s['path'],s['kind'],s['owner'],s['declaration']) in keys and s['kind']=='ModuleBlock' for f in universe['fragments'] if f['member_id'] is None and f['path']==s['path'] and f['kind'] in ['ModuleStatement','EndModuleStatement'] and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']}
    keys={(s['path'],s['kind'],s['owner'],s['declaration']) for s in support if s['fully_reconciled']};hc=headers(u,keys);ho=headers(original,keys);oc={h['git_original_id'] for h in history if h['proof']}
    def count(fs,covered):return dict(total=len(fs),branches=sum(f['is_branch'] for f in fs),inline=sum(f['is_inline'] for f in fs),covered=sum(covered(f) for f in fs),branches_covered=sum(f['is_branch'] and covered(f) for f in fs),inline_covered=sum(f['is_inline'] and covered(f) for f in fs))
    fragments=dict(current=count(u['fragments'],lambda f:f['member_id'] in closed or f['fragment_id'] in hc),original=count(original['fragments'],lambda f:f['member_id'] in oc or f['fragment_id'] in ho))
    cf=current['fragments'];need(Counter(f['fragment_id'] for f in cf)==Counter(f['fragment_id'] for f in u['fragments']),'Current branch/inline fragment missing');fmap={f['fragment_id']:f for f in u['fragments']}
    for f in cf:need(all(f[k]==v for k,v in fmap[f['fragment_id']].items()) and (f['review_status']=='COVERED_BY_REVIEWED_BODY_OR_DECLARATION')==(f['member_id'] in closed or f['fragment_id'] in hc),'Current fragment status/source forged')
    of=load(PREFIX+'original-fragment-parent-crosswalk-v2.json')['rows'];need(Counter(f['fragment_id'] for f in of)==Counter(f['fragment_id'] for f in original['fragments']),'Original source fragment missing');ofmap={f['fragment_id']:f for f in original['fragments']}
    for f in of:
        o=ofmap[f['fragment_id']];need((f['parent'],f['is_branch'],f['is_inline'])==(o['member_id'],o['is_branch'],o['is_inline']) and (f['status']=='COVERED_BY_PROVEN_BODY_OR_DECLARATION')==(o['member_id'] in oc or o['fragment_id'] in ho),'Original branch/inline mapping forged')
    individual=load(PREFIX+'new-parent-fragment-crosswalk-v1.json');need(len(individual['rows'])==1252 and not individual['unmatched_branch_inline'],'New original branch/inline correspondence missing');need(Counter(f['fragment_id'] for f in individual['rows'])==Counter(f['fragment_id'] for f in original['fragments'] if f['member_id'] in {hby[k]['git_original_id'] for k in newh}),'New parent fragment identities overlap/drop')
    summary=dict(status='NOT_READY_04A38',historical=dict(total=704,closed=sum(bool(h['proof']) for h in history),OPEN=sum(not h['proof'] for h in history)),current=dict(total=726,closed=len(closed),OPEN=726-len(closed)),support=dict(raw=len(support),distinct=sum(not s['already_in_primary_population'] for s in support),distinct_closed=sum(not s['already_in_primary_population'] and s['fully_reconciled'] for s in support),distinct_OPEN=sum(not s['already_in_primary_population'] and not s['fully_reconciled'] for s in support)),fragments=fragments,W2='IN_PROGRESS',W3='NOT_STARTED',newly_supported_historical=sorted(newh),newly_supported_current=sorted(newcurrent),compiled_retention=retention,source_composition=composition(root,overrides),original_queries=0)
    ledger=load(PREFIX+'ownership-summary-v2.json');need(all(ledger[k]==summary[k] for k in ['status','historical','current','support','fragments','W2','W3']),'Injected ownership/fragment totals');need(summary['historical']['OPEN'] and summary['current']['OPEN'] and summary['support']['distinct_OPEN'],'Unsupported READY promotion')
    queue=load(PREFIX+'remaining-work-queue-v1.json');expected=[r for r in load(OLD+'remaining-work-queue-v1.json')['members'] if r['original_id'] not in newh];need([r['original_id'] for r in queue['members']]==[r['original_id'] for r in expected] and queue['first_members']==queue['members'][:24],'OPEN queue position/order forged');need(Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(set(members)-closed),'Current OPEN complement differs')
    if check_validation:
        from verify_c9_validation_v1 import inspect as validation
        summary['validation']=validation(load,PREFIX,raw)
    return summary
if __name__=='__main__':
    try:print(json.dumps(compute(check_validation='--static' not in sys.argv)))
    except Exception as error:print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))));raise SystemExit(1)
