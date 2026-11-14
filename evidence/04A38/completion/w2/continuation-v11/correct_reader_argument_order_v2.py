"""Preserve the failed sealed v1; version only the new reader orchestration bug."""
from common import *
before=HERE/'verify_c11_checkpoint_template_v2.py'
source=before.read_text(encoding='utf-8')
old="summary['validation']=validation(load,PREFIX,raw)"
new="summary['validation']=validation(load,raw,PREFIX)"
assert source.count(old)==1
with (HERE/'verify_c11_checkpoint_template_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source.replace(old,new))
source=(HERE/'seal_controls_v1.py').read_text(encoding='utf-8')
source=source.replace('verify_c11_checkpoint_template_v2.py','verify_c11_checkpoint_template_v3.py')
source=source.replace('verify_c11_checkpoint_v1','verify_c11_checkpoint_v2')
source=source.replace('review-seal-v1.json','review-seal-v2.json').replace('sealed-reader-positive-v1','sealed-reader-positive-v2')
source=source.replace('independent-controls-v1.json','independent-controls-v2.json')
with (HERE/'seal_controls_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
for oldname,newname in [('verify_c11_publication_v1.py','verify_c11_publication_v2.py'),
                        ('publish_parts_v1.py','publish_parts_v2.py'),('copy_review_v1.py','copy_review_v2.py'),
                        ('publication_adverse_v1.py','publication_adverse_v2.py')]:
    source=(HERE/oldname).read_text(encoding='utf-8')
    source=source.replace('verify_c11_checkpoint_v1','verify_c11_checkpoint_v2').replace('verify_c11_publication_v1','verify_c11_publication_v2')
    with (HERE/newname).open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('sealed-reader-first-failure-correction-v2.json',dict(failed_seal=pin(HERE/'review-seal-v1.json'),
    failed_reader=pin(HERE/'verify_c11_checkpoint_v1.py'),failed_execution=pin(HERE/'sealed-reader-positive-v1.json'),
    failed_stdout=pin(RUNROOT/'commands/w2c11-sealed-reader-positive-v1/stdout.log'),
    observed_error="TypeError: unsupported operand type(s) for +: 'function' and 'str'",
    cause='The new checkpoint reader passed prefix and raw arguments to verify_validation.inspect in reverse order. Static-only development omitted the validation phase; the full sealed positive correctly failed closed.',
    correction='Version3 template calls validation(load,raw,PREFIX); authoritative checkpoint/publication readers become v2. All v1 bytes, seal, failed execution and source remain. No production/test change or test rerun.',
    template_before=pin(before),template_after=pin(HERE/'verify_c11_checkpoint_template_v3.py'),original_queries=0))
note='''# C11 additive sealed-reader correction

NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED

The first sealed checkpoint reader v1 failed closed on a new orchestration argument-order bug at its validation call. Its source, review-seal-v1.json, exit1 receipt and complete raw output remain unchanged. The corrected version uses validation(load, raw, PREFIX); no source/test assertion, production input or statistical output changed, and no unit suite was rerun.

Authoritative successor source: verify_c11_checkpoint_v2.py and verify_c11_publication_v2.py, with review-seal-v2.json and sealed-reader-positive-v2.json. The original owner-review.md remains sealed and unchanged; read this additive correction with its final post-seal handover addendum. All exact ownership, export, compatibility and test counts in that review remain the observed counts. Successful final reader/control/physical publication receipts must be read independently; a failed v1 is never relabelled PASS.
'''
with (HERE/'owner-review-reader-addendum-v1.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write(note)
print('Failed sealed v1 preserved; exact validation argument-order successor v2 prepared')
