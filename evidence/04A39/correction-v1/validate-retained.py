from common import *
import importlib.util

lock()
checkpoint=load(PRIVATE/'checkpoint-files.json')
rows=checkpoint['files'] if isinstance(checkpoint,dict) and 'files' in checkpoint else checkpoint
print('Checkpoint document shape',type(rows).__name__,flush=True)
spec=importlib.util.spec_from_file_location('register39_independent',PRIOR/'verify-register-v5.py')
reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
register=reader.verify(ROOT,load(PRIOR/'register/defects-v7.json'))
mapped=load(PRIOR/'map/method-policy-map-v1.json')
assert mapped['focus_members']==192 and len(mapped['methods'])==192
for row in mapped['sources']:
    path=ROOT/row['path'];assert path.stat().st_size==row['bytes'] and digest(path)==row['sha256']
old=load(PRIOR/'final/readiness.json')
# The receipt and all previous dossier files are bound to the frozen checkpoint.
save('validation/retained-evidence-readback.json',dict(status='SOURCE_AND_REGISTER_READER_VERIFIED;VALIDATION_OUTCOMES_RETAINED',source_lock=pin(HERE/'entry/source-lock.json'),
    source_paths=len(load(HERE/'entry/source-lock.json')['files']),production_or_test_edits_this_continuation=False,
    canonical_register=pin(PRIOR/'register/defects-v7.json'),register_reader=pin(PRIOR/'verify-register-v5.py'),register_counts=register,
    policy_map=pin(PRIOR/'map/method-policy-map-v1.json'),focused_members=192,
    retained_facts=old['facts'],fresh_full_managed=False,fresh_release=False,fresh_node=False,fresh_compatibility=False,
    wrappers='NOT_RUN;unchanged original39 reason: implicit restore/shared historical TestResults. No product changes; focused reporting controls substitute only for this continuation, not wrapper PASS.',
    scientific_native_manual='NOT_RUN_UNAUTHORIZED',gates=GATES))
print('Register independently verified',register['canonical_entries'],register['original_obligations'],register['restricted_controls'],'policy focus192',flush=True)
