from common import *
source=(PREVIOUS/'verify_publication.py').read_text(encoding='utf-8').split('def inspect(root=ROOT',1)[0]
source='\n'.join(line for line in source.splitlines() if not line.startswith(('from verify_ownership_v2 import','from verify_preservation import','from verify_fragment_scope import')))+'\n'
source=source.replace("p.lower().endswith(('.pyc','.pyo','/full-managed-debug-v1.json'))", "p.lower().endswith(('.pyc','.pyo')) or bool(re.search(r'/full-managed-debug-v[0-9]+\\.json$',p.lower()))")
with (HERE/'publication_policy.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
