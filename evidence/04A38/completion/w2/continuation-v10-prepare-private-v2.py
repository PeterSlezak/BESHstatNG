"""Additive post-seal reader version: preserve current replay and test binaries too."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v10'))
from common import *
prefix="import sys\nfrom pathlib import Path\nsys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v10'))\n"
source=(HERE/'retained_private_v1.py').read_text(encoding='utf-8')
point='    for row in rows.values():check(row)'
extra="""    for config in ['Debug','Release']:
        for artifact in sorted((PRIVATE/'packet-v2'/config/'udf').glob('*')):
            if artifact.is_file() and artifact.suffix.lower() in ['.dll','.exe','.config']:
                row=pin(artifact);assert rows.setdefault(row['path'],row)==row
    for folder in sorted((RUNROOT/'commands').glob('w2c10-checkpoint-v1-Debug-serial-*')):
        record=json.loads((folder/'command.json').read_bytes())
        for value in record['command']:
            path=Path(value)
            if str(value).lower().endswith('.dll') and path.is_file() and path.resolve().is_relative_to(RUNROOT.resolve()):
                row=pin(path);assert rows.setdefault(row['path'],row)==row
"""
assert point in source;source=prefix+source.replace(point,extra+point).replace("target=W2/'continuation-v10-retained-private-preservation-v1.json'","target=W2/'continuation-v10-retained-private-preservation-v2.json'")
target=W2/'continuation-v10-retained-private-reader-v2.py'
with target.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
post=(HERE/'postseal_v1.py').read_text(encoding='utf-8').replace('continuation-v10-retained-private-preservation-v1.json','continuation-v10-retained-private-preservation-v2.json')
with (W2/'continuation-v10-postseal-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(prefix+post)
record=dict(reader=dict(base=pin(HERE/'retained_private_v1.py'),final=pin(target)),postseal=dict(base=pin(HERE/'postseal_v1.py'),final=pin(W2/'continuation-v10-postseal-v2.py')),scope='External additive final tools, no sealed payload rewrite. Preserve current safe facade/recorders/config files and actual Debug test binaries in addition to the existing source-bound streams/TRX/workers/metadata/shipping assemblies and all inherited1964 pins. No test rerun, original execution or production/test edit.',original_queries=0)
with (W2/'continuation-v10-postseal-tool-crosswalk-v1.json').open('x',encoding='utf-8',newline='\n') as f:json.dump(record,f,ensure_ascii=False,indent=2);f.write('\n')
print('Additive final private reader/postseal tools versioned')
