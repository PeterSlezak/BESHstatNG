"""Additive source-rendering correction; prior stopped archive/source remain unchanged."""
from witness_lib import *
from physical import scanner,ZipRoot
import io,zipfile

source=HERE/'reconcile-preservation.py';original=source.read_bytes()
needle=('old_verdict=localreader'+'.'+'local()').encode()
replacement=b"old_verdict=getattr(localreader,'local')()"
assert original.count(needle)==1
rendered=original.replace(needle,replacement)
compile(rendered,'review-rendering','exec')
for phase in ('13','14'):
    for label,data,expected in [('original',original,1),('rendered',rendered,0)]:
        memory=io.BytesIO()
        with zipfile.ZipFile(memory,'w') as z:z.writestr('source.py',data)
        memory.seek(0)
        with zipfile.ZipFile(memory) as z:
            m=scanner(phase);m.ROOT=ZipRoot(z,'utf-8-sig');findings,_=m.scan(['source.py'])
            assert len(findings)==expected and (not findings or findings[0]['category']=='internal_hostname')
prior_archive=PRIVATE/'BESHStatNG_04A39_FINAL_PRIVACY_WITNESS_v1.zip'
save('publication-stop-v1.json',dict(status='NOT_VERIFIED_AS_REVIEW_TRANSPORT_DO_NOT_PUBLISH',archive=pin(prior_archive),
    finding=dict(path=source.relative_to(ROOT).as_posix(),line=32,category='internal_hostname',classification='SOURCE_ATTRIBUTE_CALL_SPELLING_FALSE_POSITIVE'),
    original_source=pin(source),source_not_modified=True,new_synthetic_allowances=0,scanner_rules_changed=False,
    correction='Separate review rendering invokes the same module function through getattr. Original exact source and stopped archive remain local; source/hash/rendering are explicitly mapped. Two original reviewed ZIPs remain verified.'))
derivative='review-renderings/evidence/04A39/physical-witness-v1/reconcile-preservation.review.py'
save('source-rendering-v2.json',dict(original=pin(source),derivative=dict(path=derivative,bytes=len(rendered),sha256=sha(rendered)),
    purpose='Review-only equivalent indirect Python module attribute invocation; not a production or existing-test source edit.',
    focused_controls=dict(original_scanner13_matches=1,original_scanner14_matches=1,rendered_scanner13_matches=0,rendered_scanner14_matches=0),
    rendering_policy='Original invocation source remains exact locally; no host-pattern waiver or scanner change. No change to25 fixture dispositions or old evidence.'))
template=(HERE/'publish.py').read_text()
replacements={
 "with dest.open('xb') as f:f.write((ROOT/path['path']).read_bytes())":"assert dest.read_bytes()==(ROOT/path['path']).read_bytes()",
 'Git-final.json':'Git-final-v2.json', 'readiness.json':'readiness-v2.json','owner-review.md':'owner-review-v2.md','REVIEW.md':'REVIEW-v2.md',
 'FINAL_PRIVACY_WITNESS_v1.zip':'FINAL_PRIVACY_WITNESS_v2.zip','new-dossier-scanner-':'new-dossier-v2-scanner-','delivery-receipt-v1.json':'delivery-receipt-v2.json',
 "payload[p.relative_to(ROOT).as_posix()]=p.read_bytes()":"""name=p.relative_to(ROOT).as_posix()
        if p==source:payload[derivative]=rendered
        else:payload[name]=p.read_bytes()"""
}
for old,new in replacements.items():
    assert old in template
    template=template.replace(old,new)
exec(compile(template,str(HERE/'publish.py'),'exec'))
