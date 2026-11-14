"""Keep sealed v1; isolate predecessor imports and use a unique C11 validation module."""
from common import *
before=HERE/'verify_c11_checkpoint_template_v2.py'
source=before.read_text(encoding='utf-8')
assert "validation(load,raw,PREFIX)" in source
source=source.replace('from verify_validation_v1 import inspect as validation','from verify_c11_validation_v1 import inspect as validation')
old="    sys.path.insert(0,str(root/OLD))\n    from verify_c10_checkpoint_v1 import compute as prior_compute\n    prior_proof=prior_compute(root,projected)"
new="""    saved_import_paths=list(sys.path)
    current_directories={HERE.resolve(),(root/PREFIX).resolve()}
    sys.path[:]=[p for p in saved_import_paths if Path(p).resolve() not in current_directories]
    sys.path.insert(0,str(root/OLD))
    try:
        from verify_c10_checkpoint_v1 import compute as prior_compute
        prior_proof=prior_compute(root,projected)
    finally:sys.path[:]=saved_import_paths"""
assert source.count(old)==1;source=source.replace(old,new)
with (HERE/'verify_c11_checkpoint_template_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
with (HERE/'verify_c11_validation_v1.py').open('xb') as stream:stream.write((HERE/'verify_validation_v1.py').read_bytes())
source=(HERE/'seal_controls_v1.py').read_text(encoding='utf-8')
source=source.replace('verify_c11_checkpoint_template_v2.py','verify_c11_checkpoint_template_v3.py')
source=source.replace('verify_c11_checkpoint_v1','verify_c11_checkpoint_v2')
source=source.replace('review-seal-v1.json','review-seal-v2.json').replace('sealed-reader-positive-v1','sealed-reader-positive-v2')
source=source.replace('independent-controls-v1.json','independent-controls-v2.json').replace("PREFIX+'seal_controls_v1.py'","PREFIX+'seal_controls_v2.py'")
with (HERE/'seal_controls_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
for oldname,newname in [('verify_c11_publication_v1.py','verify_c11_publication_v2.py'),
                        ('publish_parts_v1.py','publish_parts_v2.py'),('copy_review_v1.py','copy_review_v2.py'),
                        ('publication_adverse_v1.py','publication_adverse_v2.py')]:
    source=(HERE/oldname).read_text(encoding='utf-8').replace('verify_c11_checkpoint_v1','verify_c11_checkpoint_v2').replace('verify_c11_publication_v1','verify_c11_publication_v2')
    with (HERE/newname).open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('sealed-reader-first-failure-correction-v3.json',dict(failed_seal=pin(HERE/'review-seal-v1.json'),
    failed_reader=pin(HERE/'verify_c11_checkpoint_v1.py'),failed_execution=pin(HERE/'sealed-reader-positive-v1.json'),
    failed_stdout=pin(RUNROOT/'commands/w2c11-sealed-reader-positive-v1/stdout.log'),
    observed_error="TypeError: unsupported operand type(s) for +: 'function' and 'str'",
    cause='C11 used generic verify_validation_v1, also used by the retained v6/v7 readers with a different parameter order. Prior-reader imports resolved the new C11 module while replaying immutable predecessors.',
    prior_diagnosis=dict(source=pin(HERE/'correct_reader_argument_order_v2.py'),observed_exit_code=1,
        result='The correction assertion failed before any write because the call already had correct argument order. A chained invocation of the not-yet-created seal_controls_v2.py also failed; no source or sealed file was changed.'),
    correction='Use unique verify_c11_validation_v1, and remove only the C11 directory during predecessor import/replay, restoring the caller path afterward. Preserve all prior reader bytes and assertions. Authoritative current checkpoint/publication readers are v2.',
    template_before=pin(before),template_after=pin(HERE/'verify_c11_checkpoint_template_v3.py'),
    validation_before=pin(HERE/'verify_validation_v1.py'),validation_after=pin(HERE/'verify_c11_validation_v1.py'),
    production_test_changes=0,test_reruns=0,original_queries=0))
note='''# C11 additive sealed-reader correction

NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED

The first sealed checkpoint reader v1 failed closed because a new generic validation module name collided with retained v6/v7 reader imports. The first proposed argument-order diagnosis was incorrect: its assertion failed before any write, and that rejected tool is retained too. No production source or test assertion changed, and no unit campaign was rerun.

The successor gives C11 validation a unique module name and isolates predecessor imports from the C11 directory while replaying the unchanged actual prior readers. All old sources, review-seal-v1.json, failed exit1 receipt and raw output remain immutable. Authoritative successor files are verify_c11_checkpoint_v2.py, verify_c11_publication_v2.py, review-seal-v2.json and sealed-reader-positive-v2.json. Read their actual final outcomes rather than inferring success here.

The original owner-review.md remains sealed and unchanged; read this additive note with the final external handover addendum. All ownership, export, compatibility and test counts in the review remain the observed counts. A failed predecessor is never relabelled PASS.
'''
with (HERE/'owner-review-reader-addendum-v1.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write(note)
print('Failed sealed v1 and rejected diagnosis retained; import-isolated successor v2 prepared')
