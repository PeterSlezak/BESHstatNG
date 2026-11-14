"""Compile current production UDF source with the frozen native test boundary.

POST_EDIT_CURRENT only. This cannot create or overwrite a reference oracle.
"""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
from run import ROOT,HERE,PRIVATE,m
from preflight import pin,save


def main():
    label,config,version=sys.argv[1:4]
    folder=PRIVATE/('udf-current-'+label+'-'+config+'-'+version);folder.mkdir(exist_ok=False)
    build=PRIVATE/'builds'/label/config/'legacy'
    for p in build.glob('*.dll'):
        if p.name not in ['BESHStatNG.Test.dll','BESHStat.Core.Tests.dll']:shutil.copy2(p,folder/p.name)
    assert (folder/'BESHStatNG.dll').exists()
    snapshots=json.loads((HERE/'preedit/entry-source-snapshot.json').read_bytes())['files']
    sources=[ROOT/r['source']['path'] for r in snapshots]
    sources += [ROOT/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',ROOT/'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb']
    sources += [HERE/'preedit/native-safety'/name for name in ['NativeBoundary38.vb','FixtureInputs38.vb','DependencySlices38.vb']]
    locator=os.path.join(os.environ['ProgramFiles(x86)'],'Microsoft Visual Studio/Installer/vswhere.exe')
    compiler=subprocess.check_output([locator,'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/vbc.exe'],text=True).splitlines()[0]
    facade=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
    refs=[folder/name for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[facade]
    args=[compiler,'/nologo','/target:library','/rootnamespace:BESHStatNG','/optioncompare:text','/optioninfer+','/optionexplicit+',
        '/imports:System,System.Collections,System.Collections.Generic,System.Diagnostics,System.Linq,System.Globalization',
        '/optimize'+('+' if config=='Release' else '-'),'/out:'+str(folder/'BESHStat.Core.Tests.dll'),
        '/reference:System.Core.dll','/reference:System.dll','/reference:System.Data.dll','/reference:System.Xml.Linq.dll',
        *['/reference:'+str(p) for p in refs],*map(str,sources)]
    result=m.Runner(PRIVATE).command('current-clone-'+label+'-'+config+'-'+version,args);assert result['exit_code']==0
    save('compatibility/'+label+'/'+config+'/current-clone-'+version+'.json',dict(status='POST_EDIT_CURRENT_SOURCE_NOT_ORACLE',configuration=config,
        source_files=[pin(p.relative_to(ROOT).as_posix()) for p in sources],
        library_sha256=hashlib.sha256((folder/'BESHStat.Core.Tests.dll').read_bytes()).hexdigest(),
        library_bytes=(folder/'BESHStat.Core.Tests.dll').stat().st_size,native='NOT_RUN; frozen explicit native test doubles',scientific='NOT_GOLDEN'))
    print('Compiled current',label,config,'source; not a reference capture')


if __name__=='__main__':main()
