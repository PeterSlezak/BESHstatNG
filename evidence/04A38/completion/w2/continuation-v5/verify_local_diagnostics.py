from common import *
import importlib.util
spec=importlib.util.spec_from_file_location('c5diagnosticsfinal',HERE/'verify_derivative_v2.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
diagnostic=module.inspect()
relocation=json.loads((HERE/'diagnostic-encoding-relocation.json').read_bytes())
for key in ['retained_local','public_utf8_equivalent']:
    row=relocation[key];data=(ROOT/row['path']).read_bytes();assert (len(data),sha(data))==(row['bytes'],row['sha256'])
assert json.loads((ROOT/relocation['retained_local']['path']).read_bytes())==json.loads((ROOT/relocation['public_utf8_equivalent']['path']).read_bytes())
save('local-diagnostic-preservation-final.json',dict(status='RAW_DIAGNOSTIC_AND_UTF16_ATTEMPT_BYTE_PRESERVATION_PASS',
    public_derivative=diagnostic,encoding_relocation=relocation,publication_exclusion='Raw diagnostics and original UTF16 receipt remain local; exact two-label display derivative and value-equivalent UTF8 receipt are public',
    original_queries=0))
print('Local raw diagnostic/relocated encoding attempt preserved exactly')
