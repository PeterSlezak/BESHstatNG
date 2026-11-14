"""Bound final preflight to explicit complete review dependencies."""
from common_v2 import *
source=(HERE/'preflight_publication_v3.py').read_text(encoding='utf-8');source=source.replace("paths.add((W2/'continuation-v9-retained-private-reader-v1.py').relative_to(ROOT).as_posix())","paths.update((W2/name).relative_to(ROOT).as_posix() for name in ['continuation-v9-retained-private-reader-v1.py','continuation-v9-retained-private-reader-v2.py','continuation-v9-retained-private-reader-v3.py'])\npaths.update(r['current']['path'] for r in json.loads((HERE/'gui-udf-consumer-crosswalk-v1.json').read_bytes())['source'])")
with (HERE/'preflight_publication_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
print('Complete bounded preflight source prepared')
