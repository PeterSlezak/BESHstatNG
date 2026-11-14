"""All measured reached owners, complete ctor/cctor CIL and explicit new delegate ABI."""
from common import *
from proof_kernel_v5 import definitions,semantic
diag=json.loads((HERE/'cohort-diagnostic-v1.json').read_bytes());configs=[];deltas=[]
expected_native={('BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs',n) for n in ['DA_COVARIANCE','FindWrappedDiscriminantTable']}|{('BESHStatNG.WorksheetFunctions.PlotDataUDFs',n) for n in ['HIST_BINS','HIST_NORMAL','BuildKaplanMeierStepTable','StackHomogeneousTables']}|{('BESHStatNG.UdfDataImport',n) for n in ['TryGetRocInput','TryGetSurvivalRecords']}
newowners={'BESHStatNG.ExcelCommon.UdfPlotDataPreparation38','BESHStatNG.ExcelCommon.UdfPlotFiniteDoubleReader38','BESHStatNG.ExcelCommon.UdfPlotStatusReader38'}
expected_apis={('BESHStatNG.ExcelCommon.MultivariateAnalysisUDFsReports38',n) for n in ['FindWrappedDiscriminantTablePrepared','FindDiscriminantGroupPrepared']}|{('BESHStatNG.ExcelCommon.PlotDataUDFsReports38',n) for n in ['BuildHistogramPartsPrepared','BuildHistogramNormalPartsPrepared','BuildKaplanMeierStepTablePrepared','StackHomogeneousTablesPrepared']}|{('BESHStatNG.ExcelCommon.UdfPlotDataPreparation38',n) for n in ['CompleteRocInputPrepared','CompleteSurvivalRecordsPrepared']}
for config in ['Debug','Release']:
    path=HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json');meta=json.loads(path.read_bytes())
    priorpath=PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+config+'.json');prior=json.loads(priorpath.read_bytes())
    original=json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes())
    defs=definitions(meta);old=definitions(prior);types={t['type']:t for t in meta['types'] if t['type']};pt={t['type']:t for t in prior['types'] if t['type']};ot={t['type']:t for t in original['types'] if t['type']}
    changed=[k for k in old if k in defs and semantic(old[k])!=semantic(defs[k])];added=sorted(set(defs)-set(old));lost=sorted(set(old)-set(defs))
    assert {(defs[k]['declaring_type'],defs[k]['member_name']) for k in changed}==expected_native and not lost
    apis=[defs[k] for k in added if (defs[k]['declaring_type'],defs[k]['member_name']) in expected_apis]
    delegates=[defs[k] for k in added if defs[k]['declaring_type'] in newowners-{'BESHStatNG.ExcelCommon.UdfPlotDataPreparation38'}]
    ctors=[defs[k] for k in added if defs[k]['declaring_type']=='BESHStatNG.ExcelCommon.UdfPlotDataPreparation38' and defs[k]['member_name']=='.ctor']
    assert len(added)==17 and len(apis)==8 and len(delegates)==8 and len(ctors)==1
    assert all(m['implementation_flags']=='CodeTypeMask' and not m['instructions'] and not m['exceptions'] for m in delegates)
    for owner,suffix in [('BESHStatNG.ExcelCommon.UdfPlotFiniteDoubleReader38','System.Double&'),('BESHStatNG.ExcelCommon.UdfPlotStatusReader38','System.Int32&')]:
        invoke=next(m for m in delegates if m['declaring_type']==owner and m['member_name']=='Invoke')
        assert invoke['parameter_types']==['System.Object',suffix]
        returns=json.loads((HERE/('return-metadata-'+config+'-v1.json')).read_bytes())['methods']
        result=next(r for r in returns if (r['declaring_type'],r['member_name'])==(owner,'Invoke'));assert result['return_type']=='System.Boolean'
    contexts=[]
    for owner in sorted({defs[k]['declaring_type'] for k in diag['graphs'][config]['methods']}):
        assert owner in types
        constructors=[m for m in meta['methods'] if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']]
        if owner in pt:
            assert types[owner]==pt[owner] and all(m['method'] in old and semantic(m)==semantic(old[m['method']]) for m in constructors)
            authority='UNCHANGED_ACTUAL_C12_CONSTRUCTED_CONTEXT'
        else:
            assert owner in newowners and not types[owner]['fields']
            authority='NEW_EXPLICIT_TYPED_PORTABLE_SOURCE_AND_ACTUAL_CLR_CONTEXT'
        contexts.append(dict(owner=owner,type=types[owner],prior_type=pt.get(owner),original_type=ot.get(owner),constructors_and_initializers=constructors,
            reached_methods=[k for k in diag['graphs'][config]['methods'] if defs[k]['declaring_type']==owner],authority=authority))
    configs.append(dict(configuration=config,contexts=contexts,reached_methods=len(diag['graphs'][config]['methods']),types=len(contexts),metadata=pin(path),
        delegate_runtime_boundary='The exact recorded implementation_flags string CodeTypeMask denotes the CLR Runtime enum value alias3. Eight delegate .ctor/Invoke/BeginInvoke/EndInvoke methods have no CIL body by CLR design; their actual signatures, ByRef and Boolean return metadata are measured explicitly. They are not invented managed IL.'))
    deltas.append(dict(configuration=config,changed=[dict(method=k,original=old[k],current=defs[k]) for k in changed],added=[defs[k] for k in added],lost=lost,
        new_managed_apis=8,new_private_constructor=1,new_CLR_delegate_methods=8,previous_metadata=pin(priorpath),current_metadata=pin(path)))
save('constructed-owner-proofs-v1.json',dict(configurations=configs,original_queries=0))
save('compiled-method-deltas-v1.json',dict(configurations=deltas,expected_native=sorted(expected_native),expected_apis=sorted(expected_apis),original_queries=0))
print(json.dumps(dict(reached=[(r['configuration'],r['reached_methods'],r['types']) for r in configs],changed_native=8,new_managed_apis=8,new_private_constructor=1,new_CLR_delegate_methods=8,lost=0)))
