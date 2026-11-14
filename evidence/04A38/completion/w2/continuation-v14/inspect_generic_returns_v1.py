from common import *
for cfg in ['Debug','Release']:
 old=json.loads((ROOT/('evidence/04A38/preedit/compiled-'+cfg+'.json')).read_bytes())['methods']
 print(cfg,[r for r in old if 'TryGetCachedHandle' in r['method']])
