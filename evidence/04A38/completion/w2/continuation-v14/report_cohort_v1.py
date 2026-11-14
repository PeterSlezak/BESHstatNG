from common import *
q=json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes());old=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());before={n['symbol_id']:n for n in old['members']};u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes())
print('universe',len(u['members']),len(u['supports']),len(u['fragments']))
ids={n['symbol_id'] for n in q['required_native_parents']}|{n['symbol_id'] for n in q['required_callbacks']}|{r['current_record']['symbol_id'] for r in q['rows']}
print('open candidate names',[(before[k]['name'],before[k]['kind']) for k in sorted(ids) if before[k]['review_status'].startswith('OPEN')])
print('psm support',[(s['declaration'],s['fully_reconciled'],s['already_in_primary_population']) for s in old['support'] if s['path'].endswith('PropensityScoreUDFs.vb')])
from proof_kernel_v7 import definitions,semantic
for cfg in ['Debug','Release']:
 a=definitions(json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')).read_bytes()));b=definitions(json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')).read_bytes()));changed=[(b[k]['declaring_type'],b[k]['member_name']) for k in a if k in b and semantic(a[k])!=semantic(b[k])];added=[(b[k]['declaring_type'],b[k]['member_name']) for k in b if k not in a]
 print(cfg,'changed',changed,'added',added,'lost',len(set(a)-set(b)))
