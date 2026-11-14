"""Compile isolated Framework UDF metadata inspector; no restore/native execution."""
from pathlib import Path
import os
import shutil
import subprocess
import sys
from run import ROOT,HERE,PRIVATE,m

def main():
    label,config=sys.argv[1:3];attempt=sys.argv[3] if len(sys.argv)>3 else 'v1';tag='inspect-'+label+'-'+config+'-'+attempt
    out=PRIVATE/tag;out.mkdir(exist_ok=False)
    locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    compiler=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
    runner=m.Runner(PRIVATE)
    code=runner.command(tag+'-compile',[compiler,'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(out/'Inspect38.exe'),str(HERE/'helpers/Inspect38.cs')])
    assert code['exit_code']==0
    dest=HERE/('preedit/compiled-'+config+'.json' if label=='entry' else 'compatibility/compiled-'+config+'.json');dest.parent.mkdir(exist_ok=True)
    code=runner.command(tag+'-run',[str(out/'Inspect38.exe'),str(PRIVATE/'builds'/label/config/'legacy'),str(dest),str(HERE/'entry-members.json')])
    assert code['exit_code']==0

if __name__=='__main__':main()
