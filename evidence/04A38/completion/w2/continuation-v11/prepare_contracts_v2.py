from common import *
source=(HERE/'build_contracts_v1.py').read_text(encoding='utf-8')
source=source.replace("        need(not h['proof'],'Ambiguous previously supported historical identity')\n", "        # Retired/no-current bodies outside this graph remain under exact sealed v10 authority.\n")
with (HERE/'build_contracts_v2.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(source)
save('contract-builder-first-failure-v1.json',dict(before=pin(HERE/'build_contracts_v1.py'),
    after=pin(HERE/'build_contracts_v2.py'),exit_code=1,
    observed_error='ValueError: Ambiguous previously supported historical identity',
    cause='Five previously supported retired GEE/MMRM bodies have no current declaration. They lie outside the C11 graph and retain their sealed v10 removal/composite authority; the new builder incorrectly required a one-current-body mapping for them.',
    correction='Limit the new cohort mapping assertion to actual C11 candidates. Preserve all historical universe identities and prior proofs; do not create or infer current aliases for retired bodies.',production_changes=0,original_queries=0))
result=command('contracts-v2',[sys.executable,'-B',str(HERE/'build_contracts_v2.py')])
save('contract-builder-execution-v2.json',clean(result));assert result['exit_code']==0
