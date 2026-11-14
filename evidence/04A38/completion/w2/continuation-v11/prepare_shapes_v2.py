from common import *
source=(HERE/'declaration_shapes_v1.py').read_text(encoding='utf-8')
source=source.replace("paths=list(dict.fromkeys(paths))", "paths.extend(r['path'] for r in json.loads((HERE/'dependency-source-universe-v1.json').read_bytes())['sources']);paths=list(dict.fromkeys(paths))")
with (HERE/'declaration_shapes_v2.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(source)
save('declaration-tool-scope-v2.json',dict(before=pin(HERE/'declaration_shapes_v1.py'),after=pin(HERE/'declaration_shapes_v2.py'),
    change='Include exact declared current dependency-source parameter shapes, in addition to native and portable declarations. Syntax only, no original compilation.',original_queries=0))
