"""All actually reached owners and implicit constructor/initializer contexts."""
from common import *
from proof_kernel_v3 import definitions,semantic,need
diag=json.loads((HERE/'cohort-diagnostic-v2.json').read_bytes());rows=[]
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    prior=json.loads((PREVIOUS/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    original=json.loads((HERE/('metadata-all/packet-v2-context-v5/original-'+config+'.json')).read_bytes())
    defs=definitions(meta);before=definitions(prior);types={t['type']:t for t in meta['types'] if t['type']}
    pt={t['type']:t for t in prior['types'] if t['type']};ot={t['type']:t for t in original['types'] if t['type']}
    owners=sorted({defs[k]['declaring_type'] for k in diag['graphs'][config]['methods']})
    contexts=[]
    for owner in owners:
        need(owner in types and owner in pt,'Unmeasured new constructed owner')
        need(types[owner]==pt[owner],'Existing field/type/initializer declaration changed')
        ctors=[m for m in meta['methods'] if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']]
        need(all(m['method'] in before and semantic(m)==semantic(before[m['method']]) for m in ctors),'Constructor/static initializer changed')
        contexts.append(dict(owner=owner,type=types[owner],prior_type=pt[owner],original_type=ot.get(owner),
            reached_methods=[k for k in diag['graphs'][config]['methods'] if defs[k]['declaring_type']==owner],
            constructors_and_initializers=ctors,
            qualification='Actual measured constructed owner. Implicit CLR type-initialization context is retained separately from explicit CIL call reachability. Original type present when part of the original owner universe; inherited portable additions retain sealed predecessor authority.'))
    rows.append(dict(configuration=config,contexts=contexts,reached_methods=len(diag['graphs'][config]['methods']),
        types=len(contexts),metadata=pin(HERE/('metadata-all/packet-v2-context-v5/current-'+config+'.json'))))
save('constructed-owner-proofs-v1.json',dict(configurations=rows,source_universes=[pin(HERE/name) for name in
    ['current-universe-packet-v2.json','dependency-full-common-v2.json','dependency-full-core-v2.json']],original_queries=0))
summary=json.loads((HERE/'ownership-summary-v2.json').read_bytes())
save('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',
    C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',
    mandatory_blockers=dict(historical_OPEN=summary['historical']['OPEN'],current_OPEN=summary['current']['OPEN'],
        distinct_support_OPEN=summary['support']['distinct_OPEN']),owner_acceptance='NOT_ACCEPTED_NOT_MERGED',
    native='NOT_RUN',scientific_oracle='NOT_RUN',original_queries=0))
print([(r['configuration'],r['reached_methods'],r['types']) for r in rows])
