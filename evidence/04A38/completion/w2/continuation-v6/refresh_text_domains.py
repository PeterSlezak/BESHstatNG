from common import *
binary=RUNROOT/'completion-w2-cont4/domain-tool-v2/bin/CallDomains-v2.dll'
receipt=json.loads((W2/'continuation-v4/domain-tool-corrected.json').read_bytes());assert library_pin(binary)==receipt['binary']
r=command('final-text-call-syntax',['dotnet',str(binary),str(ROOT),str(HERE/'text-call-sites-v2.json')]);assert r['exit_code']==0
for name in ['TextDomainProbe-v3.cs','run_text_domains_v6.py']:
 with (HERE/name).open('xb') as f:f.write((PREVIOUS/name).read_bytes())
source=(PREVIOUS/'record_text_domains_final.py').read_text(encoding='utf-8')
source=source.replace("elif owner=='UdfScalarChoice38':", "elif owner=='UdfInferencePreparation38':normal='Native NormalizeText yields trimmed invariant-uppercase selector at its original before/after-fit position; all option RHS/choices are fixed non-null ASCII; raw null/ignorable RHS differences remain out of caller domain'\n elif owner=='UdfScalarChoice38':")
source=source.replace('ALL59_DIRECT_LINES_AND64_CASETOKEN_SITES_DOMAIN_REVIEWED','ALL_ACTUAL_C6_SITES_DOMAIN_REVIEWED')
with (HERE/'record_text_domains.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('text-domain-syntax-v1.json',dict(command=clean(r),binary=library_pin(binary),source=pin(W2/'continuation-v4/CallDomains-v2.cs'),original_queries=0))
exec(compile(source,str(HERE/'record_text_domains.py'),'exec'))
