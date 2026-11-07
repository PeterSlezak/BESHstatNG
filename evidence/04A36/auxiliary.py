"""Isolated nonmanaged validation; existing historical wrappers are not mutated."""
from run import m,ROOT
import sys
r=m.Runner(ROOT/'.test-rationalization-04A36.tmp');action,label=sys.argv[1:3]
if action=='source':
    results=[]
    for suffix,cwd,args in [('office-node','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),('wasm-node','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:results.append(r.command(label+'-'+suffix,args,cwd=ROOT/cwd))
    for suffix,path in [('tools','BESHstatNG/tools/tests'),('help','BESHStatNG_Help_MkDocs/tests'),('scripts','scripts/tests')]:results.append(r.command(label+'-python-'+suffix,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p','test_*.py','-v']))
    name=label+'-semantic';results.append(r.test('Debug',label,'Common',name,'FullyQualifiedName~MigrationSemantic04A02Tests'))
    results.append(r.command(label+'-common-python',[sys.executable,'-B','-m','unittest','discover','-s','BESHStat.Excel.Common.Tests/src','-p','test_*.py','-v'],env={'BESHSTAT_04A02_G3_CANDIDATES':str(r.run/'executions'/name/'observations')}))
    for batch in ['34','35']:results.append(r.command(label+'-accepted'+batch+'-audits',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A'+batch,'-p','test_audit.py','-v']))
elif action=='audits':results=[r.command(label+'-packet-audits',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A36','-p','test_audit.py','-v'])]
elif action=='help-recheck':results=[r.command(label+'-help-serial-recheck',[sys.executable,'-B','-m','unittest','discover','-s','BESHStatNG_Help_MkDocs/tests','-p','test_mkdocs_htaccess.py','-v'])]
elif action=='focused36':
    config=sys.argv[3] if len(sys.argv)>3 else 'Debug'
    results=[r.test(config,label,s,label+'-'+config+'-new36-'+s,'FullyQualifiedName~Graphics36') for s in ['Core','Common','Legacy']]
elif action=='captures':
    results=[]
    for config in ['Debug','Release']:
        for suffix,script in [('geometry','capture.ps1'),('presentation','capture-presentation-v2.ps1'),('axis','capture-axis-v2.ps1')]:
            args=['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','evidence/04A36/'+script,'-Label',label,'-Configuration',config,'-Output','post-edit-'+suffix+'-'+config+'.csv']
            if suffix!='geometry':args+=['-Post']
            results.append(r.command(label+'-'+config+'-capture-'+suffix,args))
elif action=='contracts':
    results=[]
    for suffix,args in [('inventory',['tools/verify_stage304_inventory.py']),('catalog',['BESHstatNG/tools/validate_methods_catalog.py']),('metadata',['BESHstatNG/tools/generate_analysis_metadata.py','--check']),('resolver-check',['BESHStatNG_Help_MkDocs/generate_help_resolver_routes.py','--check']),('help-check',['BESHstatNG/tools/generate_help_links.py','--methods-yml','BESHStatNG_Help_MkDocs/docs/_meta/methods.yml','--project-dir','BESHstatNG','--check']),('office-audit',['BESHstatNG/tools/audit_transitional_officejs.py'])]:results.append(r.command(label+'-'+suffix,[sys.executable,'-B',*args]))
else:raise ValueError(action)
name=label+'-'+(config+'-' if action=='focused36' else '')+action+'.json'
m.save(r.run/name,results)
sys.exit(1 if m.failed(results) else 0)
