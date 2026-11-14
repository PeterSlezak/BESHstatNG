"""Recompute exact closure sets and bind fresh compiled dependencies in both modes."""
from common import *
from collections import Counter

lock()
prior=W2/'continuation-v17'
accepted=load(ACCEPTED/'canonical-identity-status-v1.json')['entries']
baseline=load(prior/'canonical-identity-status-v4.json')['entries']
key=lambda r:(r['population'],r['identity'])
a={key(r):r for r in accepted};b={key(r):r for r in baseline}
assert len(a)==len(accepted)==len(b)==1733
missing=sorted(b.keys()-a.keys());extra=sorted(a.keys()-b.keys())
opened=[r for r in accepted if not r['supported'] or r['transition']=='OPEN']
assert not missing and not extra and not opened
assert Counter(r['population'] for r in accepted)==dict(historical=704,current=726,support=303)
original=load(prior/'original-fragment-parent-crosswalk-v4.json')['rows']
current=load(prior/'current-ownership-v4.json')
transition=load(prior/'original-current-fragment-transition-matrix-v2.json')
assert len(original)==20683 and all(r['covered'] for r in original)
assert len(current['members'])==726 and len(current['support'])==325 and sum(r['distinct'] for r in current['support'])==303
assert len(current['fragments'])==12659 and all(not r['review_status'].startswith('OPEN') for r in current['fragments'])
assert all(not r['review_status'].startswith('OPEN') for r in current['members'])
assert all(r['fully_reconciled'] for r in current['support'])
diffs=load(HERE/'abi-cil/cross-configuration-v1.json')
attributes=load(HERE/'abi-cil/signature-attribute-differences-v1.json')['rows']
allowed={'System.Diagnostics.DebuggerStepThroughAttribute','System.Diagnostics.DebuggerNonUserCodeAttribute','System.Diagnostics.DebuggerHiddenAttribute'}
assert all(r['changed_keys']==['attributes'] and not r['member_flags_changed'] and not r['added_attributes'] and set(r['removed_attributes'])<=allowed for r in attributes)
assert len(diffs['Debug_only'])==2 and not diffs['Release_only']
assert all('VB$StateMachine_' in r['declaring_type'] and r['member_name']=='.ctor' for r in diffs['Debug_only'])
debug=load(PRIVATE/'Debug/metadata/current.json');release=load(PRIVATE/'Release/metadata/current.json')
typekey=lambda r:(r['assembly'],r['type'])
dt={typekey(r):r for r in debug['types']};rt={typekey(r):r for r in release['types']}
type_diffs=[dict(Debug=dt[k],Release=rt[k]) for k in dt.keys()&rt.keys() if dt[k]!=rt[k]]
assert dt.keys()==rt.keys()
assert all('VB$StateMachine_' in r['Debug']['type'] for r in type_diffs),'UNEXPLAINED_TYPE_ABI_DIFFERENCE'
edges=[]
def normalized_calls(rows):
    return [('value-type-constructor' if (operand.startswith('METHOD:System.ValueTuple`') or operand.startswith('METHOD:System.Nullable`')) and '..ctor(' in operand else op,operand)
            for op,operand in rows]
