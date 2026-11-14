"""Single fresh Node or serial strict/profile/MkDocs packet."""
from common39 import *
from collections import Counter
import re

source_lock();mode=sys.argv[1]
if mode=='node':
    previous={r['suite']:r for r in load(W3/'python-node-scanners/node-v1.json')['suites']};rows=[]
    for suite,cwd,args in [('office-node','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),
        ('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),
        ('wasm-node','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:
        receipt=command('node-'+suite,args,cwd=ROOT/cwd)
        folder=PRIVATE/'commands'/('a39v1-node-'+suite)
        text=(folder/'stdout.log').read_bytes().decode('utf-8-sig')
        names=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)\r?$',text,re.M|re.S)
        identities=[sha(x.encode()) for x in names]
        count=lambda label:int(re.findall(r'(?:# |\u2139 )'+label+r' (\d+)',text)[-1])
        row=dict(suite=suite,identities=identities,tests=count('tests'),passed=count('pass'),failed=count('fail'),
            identity_multiset_exact=Counter(identities)==Counter(previous[suite]['identities']),command=receipt,
            stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'))
        save('python-node-scanners/node-'+suite+'-v1.json',row);rows.append(row)
        assert row['identity_multiset_exact'] and len(identities)==row['tests']==row['passed'] and not row['failed'] and receipt['exit_code']==0
    save('python-node-scanners/node-v1.json',dict(status='FRESH_04A39',suites=rows,passed=sum(x['passed'] for x in rows)))
    assert sum(x['passed'] for x in rows)==916
elif mode=='serial':
    results=runner().serial('a39serial');rows=[]
    for name,result in zip(['strict','profile','docs'],results):
        folder=PRIVATE/'commands'/('a39serial-'+name)
        rows.append(dict(category=name,result=result,stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),
            disposition='INHERITED_RESTRICTED;EXPLICIT_OWNER_DECISION_REQUIRED' if result['exit_code'] else 'VERIFIED_PASS'))
    save('python-node-scanners/serial-v1.json',dict(results=rows,prior_exits=[1,1,0],
        source_backed_dispositions='Strict NormalPlot identity ambiguity and profile source-contract failures remain real nonpasses, not scientific/native release.'))
else:raise ValueError(mode)
source_lock()
