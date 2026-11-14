from common import *
u=json.loads((PREVIOUS/'current-universe-packet-v1.json').read_bytes());shapes=json.loads((PREVIOUS/'declaration-shapes-v1.json').read_bytes())['rows'];meta=json.loads((PREVIOUS/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes())
for n in u['members']:
 if n['path'].endswith('PropensityScoreUDFs.vb'):
  print(n['name'],n['kind'],'parent',n['parent_member'],'shape',next((s for s in shapes if s['symbol_id']==n['symbol_id']),None))
print('handle methods',[(m['member_name'],m['parameter_types']) for m in meta['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs+PsmUdfHandle'])
print('prior types',len(meta['types']))
