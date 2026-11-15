from pathlib import Path
p=Path(__file__).with_name('compiled-index-v2.py');s=p.read_text(encoding='utf-8')
old="if 'type' in v and 'fields' in v"
assert s.count(old)==1
exec(compile(s.replace(old,"if isinstance(v.get('type'),str) and 'fields' in v"),str(p),'exec'))
