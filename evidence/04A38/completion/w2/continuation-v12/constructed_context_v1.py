"""Actual reached constructed owners and every implicit initializer/constructor."""
from common import *
from proof_kernel_v4 import definitions,semantic
diag=json.loads((HERE/'cohort-diagnostic-v1.json').read_bytes());rows=[];changes=[]
expected_native={('BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs',name) for name in ['MCA_SUMMARY','DA_SUMMARY']}|{('BESHStatNG.UdfDataImport',name) for name in ['TryGetCategoricalMatrix','TryGetStringVector','TryGetDoubleVector']}
expected_added={('BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38',name) for name in ['BuildMcaSummary','BuildDiscriminantSummary']}|{('BESHStatNG.ExcelCommon.UdfModelImportPreparation38',name) for name in ['CompleteCategoricalMatrixPrepared','TryGetStringVectorPrepared','TryGetDoubleVectorPrepared']}
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json')).read_bytes())
    prior=json.loads((PREVIOUS/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    original=json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes())
    defs=definitions(meta);old=definitions(prior);types={t['type']:t for t in meta['types'] if t['type']};pt={t['type']:t for t in prior['types'] if t['type']};ot={t['type']:t for t in original['types'] if t['type']}
    changed=[key for key in old if key in defs and semantic(old[key])!=semantic(defs[key])];added=sorted(set(defs)-set(old));lost=sorted(set(old)-set(defs))
    assert {(defs[k]['declaring_type'],defs[k]['member_name']) for k in changed}==expected_native
    assert {(defs[k]['declaring_type'],defs[k]['member_name']) for k in added}==expected_added and not lost
    owners=sorted({defs[key]['declaring_type'] for key in diag['graphs'][config]['methods']});contexts=[]
    for owner in owners:
        assert owner in types and owner in pt and types[owner]==pt[owner],('Changed/unmeasured constructed owner',owner)
        constructors=[m for m in meta['methods'] if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']]
        assert all(m['method'] in old and semantic(m)==semantic(old[m['method']]) for m in constructors)
        contexts.append(dict(owner=owner,type=types[owner],prior_type=pt[owner],original_type=ot.get(owner),
            reached_methods=[key for key in diag['graphs'][config]['methods'] if defs[key]['declaring_type']==owner],constructors_and_initializers=constructors,
            qualification='Actual constructed owner and implicit CLR initialization, not inferred from a source Call or unconstructed generic method. Retained source/field semantics checked against C11; original records retained where present.'))
    rows.append(dict(configuration=config,contexts=contexts,reached_methods=len(diag['graphs'][config]['methods']),types=len(contexts),metadata=pin(HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json'))))
    changes.append(dict(configuration=config,changed=[dict(method=key,original=old[key],current=defs[key]) for key in changed],added=[defs[key] for key in added],lost=lost,
        previous_metadata=pin(PREVIOUS/('metadata-all/packet-v2-context-v5/current-'+config+'.json')),current_metadata=pin(HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json'))))
save('constructed-owner-proofs-v1.json',dict(configurations=rows,source_universes=[pin(HERE/name) for name in ['current-universe-packet-v1.json','dependency-full-common-v1.json','dependency-full-core-v1.json']],original_queries=0))
save('compiled-method-deltas-v1.json',dict(configurations=changes,expected_native=sorted(expected_native),expected_added=sorted(expected_added),original_queries=0))
summary=json.loads((HERE/'ownership-summary-v2.json').read_bytes())
save('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',
    mandatory_blockers=dict(historical_OPEN=summary['historical']['OPEN'],current_OPEN=summary['current']['OPEN'],distinct_support_OPEN=summary['support']['distinct_OPEN']),
    owner_acceptance='NOT_ACCEPTED_NOT_MERGED',native='NOT_RUN',scientific_oracle='NOT_RUN',original_queries=0))
print(json.dumps(dict(contexts=[dict(configuration=r['configuration'],methods=r['reached_methods'],owners=r['types']) for r in rows],changed_existing_per_configuration=5,added_per_configuration=5,lost=0)))
