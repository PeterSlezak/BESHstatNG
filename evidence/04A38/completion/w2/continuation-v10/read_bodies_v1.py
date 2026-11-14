"""Read authenticated whole current/original Roslyn bodies; never compile/execute originals."""
from common import *
sys.stdout.reconfigure(encoding='utf-8')
sys.path.insert(0,str(PREVIOUS));from source_views import body
mode=sys.argv[1];names=sys.argv[2:];u=json.loads((PREVIOUS/'current-universe-packet-v1.json').read_bytes());h=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];o=json.loads((W2.parent/'w1/original-universe.json').read_bytes());originals={r['symbol_id']:r for r in o['members']};snapshots={r['source']['path']:r['snapshot']['path'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
for name in names:
    matches=[r for r in u['members'] if r['name']==name and r['kind']=='FunctionBlock'];assert len(matches)==1,(name,len(matches));n=matches[0]
    if mode=='original':
        history=next(r for r in h if n['symbol_id'] in r['current_ids']);n=originals[history['git_original_id']];raw=(ROOT/snapshots[n['path']]).read_bytes()
    else:assert mode=='current';raw=(ROOT/n['path']).read_bytes()
    text,policy=body(raw,n);print('\n'+n['symbol_id']+' ['+policy+']\n'+text)
