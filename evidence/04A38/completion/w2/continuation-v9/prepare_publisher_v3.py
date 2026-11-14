"""Include four unchanged GUI source consumers as reviewable proof dependencies."""
from common_v2 import *
source=(HERE/'publish_v2.py').read_text(encoding='utf-8');needle="    paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])";assert needle in source
source=source.replace(needle,needle+"\n    paths.update(r['current']['path'] for r in json.loads((HERE/'gui-udf-consumer-crosswalk-v1.json').read_bytes())['source'])")
with (HERE/'publish_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('publisher-dependency-crosswalk-v1.json',dict(before=pin(HERE/'publish_v2.py'),after=pin(HERE/'publish_v3.py'),included_GUI_sources=json.loads((HERE/'gui-udf-consumer-crosswalk-v1.json').read_bytes())['source'],scope='Source contents unchanged; include exact GUI source consumers in the bounded derivative. No GUI closure/native execution claim.'))
print('Publisher includes unchanged GUI source proof dependencies')
