"""Record actual auxiliary commands using the accepted isolated command recorder."""
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).resolve().parent.parent))
from run import m,ROOT
runner=m.Runner(ROOT/'.test-rationalization-04A34.tmp')
mode=sys.argv[1]
if mode=='python':
    r=runner.command('review-Debug-python-scripts',[sys.executable,'-B','-m','unittest','discover','-s','scripts/tests','-p','test_*.py','-v'])
elif mode=='audit':
    r=runner.command((sys.argv[2] if len(sys.argv)>2 else 'review')+'-04A34-packet-audits',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A34','-p','test_audit.py','-v'])
elif mode=='verify':r=runner.command('review-independent-verifier',[sys.executable,'-B','evidence/04A34/verify_manifest.py'])
elif mode=='guard':r=runner.command((sys.argv[2] if len(sys.argv)>2 else 'review')+'-source-guard',['dotnet','.test-rationalization-04A34.tmp/guard3-bin/SourceGuard.dll','--history'])
elif mode=='il':r=runner.command((sys.argv[2] if len(sys.argv)>2 else 'review')+'-partial-portable-il',['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','evidence/04A34/completion/compare-entry-il.ps1'])
elif mode=='capture':
    config=sys.argv[2]
    results=[runner.command('review3-capture-'+config+'-'+suffix,['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','evidence/04A34/completion/'+file,config]) for suffix,file in [('families','capture-final.ps1'),('spc','capture-final-spc.ps1')]]
    sys.exit(1 if any(r['exit_code'] for r in results) else 0)
else:raise ValueError(mode)
sys.exit(r['exit_code'])
