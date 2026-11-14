"""Rebuild all ownership universes, OPEN complements and individual fragment matches."""
from collections import Counter
from proof_kernel_v4 import need
from verify_c12_composition_v1 import tokens,PHASES,unwrap

def inspect(load,prefix,original,u,old,current,history,closed,newhist,contexts):
    support=current['support'];olds={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in old['support']}
    need(Counter(s['support_id'] for s in support)==Counter(s['support_id'] for s in u['supports']),'Support declaration omitted or duplicated')
    sources={s['support_id']:s for s in u['supports']}
    newclasses={'Private Class MultipleCorrespondenceHandle','Private Class DiscriminantHandle'}
    for s in support:
        src=sources[s['support_id']];need(all(s[k]==v for k,v in src.items()),'Support source declaration forged')
        before=olds[s['path'],s['kind'],s['owner'],s['declaration']]
        expected=before['fully_reconciled'] or s['kind']=='ClassBlock' and s['declaration'] in newclasses and s['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs'
        need(s['fully_reconciled']==expected,'Unsupported declaration closure')
        if s['declaration']=='Public Module MultivariateAnalysisUDFs':need(not s['fully_reconciled'],'Incomplete native module promoted')
    classes={'Private Class PcaHandle','Private Class FactorAnalysisHandle','Private Class KMeansHandle','Private Class HierarchicalHandle','Private Class CorrespondenceHandle'}|newclasses
    owners={'BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs.'+s.split()[-1] for s in classes}
    keys={(s['path'],s['kind'],s['owner'],s['declaration']) for s in support if s['fully_reconciled']}
    def headers(universe):
        found=set()
        for s in universe['supports']:
            if (s['path'],s['kind'],s['owner'],s['declaration']) not in keys:continue
            if s['kind']=='ModuleBlock':kinds=['ModuleStatement','EndModuleStatement']
            elif s['kind']=='ClassBlock' and s['declaration'] in classes:kinds=['ClassStatement','EndClassStatement']
            elif s['kind']=='PropertyStatement' and s['owner'] in owners:kinds=['PropertyStatement']
            else:continue
            found.update(f['fragment_id'] for f in universe['fragments'] if f['member_id'] is None and f['path']==s['path'] and f['kind'] in kinds and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length'])
        return found
    hc=headers(u);ho=headers(original);oc={h['git_original_id'] for h in history if h['proof']}
    def counts(rows,predicate):return dict(total=len(rows),branches=sum(r['is_branch'] for r in rows),inline=sum(r['is_inline'] for r in rows),covered=sum(predicate(r) for r in rows),branches_covered=sum(r['is_branch'] and predicate(r) for r in rows),inline_covered=sum(r['is_inline'] and predicate(r) for r in rows))
    summary=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=dict(total=704,closed=sum(bool(h['proof']) for h in history),OPEN=sum(not h['proof'] for h in history)),
        current=dict(total=726,closed=len(closed),OPEN=726-len(closed)),support=dict(raw=len(support),distinct=sum(not s['already_in_primary_population'] for s in support),distinct_closed=sum(not s['already_in_primary_population'] and s['fully_reconciled'] for s in support),distinct_OPEN=sum(not s['already_in_primary_population'] and not s['fully_reconciled'] for s in support)),
        fragments=dict(current=counts(u['fragments'],lambda f:f['member_id'] in closed or f['fragment_id'] in hc),original=counts(original['fragments'],lambda f:f['member_id'] in oc or f['fragment_id'] in ho)),original_queries=0)
    need(summary==load(prefix+'ownership-summary-v2.json'),'Injected census/fragment totals')
    need(summary['historical']['OPEN'] and summary['current']['OPEN'] and summary['support']['distinct_OPEN'],'Unsupported READY closure')
    cf=current['fragments'];need(Counter(f['fragment_id'] for f in cf)==Counter(f['fragment_id'] for f in u['fragments']),'Current fragment missing')
    fm={f['fragment_id']:f for f in u['fragments']}
    for f in cf:need(all(f[k]==v for k,v in fm[f['fragment_id']].items()) and (f['review_status']=='COVERED_BY_REVIEWED_BODY_OR_DECLARATION')==(f['member_id'] in closed or f['fragment_id'] in hc),'Wrong current fragment/arm status')
    of=load(prefix+'original-fragment-parent-crosswalk-v2.json')['rows'];need(Counter(f['fragment_id'] for f in of)==Counter(f['fragment_id'] for f in original['fragments']),'Original fragment missing')
    om={f['fragment_id']:f for f in original['fragments']}
    for f in of:
        o=om[f['fragment_id']];need((f['parent'],f['is_branch'],f['is_inline'])==(o['member_id'],o['is_branch'],o['is_inline']) and (f['status']=='COVERED_BY_PROVEN_BODY_OR_DECLARATION')==(o['member_id'] in oc or o['fragment_id'] in ho),'Original arm status forged')
    cross=load(prefix+'new-parent-fragment-crosswalk-v1.json');rows=cross['rows'];new={c['original_record']['symbol_id']:c for c in newhist.values()}
    expected=[f for f in original['fragments'] if f['member_id'] in new]
    need(Counter(r['fragment_id'] for r in rows)==Counter(f['fragment_id'] for f in expected) and not cross['unmatched_branch_inline'],'New original fragment population incomplete')
    portable=load(prefix+'portable-api-contracts-v1.json')['contracts']
    for r in rows:
        f=om[r['fragment_id']];c=new[f['member_id']];need(r['original_source_record']==f and r['original_parent']==c['original_id'] and r['current_parent']==c['current_id'],'Synthetic or misowned fragment')
        pool=[v for v in u['fragments'] if v['member_id']==c['current_id']]
        if not c['current_record']['parent_member']:
            graph=set(c['compiled'][0]['transitive']['methods'])
            for p in portable:
                if p['compiled'][0]['method']['method'] in graph:pool.extend(p['fragments'])
        bound=f['code'];name=c['current_record']['name']
        if name in PHASES:
            for a,b in PHASES[name]['bindings'].items():bound=bound.replace(a,b)
        matches=[v['fragment_id'] for v in pool if v['kind']==f['kind'] and any(tokens(v['code'])==tokens(x) or tokens(unwrap(v['code']))==tokens(x) for x in [f['code'],bound])]
        need(r['matched_current_fragments']==matches and r['bound_source']==bound,'Fragment match fabricated')
        need(matches or not (f['is_branch'] or f['is_inline']),'Unmatched actual branch/inline arm')
    return summary
