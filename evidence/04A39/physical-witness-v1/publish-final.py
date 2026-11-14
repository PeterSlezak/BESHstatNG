"""Exact-literal versioned publication names; no historical path substitutions."""
from witness_lib import *
raw_source=HERE/'reconcile-preservation.py'
raw=raw_source.read_bytes();needle=('old_verdict=localreader'+'.'+'local()').encode()
assert raw.count(needle)==1
rendered=raw.replace(needle,b"old_verdict=getattr(localreader,'local')()")
derivative='review-renderings/evidence/04A39/physical-witness-v1/reconcile-preservation.review.py'
save('publication-rendering-v4.json',dict(original=pin(raw_source),derivative=dict(path=derivative,bytes=len(rendered),sha256=sha(rendered)),
    new_synthetic_allowances=0,source_or_scanner_change=False,
    focused_controls=load(HERE/'source-rendering-v2.json')['focused_controls'],
    retained_attempts=[dict(version=1,status='QUARANTINED_NOT_PUBLISHED_SOURCE_SPELLING_FALSE_POSITIVE'),dict(version=2,status='PREARCHIVE_RECEIPT_FILENAME_COLLISION'),dict(version=3,status='PREARCHIVE_LINEAGE_FILENAME_SUBSTITUTION_ERROR')],
    correction='Equivalent getattr invocation in explicitly named review rendering only. Original execution source, old authority files, private receipts and stopped outputs remain exact.'))
template=(HERE/'publish.py').read_text(encoding='utf-8')
edits={
 "with dest.open('xb') as f:f.write((ROOT/path['path']).read_bytes())":"assert dest.read_bytes()==(ROOT/path['path']).read_bytes()",
 "'git-final-raw.json'":"'git-final-v4-raw.json'",
 "'Git-final.json'":"'Git-final-v4.json'",
 "'readiness.json'":"'readiness-v4.json'",
 "'owner-review.md'":"'owner-review-v4.md'",
 "'REVIEW.md'":"'REVIEW-v4.md'",
 "'BESHStatNG_04A39_FINAL_PRIVACY_WITNESS_v1.zip'":"'BESHStatNG_04A39_FINAL_PRIVACY_WITNESS_v4.zip'",
 "'new-dossier-scanner-'":"'new-dossier-v4-scanner-'",
 "'delivery-receipt-v1.json'":"'delivery-receipt-v4.json'",
 'from readback import readback':'from readback_v2 import readback',
 "index=dict(schema='04A39-final-physical-privacy-witness-v1'":"index=dict(active_readiness='evidence/04A39/physical-witness-v1/readiness-v4.json',schema='04A39-final-physical-privacy-witness-v1'",
 "payload[p.relative_to(ROOT).as_posix()]=p.read_bytes()":"""name=p.relative_to(ROOT).as_posix()
        if p==raw_source:payload[derivative]=rendered
        else:payload[name]=p.read_bytes()"""
}
for old,new in edits.items():
    assert old in template
    template=template.replace(old,new)
assert "OLD/'final/readiness.json'" in template
exec(compile(template,str(HERE/'publish.py'),'exec'))
