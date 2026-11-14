"""One fresh Node campaign or one serial strict/profile/MkDocs campaign."""
from common import *
import re
from collections import Counter

lock()
mode=sys.argv[1]
if mode=='node':
    old={r['suite']:r for r in load(W2/'continuation-v17/node-identities-v1.json')['suites']}
    rows=[]
    for suite,cwd,args in [('office-node','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),
                           ('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),
                           ('wasm-node','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:
        result=command('node-'+suite,args,cwd=ROOT/cwd)
        folder=RUNROOT/'commands'/('w3v1-node-'+suite)
        text=(folder/'stdout.log').read_bytes().decode('utf-8-sig')
        names=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)\r?$',text,re.M|re.S)
        hashes=[sha(v.encode()) for v in names]
        count=lambda label:int(re.findall(r'(?:# |\u2139 )'+label+r' (\d+)',text)[-1])
        row=dict(suite=suite,identities=hashes,tests=count('tests'),passed=count('pass'),failed=count('fail'),
                 identity_multiset_exact=Counter(hashes)==Counter(old[suite]['identities']),command=result,
                 stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'))
        rows.append(row)
        save('python-node-scanners/node-'+suite+'-v1.json',row)
        assert row['identity_multiset_exact'] and len(hashes)==row['tests']==row['passed'] and not row['failed'] and result['exit_code']==0
    save('python-node-scanners/node-v1.json',dict(status='FRESH_W3_EXECUTION',suites=rows,passed=sum(r['passed'] for r in rows)))
    assert sum(r['passed'] for r in rows)==916
elif mode=='serial':
    audit=ROOT/'scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll'
    results=m.Runner(RUNROOT).serial('w3v1-serial')
    rows=[]
    for name,result in zip(['strict','profile','docs'],results):
        folder=RUNROOT/'commands'/('w3v1-serial-'+name)
        rows.append(dict(category=name,result=result,stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),
                         disposition='INHERITED_RESTRICTED;EXPLICIT_OWNER_DECISION_REQUIRED' if result['exit_code'] else 'VERIFIED_PASS'))
    save('python-node-scanners/serial-v1.json',dict(results=rows,audit_assembly=pin(audit),
          prior_exits=load(ACCEPTED/'validation-v1.json')['strict_profile_MkDocs'],
          NormalPlot='Preserve actual strict ambiguity; no gate waiver or source normalization'))
else:raise ValueError(mode)
lock()
