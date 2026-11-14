"""Source prediction and separate current-only probe; no historical oracle."""
from common import *
from source_views import body
from proof_kernel_v6 import definitions
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
node=next(n for n in original['members'] if n['name']=='GuessPositiveClass' and n['parent_member'] is None)
current=next(n for n in u['members'] if n['name']=='GuessPositiveClass' and n['parent_member'] is None)
common=next(n for n in json.loads((HERE/'dependency-full-common-v1.json').read_bytes())['members'] if n['name']=='GuessPositiveClass' and n['parent_member'] is None)
text=body((ROOT/snapshots[node['path']]['path']).read_bytes(),node)[0]
assert text.index('positiveLabel = Nothing')<text.index('CellToTrimmedText(explicitPositive)')
native=body((ROOT/current['path']).read_bytes(),current)[0];portable=body((ROOT/common['path']).read_bytes(),common)[0]
assert native.index('positiveLabel = Nothing')<native.index('CellToTrimmedText(explicitPositive)') and 'positiveLabel = Nothing' in portable
probes=[]
for v in [1,2]:
    folder=PRIVATE/('current-byref-alias-probe-v'+str(v));probes.append(dict(version=v,files=[pin(p) for p in sorted(folder.iterdir()) if p.is_file()]))
save('open-byref-alias-defect-v1.json',dict(status='OPEN_NOT_REPAIRED_NOT_WAIVED',
    blocked=[dict(original_id=h['canonical_original_id'],name=h['name'],current_ids=h['current_ids']) for h in json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'] if h['name'] in ['GuessPositiveClass','TryGetRocInput','ROC_POINTS','ROC_STATS']],
    original_record=node,original_source=snapshots[node['path']],original_whole_body=text,current_record=current,current_whole_body=native,portable_record=common,portable_whole_body=portable,
    condition='Custom explicit-positive cell ToString mutates the ByRef output alias while returning an empty string; prepared labels red/green/blue cannot infer a positive label.',
    original_source_prediction=dict(result=False,positiveLabel='converted',executed=False),current_only_measurement=dict(result=False,positiveLabel=None,conversionCalls=1),probes=probes,
    cause='Original resets once before conversion. The existing current Common GuessPositiveClass resets again after conversion, erasing the alias mutation. This adapter preceded C13 and was not previously closed.',
    action='Keep all four transitive parents and PlotData module OPEN. Preserve ordinary current compatibility and independently proved callbacks. No production/test amendment after freeze.',
    supplemental_current_managed_probes=2,unit_test_identities_added=0,original_queries=0,scientific_changes=0))
print('Four ROC parents remain OPEN; source prediction distinguished from two actual current-only probes')
