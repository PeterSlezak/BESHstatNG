"""Final actual receipts, controlled diagnostics and bounded new preservation inventory."""
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'evidence/04A40'))
from common40_v3 import *
import shutil,collections,re
sys.path.append(str(ROOT/'evidence/04A38'));from baseline_report import clean
lock()
serial=runner().serial('a40-final2-serial-v1');save(PRIVATE/'serial-raw-v2.json',serial)
save('python/scanners-fresh-v2.json',dict(exits=[r['exit_code'] for r in serial],raw=pin(PRIVATE/'serial-raw-v2.json'),
    categories=['strict','profile','MkDocs'],disposition=['INHERITED_NONPASS_UNWAIVED','INHERITED_NONPASS_UNWAIVED','ACTUAL_EXIT'],NormalPlot='Strict ambiguity remains uncorrected; no scientific/native gate release.'))
raw=HERE/'baseline/Debug-build-v1.json';target=PRIVATE/'protected-public-raw/baseline/Debug-build-v1.json';target.parent.mkdir(parents=True);shutil.copyfile(raw,target)
assert pin(raw)['sha256']==pin(target)['sha256']
shutil.copyfile(PRIVATE/'baseline-Debug-build-review-v1.json',HERE/'baseline/Debug-build-review-v2.json')
save('baseline/Debug-build-review-v2-provenance.json',dict(kind='SANITIZED_REVIEW_DERIVATIVE_ONLY',original=pin(raw),protected_exact_copy=pin(target),derivative=pin(HERE/'baseline/Debug-build-review-v2.json'),
    actual_original_scanner_findings=6,derivative_findings=0,repository_scanner_FAIL_unwaived=True,replace_original=False))
commands=[]
for p in sorted((PRIVATE/'commands').glob('*/command.json')):
    d=load(p);commands.append(dict(id=p.parent.name,raw=pin(p),review=clean(d)))
save('matrix/actual-commands-v2.json',dict(commands=commands,scope='Every completed recorded command in this task before this finalization receipt; actual arguments/cwd/exits/times/output fingerprints. Raw remains private; public paths use deterministic aliases.',
  source=pin(ROOT/'scripts/test_suite_rationalization.py'),environment_overrides='Source-bound runner and campaign scripts; inherited environment is not dumped or falsely claimed identical to older machines.'))
w3=load(ROOT/'evidence/04A38/completion/w3/build-and-tests/Release-identities-v1.json');old={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in w3['nonpasses']}
current=load(HERE/'build-and-tests/Release-identities-v2.json');rows=[]
for r in current['nonpasses']:
    previous=old[(r['suite'],r['identity_sha256'],r['occurrence'])]
    assert r['outcome']==previous['outcome'] and r['message_sha256']==previous['message_sha256']
    rows.append(dict(identity_sha256=r['identity_sha256'],suite=r['suite'],outcome=r['outcome'],message_exact=True,trace_exact=r['traceback_sha256']==previous['traceback_sha256'],
       current_trace=r['traceback_sha256'],W3_Release_trace=previous['traceback_sha256'],Debug_comparison=r['classification']))
save('build-and-tests/Release-W3-comparison-v1.json',dict(rows=rows,baseline=pin(ROOT/'evidence/04A38/completion/w3/build-and-tests/Release-identities-v1.json'),
    scope='Release inherited nonpass comparison separately from39 Debug crosswalk; no missing fullRelease claim.'))
original=load(HERE/'entry/source-preimages-v3.json')['files'];source=load(HERE/'matrix/source-freeze-v2.json');changed={r['before']['path']:r['after'] for r in source['source_delta']}
for r in original:assert pin(r['path'])==changed.get(r['path'],r)
for r in source['new_tests']:assert pin(r['path'])==r
owner=load(HERE/'entry/authentication-v3.json')['preexisting_owner_edit'];assert pin(owner['path'])==owner
index=pin('.git/index');expected=load(HERE/'entry/authentication-v3.json')['index'];assert index==expected
changed_names=git('diff','--name-only').decode().splitlines();allowed=set(changed)
assert set(changed_names)==allowed
untracked=git('ls-files','--others','--exclude-standard').decode().splitlines()
assert all(p.startswith('evidence/04A40/') or p in [r['path'] for r in source['new_tests']]+['planning/BESHStatNG_CrossPlatform_MasterPlan2.15.md'] for p in untracked)
assert not git('diff','--check')
save('final/git-final-v1.json',dict(utc=utc(),branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),
  merge_base=git('merge-base','HEAD','main').decode().strip(),accepted39_merge='f2eb060c9c422c1fec4e82989d07907dbd4b9337',pre39=PREVIOUS,index=index,staged_empty=not bool(git('diff','--cached','--name-only')),diff_check_exit=0,
  exact_commands=['git -c safe.directory=<REPO> --no-optional-locks branch --show-current','git ... rev-parse HEAD','git ... rev-parse main','git ... merge-base HEAD main','git ... diff --cached --name-only','git ... diff --check','git ... status --short --untracked-files=all'],
  modified=changed_names,untracked=untracked,owner_metadata_unchanged=owner,commit_merge_stage_rebase_push_release=False))
private=[]
for p in sorted(PRIVATE.rglob('*')):
    if not p.is_file():continue
    rel=p.relative_to(PRIVATE).parts
    if rel[0].startswith('publication') or rel[0] in ['builds','readback-private']:continue
    if rel[0]=='executions' and p.suffix not in ['.json','.trx','.log']:continue
    if p.suffix.lower() in ['.json','.jsonl','.trx','.log','.tsv','.csv','.txt','.vb','.cs','.py','.ps1']:private.append(pin(p))
save(PRIVATE/'new-authoritative-private-pins-v1.json',dict(files=private,scope='All new task raw receipts/logs/TRX/cases/observations/source preimages/scripts/typed compatibility captures. Build intermediate/object trees and redundant sandbox binaries retained physically, not claimed as an exhaustive new pin union. No historical TestResults walk.'))
save('preservation/restricted-new-raw-inventory-v1.json',dict(private_inventory=pin(PRIVATE/'new-authoritative-private-pins-v1.json'),files=len(private),bytes=sum(r['bytes'] for r in private),
    raw_public_artifact=pin(raw),protected_exact_copy=pin(target),safe_counterpart=pin(HERE/'baseline/Debug-build-review-v2.json'),
    transport_exclusions=['Raw managed/Python stdout/stderr/TRX/tracebacks','Owner context and token fingerprints','Build intermediates and binary sandbox copies','Live Git index bytes','Historical ignored TestResults payload'],
    typed_compatibility_exception='Synthetic frozen input JSONL and exact typed original/current TSV may be included only after original physical scanner gate; full error stack traces remain excluded.',
    historical_scope='468 originals preserved, four already-committed owner-authorized LF renderings with exact original copies.8984 protected listed paths checked incrementally.37484/154 W3 union inherited qualified, not freshly re-audited.',
    original_file_id_continuity='NOT_VERIFIED;precommit tracked-path file IDs changed, bytes/copies authenticated. No ID baseline rewritten.'))
print('Final actual command/Git/raw-preservation receipts written',flush=True)
