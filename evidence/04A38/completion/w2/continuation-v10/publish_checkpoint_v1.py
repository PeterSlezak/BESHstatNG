"""One sealed C10 inventory/export; all command bookkeeping after seal stays external."""
from common import *
inventory=command('publication-inventory-v1',[sys.executable,'-B',str(HERE/'publish_v2.py'),'inventory']);assert inventory['exit_code']==0
export=command('publication-export-v1',[sys.executable,'-B',str(HERE/'publish_v2.py'),'export']);assert export['exit_code']==0
target=W2/'continuation-v10-publication-execution-v1.json'
with target.open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(inventory=clean(inventory),export=clean(export),scope='External additive process receipts; sealed selected payloads are never rewritten.'),f,ensure_ascii=False,indent=2);f.write('\n')
print('Physical review publication and copied executable complete; external receipt',target.name)
