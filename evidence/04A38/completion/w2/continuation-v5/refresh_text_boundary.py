from common import *
binary=RUNROOT/'completion-w2-cont4/domain-tool-v2/bin/CallDomains-v2.dll'
assert library_pin(binary)==json.loads((PREVIOUS/'domain-tool-corrected.json').read_bytes())['binary']
r=command('boundary-text-call-syntax',['dotnet',str(binary),str(ROOT),str(HERE/'text-call-sites-v2.json')]);assert r['exit_code']==0
save('text-domain-syntax-boundary.json',dict(command=clean(r),binary=library_pin(binary),original_queries=0))
