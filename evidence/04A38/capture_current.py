"""Execute frozen input/recorder contracts against current implementation only."""
import os
from pathlib import Path
import subprocess
import sys
from run import HERE,PRIVATE,m


def main():
    label,config,version,corpus,attempt=sys.argv[1:6]
    tag='current-'+label+'-'+config+'-'+corpus+'-'+attempt
    private=PRIVATE/tag;private.mkdir(exist_ok=False)
    reference=PRIVATE/('udf-current-'+label+'-'+config+'-'+version)
    if corpus=='original-v1':source=HERE/'preedit/frozen-v2';case_file=source/'cases.jsonl';recorder=source/'Capture38.cs'
    else:
        source=HERE/'preedit/attempts'/corpus;case_file=source/'cases.jsonl';recorder=source/'Capture38.cs'
        if not recorder.exists():recorder=source/'Capture38.cs.txt'
    copied=private/'Capture38.cs';copied.write_bytes(recorder.read_bytes())
    executable=reference/('CurrentCapture38-'+corpus+'-'+attempt+'.exe')
    out=HERE/'compatibility'/label/config/corpus/attempt;out.mkdir(parents=True,exist_ok=False)
    locator=os.path.join(os.environ['ProgramFiles(x86)'],'Microsoft Visual Studio/Installer/vswhere.exe')
    compiler=subprocess.check_output([locator,'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
    facade=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
    runner=m.Runner(PRIVATE)
    result=runner.command(tag+'-compile',[compiler,'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(reference/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(executable),str(copied)]);assert result['exit_code']==0
    result=runner.command(tag+'-run',[str(executable),str(reference),str(case_file),str(out/'current.tsv'),str(out/'execution-current.json')]);assert result['exit_code']==0


if __name__=='__main__':main()
