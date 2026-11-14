"""Resolve original callbacks by parent plus body, preserving the rejected v1 attempt."""
from common import *
from source_views import body

save('packet-preparation-first-failure-v1.json', dict(
    command=[sys.executable,'-B',str(HERE/'prepare_packet_v1.py')],exit_code=1,
    exception='AssertionError: assert len(h[\'current_ids\']) == 1 and not h[\'proof\']',
    cause='OPEN historical lambda candidate lists intentionally contain nine signature matches. No source was changed. Resume and tool copies had completed; no packet plan or targets had been written.',
    correction='Version2 reconciles original parent through its own historical identity and requires the callback exact body/token hashes within that one current parent.',
    source=pin(HERE/'prepare_packet_v1.py'),production_edits=0))
entry=json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())
for row in entry['files']:
    assert pin(ROOT/row['live']['path']) == row['live']
assert git('rev-parse','HEAD').decode().strip()==BASE
assert not git('diff','--cached','--name-only')
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())
universe=json.loads((PREVIOUS/'current-universe-packet-v2.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
om={r['symbol_id']:r for r in original['members']};cm={r['symbol_id']:r for r in universe['members']}
hm={r['canonical_original_id']:r for r in history};bygit={r['git_original_id']:r for r in history}
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/
    'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
targets=[];callback_resolutions=[]
for queued in queue['first_members']:
    h=hm[queued['original_id']];assert not h['proof'];o=om[h['git_original_id']]
    if o['parent_member'] is not None:
        parent=bygit[o['parent_member']];assert len(parent['current_ids'])==1
        candidates=[cm[k] for k in h['current_ids'] if cm[k]['parent_member']==parent['current_ids'][0]
            and cm[k]['body_sha256']==o['body_sha256'] and cm[k]['body_token_sha256']==o['body_token_sha256']]
        assert len(candidates)==1
        n=candidates[0]
        callback_resolutions.append(dict(original_id=queued['original_id'],original_parent=o['parent_member'],
            current_parent=parent['current_ids'][0],current_id=n['symbol_id'],
            rejected_other_candidates=[k for k in h['current_ids'] if k!=n['symbol_id']]))
    else:
        assert len(h['current_ids'])==1;n=cm[h['current_ids'][0]]
    text,view=body((ROOT/n['path']).read_bytes(),n)
    old,oldview=body((ROOT/snapshots[o['path']]['path']).read_bytes(),o)
    targets.append(dict(original_id=queued['original_id'],name=n['name'],current_record=n,original_record=o,
        current_source=pin(ROOT/n['path']),original_source=snapshots[o['path']],
        current_whole_body=text,original_whole_body=old,current_view=view,original_view=oldview))
assert len(targets)==24 and len({r['current_record']['symbol_id'] for r in targets})==24
save('target-entry-bodies-v1.json',dict(rows=targets,callback_resolutions=callback_resolutions,original_queries=0))
paths=['BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb',
 'BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb',
 'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
new='BESHStat.Excel.Common.Tests/src/UdfContinuation11Tests.vb';assert not (ROOT/new).exists()
by_path={r['live']['path']:r for r in entry['files']};PRIVATE.mkdir(exist_ok=False)
save('implementation-plan-v1.json',dict(status='AUTHENTICATED_BEFORE_C11_PRODUCTION_EDITS',
    source_paths=paths+[new],source_before=[by_path[p] for p in paths],
    new_sources=[dict(path=new,entry_state='ABSENT')],first_members=queue['first_members'],
    entry=pin(HERE/'entry-outcome-v1.json'),scope='Five pure report layout phases in the exact FA/KMeans/hierarchical/CA cohort. FIT models, caches, Guid, import, coercion, exception/logging and materialization stay Windows-owned. All necessary helper/callback/declaration closure requires full actual graph proof. Original numerics and known defects remain.',original_queries=0))
print('C11 canonical initial24: two distinct callbacks resolved by parent/body; source plan ready')
