"""Retain the new C5 diagnostic byte-for-byte; do not overwrite its encoding."""
from common import *
source=HERE/'preservation.json'
before=pin(source);data=source.read_bytes()
assert data.startswith(b'\xff\xfe')
folder=PRIVATE/'retained-encoding-attempt';folder.mkdir()
retained=folder/'preservation-utf16-original.json'
assert source.resolve().is_relative_to(HERE.resolve()) and retained.resolve().is_relative_to(PRIVATE.resolve())
assert not retained.exists()
source.rename(retained)
assert (len(retained.read_bytes()),sha(retained.read_bytes()))==(before['bytes'],before['sha256'])
values=json.loads(data)
save('preservation-encoding-normalized.json',values)
equivalent=HERE/'preservation-encoding-normalized.json'
assert json.loads(equivalent.read_bytes())==json.loads(retained.read_bytes())
save('diagnostic-encoding-relocation.json',dict(status='BYTE_EXACT_NEW_C5_DIAGNOSTIC_RELOCATED_WITH_UTF8_EQUIVALENT',
    former=before,retained_local=pin(retained),public_utf8_equivalent=pin(equivalent),decoded_JSON_values_equal=True,
    reason='PowerShell5 native redirection produced UTF16. Generic inherited UTF8-only privacy readers may report setup ERROR or cached-path missing after this correction; preserve those receipts and rerun the affected identity without changing its assertion.',
    preflight='publication-preflight.json records the former byte pin and was explicitly preliminary, not final publication authority',
    protected_prior_versions='No C4/earlier evidence, source preimage, corpus, owner document or tracked path moved or changed',original_queries=0))
print('New C5 UTF16 diagnostic retained exactly; UTF8 equivalent and explicit location ledger recorded')
