import sys
from pathlib import Path
sys.path.insert(0,str(Path.cwd()/'evidence/04A38/completion/w2/continuation-v14'))
from common import *
book=PRIVATE/'bookkeeping-code-v1';source=book/'run_private_v1.py';v=source.read_text();old="import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:]";new="import sys,runpy,pathlib;sys.path.insert(0,sys.argv[1]);sys.path.insert(0,str(pathlib.Path(sys.argv[2]).parent));sys.argv=[sys.argv[2]]+sys.argv[3:]";assert old in v
with (book/'run_private_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v.replace(old,new,1))
with (book/'private-draft-bootstrap-correction-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(before=pin(source),after=pin(book/'run_private_v2.py'),reason='The subprocess -c bootstrap must explicitly include the ignored private tool directory for versioned draft imports. Failed import occurred before contract execution; no public or production bytes changed.'),f,indent=2)
