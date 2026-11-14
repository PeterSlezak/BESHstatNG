"""Reconcile actual tests by identity; inherited nonpasses never become acceptance."""
from collections import Counter
import hashlib
from proof_kernel_v3 import need

OLD='evidence/04A38/completion/w2/continuation-v10/'
PRIVACY={
 'test_04a13_survival_workflow.Survival13SourceContracts.test_complete_existing_publication_set_has_no_unresolved_private_values',
 'test_04a14_mixed_workflow.Mixed14SourceContracts.test_entire_proposed_publication_set_privacy'}

def inspect(load,raw,prefix):
    validation=load(prefix+'validation-v1.json');managed=load(prefix+'managed-Debug-identities-v1.json')
    old=load(OLD+'managed-Debug-identities-v1.json')
    key=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
    rows={key(r):r for r in managed['tests']};prior={key(r):r for r in old['tests']}
    need(len(rows)==len(managed['tests'])==20817 and len(prior)==20788,'Wrong/duplicate managed identity count')
    need(set(prior)<=set(rows) and all(rows[k]['outcome']==r['outcome'] for k,r in prior.items()),'Lost/changed inherited managed identity')
    additions=set(rows)-set(prior)
    need(len(additions)==29 and all(rows[k]['outcome']=='Passed' for k in additions),'New managed nonpass or substituted test')
    outcomes=Counter(r['outcome'] for r in rows.values())
    need(outcomes==Counter(Passed=20803,Failed=14) and validation['Debug']['outcomes']==dict(outcomes),'Wrong managed first/effective outcomes')
    nonpass=lambda r:(*key(r),r['outcome'],r['message_sha256'])
    need(Counter(map(nonpass,managed['nonpasses']))==Counter(map(nonpass,old['nonpasses'])),'Managed nonpass/message identity changed')
    execution=load(prefix+'managed-full-execution-v1.json')
    need(execution['first_full_outcomes']==dict(outcomes) and execution['source_unchanged'],'Managed result stitched or source changed')
    managed_report=load(prefix+'managed-validation-v1.json')
    need(managed_report['full_Debug_runs']==1 and managed_report['Debug']['new_passes']==29,'Unreported repeat managed campaign')
    need(validation['Release_full']==validation['Release_selected']==managed_report['Release_full']==managed_report['Release_selected']=='NOT_RUN_OWNER_PREFERENCE','Release test preference violated')
    for r in load(prefix+'production-test-freeze-v1.json')['sources']:
        data=raw(r['path']);need((len(data),hashlib.sha256(data).hexdigest())==(r['bytes'],r['sha256']),'Production/test source changed after validation')
    inventory=load(prefix+'python-discovery-inventory-v1.json')['entries']
    python=load(prefix+'python-effective-identities-v1.json');entries=python['entries']
    need(len(inventory)==len(entries)==956 and Counter(r['id'] for r in inventory)==Counter(r['id'] for r in entries),'Python missing/duplicated parent multiset')
    need(all(r['attempt']==0 and r['occurrence']==0 for r in entries) and not python['missing'] and not python['duplicated'] and not python['skipped'],'Python substituted/repeated/skipped identity')
    parents=Counter(r['outcome'] for r in entries)
    events=load(prefix+'python-first-failures-v2.json')['records'];eventcounts=Counter(r['outcome'] for r in events)
    need(python['method_outcomes']==dict(parents) and python['nonpass_events']==dict(eventcounts),'Python parent/event units conflated')
    need(validation['Python']['methods']==dict(parents) and validation['Python']['nonpass_events']==dict(eventcounts) and validation['Python']['executed']==956,'Python validation population mismatch')
    need({r['id'] for r in entries if r['outcome']=='ERROR'}>=PRIVACY,'Immutable malformed-aggregation privacy errors waived/relabelled')
    need(all(any(r['identity']==name and r['outcome']=='ERROR' for r in events) for name in PRIVACY),'Privacy raw event identity missing')
    oldevents=load(OLD+'python-first-failures-v2.json')['records']
    need(all(any(r['identity']==name and r['outcome']=='ERROR' for r in oldevents) for name in PRIVACY),'Prior privacy ERROR history missing')
    accounting=load(prefix+'python-worker-accounting-v1.json')
    need(accounting['raw_attempts']==956 and accounting['corrective_repeats']==0 and accounting['workers_at_peak']<=6,'Wrong Python worker/attempt reconciliation')
    need(sum(e['methods'] for e in accounting['executions'])==956 and accounting['effective_method_outcomes']==dict(parents) and accounting['effective_nonpass_events']==dict(eventcounts),'Worker counts do not reconcile')
    node=load(prefix+'node-identities-v1.json');oldnode=load(OLD+'node-identities-v1.json')
    need({r['suite']:Counter(r['identities']) for r in node['suites']}=={r['suite']:Counter(r['identities']) for r in oldnode['suites']},'Node identity population changed')
    need(sum(len(r['identities']) for r in node['suites'])==916 and sum(r['passed'] for r in validation['Node'])==916 and all(r['failed']==0 and r['command']['exit_code']==0 for r in validation['Node']),'Node failed/duplicate/omitted identity')
    need(set(validation['strict_profile_MkDocs'])=={'strict','profile','docs'},'Auxiliary exit omitted')
    return dict(Debug=dict(outcomes),new_managed_PASS=29,inherited_managed_FAIL=14,
        Python_parents=dict(parents),Python_events=dict(eventcounts),Python_exact_once=956,
        privacy_errors='RETAINED_NOT_REPAIRED_OR_WAIVED',Node_PASS=916,
        strict_profile_MkDocs=validation['strict_profile_MkDocs'],Release_unit='NOT_RUN_OWNER_PREFERENCE')
