"""Bounded diagnostics for optimization differences; no inferred waiver."""
from common import *
from collections import Counter
d=load(HERE/'abi-cil/cross-configuration-v1.json')
print('MISSING',[r['method'] for r in d['Debug_only']],flush=True)
for r in d['signature_attribute_differences'][:3]: print('SIGNATURE_SAMPLE',json.dumps(r),flush=True)
print('EDGE_SAMPLE',json.dumps(d['ordered_call_delegate_dependency_differences'][0]),flush=True)
print('NON_STATE_MACHINE_SIGNATURES',[r['method'] for r in d['signature_attribute_differences'] if 'VB$StateMachine' not in r['method']],flush=True)
original=W2/'continuation-v12/metadata-all/packet-v1-context-v5/original-Release.json'
old=load(original)
print('OLD_RELEASE_SCHEMA',list(old),flush=True)
