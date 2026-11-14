"""Independent file-wise derivatives followed by authoritative private preservation."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v10'))
from common import *
parts=command('review-parts-v1',[sys.executable,'-B',str(HERE/'review_parts_v1.py')]);assert parts['exit_code']==0
private=command('retained-private-v2',[sys.executable,'-B',str(W2/'continuation-v10-retained-private-reader-v2.py')]);assert private['exit_code']==0
target=W2/'continuation-v10-postpublication-executions-v1.json'
with target.open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(parts=clean(parts),private=clean(private),scope='External additive execution records; all sealed payload bytes remain unchanged.'),f,ensure_ascii=False,indent=2);f.write('\n')
print('Complementary review parts and exact private preservation completed')
