"""Read final C10 current bodies against their fresh source census."""
from common import *
from source_views import body
sys.stdout.reconfigure(encoding='utf-8')
u=json.loads((HERE/'current-universe-packet-v2.json').read_bytes())
for name in sys.argv[1:]:
    nodes=[n for n in u['members'] if n['name']==name and n['kind']=='FunctionBlock'];assert len(nodes)==1,(name,len(nodes));n=nodes[0];text,view=body((ROOT/n['path']).read_bytes(),n);print(n['symbol_id'],view,'\n'+text)