for r in diffs['ordered_call_delegate_dependency_differences']:
    if normalized_calls(r['Debug'])==normalized_calls(r['Release']):classification='IDENTICAL_ORDERED_TARGETS_VALUE_TYPE_CTOR_CALL_VS_NEWOBJ'
    elif 'VB$StateMachine_' in r['method'] or any(n in r['method'] for n in ['DiagnosticEmitter @','DiagnosticRunContext @']):classification='COMPILER_ASYNC_CLASS_VS_VALUE_TYPE_STATE_MACHINE'
    elif 'DefaultErrorReporter @' in r['method'] and '.LogAndThrow(' in r['method']:
        sealed=load(W2/'continuation-v12/metadata-all/packet-v1-context-v5/original-Release.json')
        oldmethod=next(x for x in sealed['methods'] if x['method']==r['method'])
        newmethod=next(x for x in release['methods'] if x['method']==r['method'])
        # The older V5 reader recorded null signatures for this dependency.
        # Preserve every measured old field and independently require fresh
        # Debug/Release signatures to differ only in debugger attributes above.
        assert oldmethod.get('signature') is None
        assert {k:v for k,v in oldmethod.items() if k not in ['raw_IL_sha256','signature']}=={k:v for k,v in newmethod.items() if k not in ['raw_IL_sha256','signature']}
        classification='DEBUG_PRINT_ARGUMENT_EVALUATION_OMITTED_IN_RELEASE;EXACT_SEALED_ORIGINAL_RELEASE_METHOD'
    else:classification='UNEXPLAINED'
    edges.append(dict(r,classification=classification))
assert all(r['classification']!='UNEXPLAINED' for r in edges),'UNEXPLAINED_DEPENDENCY_EDGE_DIFFERENCE'
assert not diffs['exception_metadata_differences']
assert all(normalized_calls(r['Debug'])==normalized_calls(r['Release']) for r in edges if '@ BESHStatNG,' in r['method']),'WINDOWS_BOUNDARY_TARGET_DIFFERENCE'
sealed_release=prior.parent/'continuation-v12/metadata-all/packet-v1-context-v5/original-Release.json'
old=load(sealed_release)
oldmethods={r['method']:r for r in old['methods']}
rm={r['method']:r for r in release['methods']}
unchanged=[];changed=[]
def semantic(r):return {k:v for k,v in r.items() if k!='raw_IL_sha256'}
for k in sorted(oldmethods.keys()&rm.keys()):
    (unchanged if semantic(oldmethods[k])==semantic(rm[k]) else changed).append(k)
proofs=load(ACCEPTED/'whole-body-dependency-fragment-reconciliation-v1.json')['proofs']
for r in proofs:assert pin(ROOT/r['path'])==r
save('abi-cil/complete-closure-v4.json',dict(status='SOURCE_BOUND_CLOSURE_REPLAY_AND_FRESH_BOTH_CONFIGURATION_DEPENDENCIES_VERIFIED',
    entries=accepted,missing=missing,extra=extra,OPEN=opened,
    populations=dict(Counter(r['population'] for r in accepted)),
    fragment_counts=dict(original=len(original),current=len(current['fragments']),predecessor=13695),
    transition_schema=list(transition),transition=pin(prior/'original-current-fragment-transition-matrix-v2.json'),
    replay=pin(HERE/'preservation/reader-complete-v17-proof-v1.json'),
    preserved_proofs=proofs,physical_Windows=723,moved_Common_lambdas=3,
    configuration_dispositions=dict(signature_debugger_attribute_only=len(attributes),generated_async_constructor_differences=2,
        generated_async_type_differences=type_diffs,call_differences=edges,exception_differences=[],
        Windows_ordered_call_delegate_targets_identical_after_value_type_ctor_opcode_normalization=True),
    sealed_original_Release_metadata=pin(sealed_release),Release_original_common_methods=len(oldmethods.keys()&rm.keys()),
    Release_original_exact_semantic_methods=unchanged,Release_original_migrated_or_other_different_methods=changed,
    Release_baseline_disposition='Original Release metadata is historical pre-migration authority. Source-body transition certificates and current Debug/Release ordered dependency equality bind migrated bodies; original IL equality is not expected for relocated implementations.',
    source_freeze=pin(ACCEPTED/'production-test-freeze-v1.json'),original_queries=0,
    limitations='Source composition/fragment contracts and passive CIL inspection reproduce software proof; no independent scientific oracle or native session execution.'))
print('Exact identity sets704/726/303; original20683/current12659/predecessor13695 fragment reader replay; Windows edges identical; optimization differences classified',flush=True)
