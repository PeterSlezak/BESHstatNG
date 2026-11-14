from common import *
names=['whole-body-contracts-v1.json','callback-proofs-v1.json','current-universe-families-v2.json','source-transitions-final-v1.json',
 'compiled-context-and-consumers.json','full-type-field-context.json']
pins={name:sha((HERE/name).read_bytes()) for name in names}
source=(HERE/'verify_checkpoint_template_v1.py').read_text(encoding='utf-8').replace('__AUTHENTICATED_INPUT_HASHES__',repr(pins))
with (HERE/'verify_checkpoint_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('review-seal-v1.json',dict(pins=pins,reader=pin(HERE/'verify_checkpoint_v1.py'),status='PARTIAL_W2_NOT_READY',original_queries=0))
