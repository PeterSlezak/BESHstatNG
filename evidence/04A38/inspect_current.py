"""Versioned post-edit source/registration/consumer inspection; metadata only."""
import json,os,subprocess,sys
from run import ROOT,HERE,PRIVATE,m
from preflight import save,pin

def main():
 label,config=sys.argv[1:3]
 folder=HERE/'compatibility'/label/config/'inspection';folder.mkdir(parents=True,exist_ok=False)
 private=PRIVATE/('inspection-'+label+'-'+config);private.mkdir(exist_ok=False)
 r=m.Runner(PRIVATE);build=PRIVATE/'builds'/label/config/'legacy'
 locator=os.path.join(os.environ['ProgramFiles(x86)'],'Microsoft Visual Studio/Installer/vswhere.exe')
 compiler=subprocess.check_output([locator,'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
 for tool,args in [('Inspect38',[str(build),str(folder/'compiled.json'),str(HERE/'entry-members.json')]),('ConsumerClosure38',[str(build),str(HERE/'entry-members.json'),str(folder/'consumer-closure.json')])]:
  exe=private/(tool+'.exe')
  assert r.command('inspect-current-'+label+'-'+config+'-'+tool+'-compile',[compiler,'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'helpers'/(tool+'.cs'))])['exit_code']==0
  assert r.command('inspect-current-'+label+'-'+config+'-'+tool+'-read',[str(exe),*args])['exit_code']==0
 old=json.loads((HERE/'preedit'/('compiled-'+config+'.json')).read_bytes());new=json.loads((folder/'compiled.json').read_bytes())
 assert old['exports']==new['exports'],'Registration changed'
 assert len(old['exports'])==268
 save('compatibility/'+label+'/'+config+'/registration.json',dict(status='EXACT_268_REGISTRATION_CONTRACTS',configuration=config,exports=268,
  checks='Names, return types, ordered arguments/default values, complete ExcelFunction and ExcelArgument metadata and flags; compiled metadata only, native registration NOT_RUN',
  entry=pin('evidence/04A38/preedit/compiled-'+config+'.json'),current=pin((folder/'compiled.json').relative_to(ROOT).as_posix())))
 print(config,'268 complete registration contracts exact; current CIL consumers recorded')

if __name__=='__main__':main()
