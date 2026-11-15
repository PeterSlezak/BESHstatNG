from common42 import *
import sys
lock();action=sys.argv[1]
if action=='freeze':
    build=load(PRIVATE/'wave-b3-build-v1.json');pins=runner().__class__.build.__globals__['source_pins']();assert pins==build['source_pins'] and not any(build['exit_codes'])
    entry=load(HERE/'entry/source-preimages-v1.json');delta=[dict(before=r,after=pin(r['path'])) for r in entry['files'] if pin(r['path'])!=r];assert len(delta)==8
    save('matrix/source-freeze-v2.json',dict(utc=utc(),source_pins=pins,files=[pin(p) for p in sorted(pins)],source_delta=delta,new_tests=[pin('BESHStat.Excel.Common.Tests/src/UdfMultivariate42Tests.vb'),pin('BESHStat.Excel.Common.Tests/src/UdfPayload42Tests.vb')],plan=pin('planning/BESHStatNG_CrossPlatform_MasterPlan.md'),plan_snapshot=pin('planning/BESHStatNG_CrossPlatform_MasterPlan2.18.md'),supersedes_failed=pin(HERE/'matrix/source-freeze-v1.json'),correction=pin(HERE/'matrix/failed-freeze-v1-disposition.json'),no_runtime_or_test_source_changes_after_freeze=True))
    for old,new in [('final-managed-v1.py','final-managed-v2.py'),('compiled-and-fixtures-v1.py','compiled-and-fixtures-v2.py'),('compatibility-v1.py','compatibility-v2.py'),('reconcile-managed-v1.py','reconcile-managed-v2.py')]:
        s=(HERE/old).read_text(encoding='utf-8-sig')
        if old=='final-managed-v1.py':
            a=s.index("    entry=load(");b=s.index('    suites=',a);s=s[:a]+s[b:]
            s=s.replace("load(PRIVATE/'wave-b2-build-v1.json')","load(PRIVATE/'wave-b3-build-v1.json')").replace('a42-wave-b2-v1','a42-wave-b3-v1').replace('a42-final-v1','a42-final-v2').replace('a42-final-','a42-final2-').replace('build-and-tests/'+"'",'build-and-tests/'+"'")
            s=s.replace("+'-executed-v1.json'","+'-executed-v2.json'").replace("'final-Release-build-v1.json'","'final-Release-build-v2.json'")
        elif old=='compiled-and-fixtures-v1.py':
            s=s.replace('a42-wave-b2-v1','a42-wave-b3-v1').replace('a42-final-v1','a42-final2-v2').replace("PRIVATE/'compiled'","PRIVATE/'compiled-v2'").replace("PRIVATE/'abi-cil'","PRIVATE/'abi-cil-v2'").replace("'compiled-method-differences-'","'compiled-method-differences2-'").replace("'-proof-v1.json',dict","'-proof-v2.json',dict").replace("'fixture-final-'","'fixture-final2-'")
            s=s.replace("+'-41-v1.json'","+'-41-v2.json'").replace("+'-39-v1.json'","+'-39-v2.json'").replace("+'-40-v1.json'","+'-40-v2.json'").replace("+'-42-v1.json'","+'-42-v2.json'")
        elif old=='compatibility-v1.py':
            s=s.replace('compatibility-current-v1','compatibility-current-v2').replace("PRIVATE/'compiled'","PRIVATE/'compiled-v2'").replace("'compatibility-'+config+'-driver-v1.py'","'compatibility-'+config+'-driver-v2.py'").replace("'compatibility-'+config+'-driver-provenance-v1.json'","'compatibility-'+config+'-driver-provenance-v2.json'").replace("'case-identities-'+config+'-v1.json'","'case-identities-'+config+'-v2.json'").replace("'compatibility/'+config+'-v1.json'","'compatibility/'+config+'-v2.json'")
        else:
            s=s.replace("+'-executed-v1.json'","+'-executed-v2.json'").replace("+'-identities-v1.json'","+'-identities-v2.json'").replace("config+'-nonpass-diagnostics-v1.json'","config+'-nonpass-diagnostics-v2.json'")
        s=s.replace('matrix/source-freeze-v1.json','matrix/source-freeze-v2.json')
        with (HERE/new).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
    print('Corrected source freeze v2 and successor campaigns prepared',flush=True)
