"""Public diagnostic display labels only; preserve raw identities and all reference bytes."""
from common import *
from validation_report import safe_label
source=HERE/'full-managed-Debug-v1.json';x=json.loads(source.read_bytes());changes=[]
for s,suite in enumerate(x['results']):
 for i,r in enumerate(suite['records']):
  old=r['test_name'];new=safe_label(old)
  if old!=new:
   changes.append(dict(pointer=['results',s,'records',i,'test_name'],original_label_sha256=sha(old.encode()),public_label_sha256=sha(new.encode()),public_label=new,reason='Diagnostic display name contains inherited synthetic URI/path data-row parameter; actual source fixture and raw identity digest preserved elsewhere'))
   r['test_name']=new
save('full-managed-Debug-public-v2.json',x)
save('diagnostic-public-derivative.json',dict(raw_local=pin(source),public=pin(HERE/'full-managed-Debug-public-v2.json'),changes=changes,exact_change_scope='Only test_name display labels through existing accepted safe_label policy. All other JSON values, outcomes, messages, source pins, scientific/reference inputs/results/traces remain unchanged. Raw local receipt retained byte-for-byte; raw identity+occurrence hashes in managed-Debug-identities.json.',source_fixture='BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb has an inherited intentional malicious-URI DataRow; no real credential was published and no URL whitelist added',review_copy_exclusion=source.relative_to(ROOT).as_posix(),prior_manifest='publication-manifest-v1.json is a rejected privacy attempt, immutable; its excluded local raw diagnostic cannot be hash-checked from public derivative alone',actual_changed_display_labels=len(changes)))
s=(HERE/'publish.py').read_text(encoding='utf-8').replace("paths=sorted(paths)","paths.discard((HERE/'full-managed-Debug-v1.json').relative_to(ROOT).as_posix());paths=sorted(paths)")
# The earlier source has the sort on a combined assignment line; the substring above is exact.
assert 'paths.discard' in s
with (HERE/'publish_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'test_publication.py').read_text(encoding='utf-8').replace("MANIFEST=BASE+'publication-manifest-v1.json'","MANIFEST=BASE+'publication-manifest-v2.json'")
with (HERE/'test_publication_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
with (HERE/'publication-closeout-addendum.md').open('x',encoding='utf-8',newline='\n') as f:f.write('''# Publication privacy addendum — NOT_READY_04A38

Manifest v1 (3104 hashes plus one null self-entry) was rejected before any ZIP was created. A raw full-managed receipt included the intentional credential-shaped URI DataRow in a test display name from the unchanged CompositeOutputTests source. No URL exception or privacy relaxation was added.

The raw receipt and rejected manifest remain unchanged locally. full-managed-Debug-public-v2.json changes only diagnostic test_name display labels through the established safe_label policy. diagnostic-public-derivative.json records both file identities and every changed field/label digest. Raw identity/occurrence/outcomes remain available in managed-Debug-identities.json. No original/current statistical input, output, branch, source or trace byte was transformed. The raw full-managed receipt is excluded from the public derivative; consequently the rejected v1 manifest's excluded diagnostic cannot be independently rehashed from that ZIP. This is a declared review-copy exclusion, not lost local evidence.

Use verify_publication.py with the final manifest path explicitly; its default remains the earlier rejected v1 attempt. test_publication_v2.py passes v2 explicitly. Later closeout manifest versions include prior manifests as hashed payloads and all successful/failed validation receipts without editing their chronology. Final ZIP identity/CRC and final live Git checks are external export receipts, never invented self-referential ZIP data.
''')
print('Public diagnostic derivative',len(changes),'display labels; raw/source/reference bytes retained')
