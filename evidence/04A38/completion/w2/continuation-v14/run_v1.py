from common import *
source=HERE/sys.argv[2]
r=command(sys.argv[1],[sys.executable,'-B',str(source),*sys.argv[3:]])
save(sys.argv[1]+'-execution.json',dict(source=pin(source),result=clean(r)))
assert r['exit_code']==0,'Preserve failed attempt and investigate '+sys.argv[1]
