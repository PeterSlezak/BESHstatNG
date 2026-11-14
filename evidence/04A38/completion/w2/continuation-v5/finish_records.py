"""Finalize C5 review text and independently preserve late adverse attempts."""
from common import *
from verify_derivative_v3 import inspect as derivative

p=HERE/'write_reviews_final.py'
text=p.read_text(encoding='utf-8')
text=text.replace('the `*-v2.json` ownership/fragment/validation/readiness ledgers',
                  'the `*-v3.json` ownership/fragment/validation/readiness ledgers')
text=text.replace('Earlier C5 cells-v1/v2/v3 attempts', 'Earlier C5 cells-v1/v2/v3/v4 attempts')
text=text.replace('Validation-v2.json and nonpasses-v3.json', 'validation-v3.json and nonpasses-v3.json')
text=text.replace('Preservation-final-v2.json', 'preservation-final-v3.json')
text=text.replace('command-receipts-v3.json;', 'command-receipts-final.json;')
text=text.replace('attempts/complete-attempt-accounting.json and raw isolated Runner receipts',
                  'attempts/complete-attempt-accounting.json, attempts/late-finalization-accounting.json and raw isolated Runner receipts')
text=text.replace('the local diagnostic preservation proof bind the omitted raw bytes',
                  'local-diagnostic-preservation-v2.json bind the omitted raw bytes')
text=text.replace('Final actual CIL/export/syntax:', 'Final actual CIL/export/syntax:')
text=text.replace('cells-v3 and old C5 provisional artifacts', 'cells-v1/v2/v3/v4 and old C5 provisional artifacts')
text=text.replace('Preserve source row identity, lower bounds, copy/name ownership, first-error/ByRef behavior and real native Framework Err catches.',
                  'Preserve source row identity, lower bounds, copy/name ownership, first-error/ByRef behavior and real native Framework Err catches. Native scalar conversion and grouped/nested acquisition must call the prepared Common phases after their original entry reset; a second reset can erase aliased callback writes. Existing public Common APIs keep their reset wrappers. See the retained alias-probe-before/after receipts and five whole-body source/four CIL proofs.')
text=text.replace('All 4695tracked/278pins/100captures/16owner-docs/41original snapshots',
                  'All 4695 tracked paths/278 pins/100 captures/16 owner documents/41 original snapshots')
p.write_text(text,encoding='utf-8',newline='\n')

relocation=json.loads((HERE/'diagnostic-encoding-relocation.json').read_bytes())
for key in ['retained_local','public_utf8_equivalent']:
    row=relocation[key]; data=(ROOT/row['path']).read_bytes()
    assert (len(data),sha(data))==(row['bytes'],row['sha256'])
assert json.loads((ROOT/relocation['retained_local']['path']).read_bytes())==json.loads((ROOT/relocation['public_utf8_equivalent']['path']).read_bytes())
save('local-diagnostic-preservation-v2.json',dict(status='FINAL_RAW_DIAGNOSTIC_AND_UTF16_ATTEMPT_PRESERVATION_PASS',
    final_derivative=derivative(),prior_derivative=pin(HERE/'local-diagnostic-preservation-final.json'),
    encoding_relocation=relocation,original_queries=0))

save('attempts/late-finalization-accounting.json',dict(
    status='LATE_ADVERSE_ATTEMPTS_RETAINED_AND_RESOLVED_WITHOUT_NEW_ORIGINAL_QUERIES',
    earlier=pin(HERE/'attempts/complete-attempt-accounting.json'),
    byref=dict(record=pin(HERE/'byref-boundary-implementation.json'),
        before=[pin(HERE/('alias-probe-before-'+cfg+'.json')) for cfg in ['Debug','Release']],
        after=[pin(HERE/('alias-probe-after-'+cfg+'.json')) for cfg in ['Debug','Release']],
        disposition='Original-source reset timing restored through prepared phases; previous no-observable-operation assertion explicitly rejected; all intermediate bodies/ledgers retained'),
    foreach_context=dict(record=pin(HERE/'attempts/prepared-foreach-context-v3.json'),
        disposition='v3 verifier wrongly required zero EH; v4 preserves the existing For Each disposal-Finally context exactly, without production change'),
    encoding=dict(record=pin(HERE/'python-encoding-correction-final.json'),
        disposition='Actual unchanged targeted rerun restores inherited FAIL; raw UTF16 bytes retained locally; no assertion waived'),
    narrative_errata=[
        dict(record='git-closeout-v3.json',field='scope',correction='Seven modified existing source/test paths plus one new Common source, not four plus one. The actual source_changes array and all eight transitions were already correct.'),
        dict(record='readiness-v3.json',field='decision',correction='Final executable authority is verify_slice_v4.py and verify_ownership_final.py. Generic earlier filenames in prose are superseded; independently recomputed nonzero counts/status remain correct.')],
    final_authority=['source-transitions-v4.json','body-contracts-v3.json','verify_slice_v4.py','verify_ownership_final.py','verify_preservation_v3.py'],
    original_queries=0))

records=[]
for path in sorted((RUNROOT/'commands').glob('w2c5-*/command.json')):
    records.append(dict(label=path.parent.name,receipt=clean(json.loads(path.read_bytes()))))
save('command-receipts-final.json',dict(records=records,
    raw_logs='Retained locally in isolated Runner command directories; excluded from publication copy',
    post_seal='Export/live/copied reader commands recorded in external post-seal receipt to avoid manifest recursion'))

git_records=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
    result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
    assert result.returncode==0
    git_records.append(dict(check=name,exit_code=result.returncode,stdout=result.stdout.decode('utf-8'),stderr=result.stderr.decode('utf-8')))
assert next(r for r in git_records if r['check']=='branch')['stdout'].strip()=='codex/04A-38'
assert all(r['stdout'].strip()==BASE for r in git_records if r['check'] in ['HEAD','main','merge-base'])
assert next(r for r in git_records if r['check']=='index')['stdout']==''
save('git-closeout-final.json',dict(records=git_records,branch='codex/04A-38',HEAD=BASE,index='EMPTY',diff_check_exit=0,
    source_changes=[r['path'] for r in json.loads((HERE/'source-transitions-v4.json').read_bytes())['files']],
    scope='Seven existing modified source/test paths plus one new Common source; all earlier uncommitted work retained. All4695 tracked paths independently verified. No index/branch/history operations.'))
print('Late adverse evidence, final diagnostic proof, command inventory and Git checks recorded')
