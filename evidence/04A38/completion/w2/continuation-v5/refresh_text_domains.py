from common import *
binary=RUNROOT/'completion-w2-cont4/domain-tool-v2/bin/CallDomains-v2.dll'
receipt=json.loads((PREVIOUS/'domain-tool-corrected.json').read_bytes())
assert library_pin(binary)==receipt['binary']
r=command('final-text-call-syntax',['dotnet',str(binary),str(ROOT),str(HERE/'text-call-sites.json')]);assert r['exit_code']==0
for name in ['TextDomainProbe-v3.cs','run_text_domains_v5.py','record_text_domains_v3.py']:
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write((PREVIOUS/name).read_text(encoding='utf-8'))
text=(PREVIOUS/'verify_text_domains_v2.py').read_text(encoding='utf-8').replace('parametric-v4','cells-v4')
with (HERE/'verify_text_domains_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
save('text-domain-syntax-final.json',dict(command=clean(r),binary=library_pin(binary),source=pin(PREVIOUS/'CallDomains-v2.cs'),
    source_census='All live Common source files including the new C5 conversion class; actual209 comparison sites',original_queries=0))
