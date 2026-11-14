from common import *
from source_views import body
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());n=next(n for n in u['members'] if n['symbol_id']=='BESHstatNG/src/ExcelUDFs/UdfOutputTables.vb|BESHStatNG.UdfOutputTables|FunctionBlock|0930af4a3f1a65dc@12669:634')
old=next(n for n in json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())['members'] if n['symbol_id']==n['symbol_id'] and n['name']==n['name']) if False else next(p for p in json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())['members'] if p['symbol_id']==n['symbol_id'])
print(n['name'],old['review_status']);print(body((ROOT/n['path']).read_bytes(),n)[0])
meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());target=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(n['owner'],n['name']))['method']
print('callers',[(m['declaring_type'],m['member_name']) for m in meta['methods'] if any(i.get('operand')=='METHOD:'+target for i in m['instructions'])])
