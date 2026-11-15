from common44 import *
import stat
path=HERE/'privacy/neutral-replay-v1.json';relative=path.relative_to(ROOT).as_posix();assert not git('ls-files','--',relative).strip()
assert relative not in {r['path'] for r in load(PRIVATE/'tracked-entry-inventory-v1.json')['files']}
assert path.is_file() and path.stat().st_mode&stat.S_IWRITE
original=pin(path);data=load(path);target=PRIVATE/'neutral-replay-raw-v1.json'
with target.open('xb') as f:f.write(path.read_bytes())
assert pin(target)['sha256']==original['sha256'] and pin(target)['bytes']==original['bytes'];target.chmod(stat.S_IREAD)
command_record=data.pop('raw_command');assert command_record['exit_code']==0
data.update(kind='VERSIONED_SANITIZED_NEUTRAL_REPLAY_RECEIPT;NOT_RAW_COMMAND_OUTPUT',raw_original=original,protected_copy=pin(target),execution=dict(exit_code=command_record['exit_code'],seconds=command_record['seconds'],stdout_sha256=command_record['stdout_sha256'],stderr_sha256=command_record['stderr_sha256']),transformation='Remove absolute command argv/cwd from new unprotected untracked44 report. Exact original retained locally before Git-facing correction;no old historical/protected record changed.')
save('privacy/neutral-replay-v2.json',data)
assert path.resolve().is_relative_to(HERE.resolve()) and pin(path)==original
path.unlink() # Only own newly generated,untracked,unprotected erroneous44 presentation;exact raw copy already pinned/read-only.
save('privacy/new-report-presentation-crosswalk-v1.json',dict(original=original,protected_copy=pin(target),representation=pin(HERE/'privacy/neutral-replay-v2.json'),status='RAW_PRIVACY_FAILURE_RETAINED_LOCALLY;SANITIZED_CANDIDATE_SUCCESSOR',authority='Current44 owner authorization to protect new raw reports and publish versioned sanitized representations;not broader historical evidence mutation.',prior_protected_evidence_changed=False))
print('Own new untracked raw replay receipt preserved exactly outside candidate;sanitizedv2 provenance created',flush=True)
