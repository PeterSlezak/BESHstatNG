"""Use the manifest-selected active readiness version; keep the original reader unchanged."""
from pathlib import Path
source=Path(__file__).with_name('readback.py').read_text(encoding='utf-8')
old="ready=json.loads(z.read(prefix+'readiness.json'))"
new="ready=json.loads(z.read(index.get('active_readiness',prefix+'readiness.json')))"
assert source.count(old)==1
exec(compile(source.replace(old,new),str(Path(__file__).with_name('readback.py')),'exec'))
