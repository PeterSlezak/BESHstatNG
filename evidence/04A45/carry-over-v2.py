from common45 import *
source=(HERE/'carry-over-v1.py').read_text()
source=source.replace("importer=sorted(ids&set(strings(transport)));assert len(importer)==44","importer=sorted({r['obligation_id'] for r in transport['blocked_switch']['affected_refedit_obligations']});assert len(importer)==44 and set(importer)<=ids")
source=source.replace("'scope/carry-over-v1.json'","'scope/carry-over-v2.json'")
exec(compile(source,str(HERE/'carry-over-v1.py'),'exec'))
