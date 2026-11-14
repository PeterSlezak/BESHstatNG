from common import *
from verify_ownership import inspect as ownership
from verify_preservation import inspect as preservation
from verify_derivative import inspect as derivative
own=ownership()
save('independent-ownership-proof.json',own)
pres=preservation()
save('preservation-final.json',pres)
summary=json.loads((HERE/'ownership-summary.json').read_bytes())
validation=json.loads((HERE/'validation.json').read_bytes())
fragments=json.loads((HERE/'fragment-coverage.json').read_bytes())
mandatory=dict(historical_OPEN=own['historical']['OPEN'],current_OPEN=own['current']['OPEN'],distinct_support_OPEN=own['support']['distinct_OPEN'],
    current_fragment_OPEN=fragments['current']['total']-fragments['current']['covered'],
    original_fragment_OPEN=fragments['original']['total']-fragments['original']['covered'])
need_ready=all(count==0 for count in mandatory.values())
assert not need_ready,'This partial reader cannot authorize W3; genuine zero OPEN requires the separately specified readiness verifier'
save('readiness.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',mandatory_blockers=mandatory,
    ownership=summary,validation=validation,new_original_queries=0,owner_decision_needed=False,
    gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'),
    decision='Critical statuses independently recomputed by verify_slice.py and verify_ownership.py; nonzero mandatory OPEN prohibits W3. No READY verifier or owner acceptance claimed.'))
git_records=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),
                  ('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
    result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
    assert result.returncode==0
    git_records.append(dict(check=name,exit_code=result.returncode,stdout=result.stdout.decode('utf-8'),stderr=result.stderr.decode('utf-8')))
save('git-closeout.json',dict(records=git_records,branch='codex/04A-38',HEAD=BASE,index='EMPTY',diff_check_exit=0,
    source_changes=[r['path'] for r in json.loads((HERE/'source-transitions.json').read_bytes())['files']],
    scope='Four previously modified source/test files plus one new Common source; earlier uncommitted work retained. All4695 tracked paths independently verified. No index/branch/history operations.'))
records=[]
for path in sorted((RUNROOT/'commands').glob('w2c5-*/command.json')):
    records.append(dict(label=path.parent.name,receipt=clean(json.loads(path.read_bytes()))))
save('command-receipts.json',dict(records=records,raw_logs='Retained locally in isolated Runner command directories; excluded from publication copy',
    post_seal='Export/live/copied reader commands recorded in the external post-seal receipt to avoid manifest recursion'))
print(json.dumps(dict(readiness='NOT_READY_04A38',blockers=mandatory,managed=validation['managed_Debug']['outcomes'],Python=validation['Python'])))
