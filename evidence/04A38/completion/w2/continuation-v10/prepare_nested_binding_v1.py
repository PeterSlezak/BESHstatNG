"""Resolve two declared VB nested cache types to their exact CLR names, not closure aliases."""
from common import *
source=(HERE/'proof_kernel_v1.py').read_text(encoding='utf-8');needle='def bind(node,meta,shapes):';assert needle in source
helper='''NESTED_CACHE_DECLARATIONS={"BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs": {"PcaHandle", "FactorAnalysisHandle"}}
def nested_parameter_matches(node,source,actual,meta):
    name=source.get('type');owner=node['owner']
    if name not in NESTED_CACHE_DECLARATIONS.get(owner,set()):return False
    expected=owner+'+'+name+('&' if source['byref'] else '')
    if actual!=expected:return False
    need(any(t['type']==owner+'+'+name for t in meta['types']),'Missing declared nested cache type context')
    return True

'''
source=source.replace(needle,helper+needle).replace('all(parameter_matches(p,a) for p,a in zip(params,r[\'parameter_types\']))','all(parameter_matches(p,a) or nested_parameter_matches(node,p,a,meta) for p,a in zip(params,r[\'parameter_types\']))')
with (HERE/'proof_kernel_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('nested-binding-source-scope-v1.json',dict(before=pin(HERE/'proof_kernel_v1.py'),after=pin(HERE/'proof_kernel_v2.py'),first_diagnostic=pin(HERE/'binding-diagnostic-v1.json'),scope='Exactly two existing source-declared nested cache types under the real MultivariateAnalysisUDFs owner. Match full CLR owner-plus-type, ByRef and actual metadata type existence. Not a compiler-generated closure alias or a signature-only fallback. Complete source declarations/fields/ctors/accessors/callers are separately mandatory.',original_queries=0))
print('Two exact lexical nested cache type bindings versioned')
