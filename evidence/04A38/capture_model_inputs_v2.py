"""Versioned runner; preserves the invalid v1 matrix and command receipts."""
import os
import subprocess
import sys
from run import HERE,PRIVATE,m


def main():
    config,reference_version,attempt=sys.argv[1:4]
    tag='model-inputs-v2-'+config+'-'+attempt
    private=PRIVATE/tag;private.mkdir(exist_ok=False)
    folder=HERE/'preedit/attempts/model-inputs-v2'
    reference=PRIVATE/('udf-clone-entry-'+config+'-'+reference_version)
    executable=reference/('ModelInputCapture38-v2-'+attempt+'.exe')
    locator=os.path.join(os.environ['ProgramFiles(x86)'],'Microsoft Visual Studio/Installer/vswhere.exe')
    compiler=subprocess.check_output([locator,'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
    facade=os.path.join(os.environ['ProgramFiles(x86)'],'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
    runner=m.Runner(PRIVATE)
    result=runner.command(tag+'-compile',[compiler,'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(reference/'BESHStat.Core.dll'),'/reference:'+facade,'/out:'+str(executable),str(folder/'Capture38.cs')]);assert result['exit_code']==0
    result=runner.command(tag+'-run',[str(executable),str(reference),str(folder/'cases.jsonl'),str(folder/('capture-'+config+'-'+attempt+'.tsv')),str(folder/('execution-'+config+'-'+attempt+'.json'))]);assert result['exit_code']==0


if __name__=='__main__':main()
