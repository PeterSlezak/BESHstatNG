"""Reauthenticate interruption boundary; plan exact initial packet before source edits."""
from common import *

assert json.loads((HERE/'entry-outcome-v1.json').read_bytes())['status'] == 'AUTHENTICATED'
assert git('branch', '--show-current').decode().strip() == 'codex/04A-38'
assert all(git(*args).decode().strip() == BASE for args in [
    ('rev-parse', 'HEAD'), ('rev-parse', 'main'), ('merge-base', 'HEAD', 'main')])
assert not git('diff', '--cached', '--name-only')
assert pin(ROOT/'.git/index')['sha256'] == json.loads((HERE/'git-entry-v1.json').read_bytes())['index_file']['sha256']
entry = json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())
for row in entry['files']:
    assert pin(ROOT/row['live']['path']) == row['live'], 'Interrupted source changed'
    assert pin(ROOT/row['preimage']['path']) == row['preimage']
for row in json.loads((HERE/'entry-authority-pins-v1.json').read_bytes())['authorities']:
    assert pin(ROOT/row['path']) == row
save('interruption-resume-v1.json', dict(status='EXACT_ENTRY_BYTES_RETAINED',
    files=len(entry['files']), index=pin(ROOT/'.git/index'), original_queries=0))

expected = ['FA_COMMUNALITIES','FA_FACTORABILITY','FA_SCORES','KMEANS_FIT',
 'KMEANS_SUMMARY','KMEANS_CENTERS','KMEANS_ASSIGNMENTS','KMEANS_PREPROCESS',
 'KMEANS_REMOVED','HCLUST_FIT','HCLUST_SUMMARY','HCLUST_AGGLOM','HCLUST_LEAFORDER',
 'HCLUST_MEMBERSHIP','HCLUST_PREPROCESS','HCLUST_REMOVED','CA_FIT','CA_SUMMARY',
 'CA_EIGEN','CA_ROWS','CA_ROW_COORD','lambda','CA_ROW_COS2','lambda']
spans = ['50363:2342','53401:1994','56071:1206','62887:7333','70827:3621',
 '75243:1306','77132:1115','79037:1544','81226:1201','86887:4288','91778:2946',
 '95280:1109','96897:1114','99229:1892','101711:1572','103947:1232','109314:2167',
 '112081:2154','114877:1050','116705:1134','118528:1164','119451:50','120288:1151','121202:47']
queue = json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())
assert len(queue['members']) == 335 and queue['first_members'] == queue['members'][:24]
assert [r['name'] for r in queue['first_members']] == expected
assert [r['original_id'].split('@')[-1] for r in queue['first_members']] == spans
summary = json.loads((PREVIOUS/'ownership-summary-v2.json').read_bytes())
assert summary['historical'] == dict(total=704, closed=369, OPEN=335)
assert summary['current'] == dict(total=726, closed=399, OPEN=327)
assert summary['support'] == dict(raw=325, distinct=303, distinct_closed=247, distinct_OPEN=56)
assert (summary['fragments']['original']['covered'],summary['fragments']['original']['total']) == (8895,20683)
assert (summary['fragments']['current']['covered'],summary['fragments']['current']['total']) == (5095,15001)
for name in ['source_views.py','proof_kernel_v2.py','MethodMetadata-v11.cs',
             'native-metadata-owners-v3.txt','ReturnMetadata-v1.cs','DeclarationShapes-v1.cs']:
    with (HERE/name).open('xb') as stream:
        stream.write((PREVIOUS/name).read_bytes())
from source_views import body
universe = json.loads((PREVIOUS/'current-universe-packet-v2.json').read_bytes())
history = json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
original = json.loads((W2.parent/'w1/original-universe.json').read_bytes())
om = {r['symbol_id']:r for r in original['members']}
cm = {r['symbol_id']:r for r in universe['members']}
hm = {r['canonical_original_id']:r for r in history}
snapshots = {r['source']['path']:r['snapshot'] for r in json.loads((ROOT/
    'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
targets=[]
for queued in queue['first_members']:
    h=hm[queued['original_id']]
    assert len(h['current_ids']) == 1 and not h['proof']
    n=cm[h['current_ids'][0]]; o=om[h['git_original_id']]
    current_text, current_view=body((ROOT/n['path']).read_bytes(),n)
    original_text, original_view=body((ROOT/snapshots[o['path']]['path']).read_bytes(),o)
    targets.append(dict(original_id=queued['original_id'],name=n['name'],current_record=n,
        original_record=o,current_source=pin(ROOT/n['path']),original_source=snapshots[o['path']],
        current_whole_body=current_text,original_whole_body=original_text,
        current_view=current_view,original_view=original_view))
save('target-entry-bodies-v1.json',dict(rows=targets, original_queries=0))
paths=['BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb',
 'BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb',
 'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
new='BESHStat.Excel.Common.Tests/src/UdfContinuation11Tests.vb'
assert not (ROOT/new).exists()
by_path={r['live']['path']:r for r in entry['files']}
PRIVATE.mkdir(exist_ok=False)
save('implementation-plan-v1.json',dict(status='AUTHENTICATED_BEFORE_C11_PRODUCTION_EDITS',
    source_paths=paths+[new],source_before=[by_path[p] for p in paths],
    new_sources=[dict(path=new,entry_state='ABSENT')],first_members=queue['first_members'],
    entry=pin(HERE/'entry-outcome-v1.json'), scope='Five pure report layout phases in the exact FA/KMeans/hierarchical/CA cohort. FIT models, caches, Guid, import, coercion, exception/logging and materialization stay Windows-owned. Necessary complete helper/callback/declaration proof follows the actual graph. Original numerics and known defects are preserved.',
    original_queries=0))
print('C11 resume exact; canonical initial24 and all source preimages authenticated')
