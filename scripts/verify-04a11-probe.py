"""Build/run the diagnostic-only managed probe offline, in fresh ignored directories."""
import importlib.util
from pathlib import Path
from xml.sax.saxutils import escape

ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('v11',ROOT/'scripts/verify-04a11.py')
v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
RUN=ROOT/'TestResults/04A-11-20261103-run01'
work=RUN/'diagnostic-probe';work.mkdir(exist_ok=False)
source=ROOT/'scripts/diagnostic-04a11-probe.cs'
refs=''.join('<Reference Include="'+p+'"><HintPath>'+escape(str(ROOT/p/'bin/Debug/netstandard2.0'/(p+'.dll')))+'</HintPath></Reference>' for p in ['BESHStat.Core','BESHStat.Excel.Common'])
project=work/'Probe.csproj'
project.write_text('''<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><PropertyGroup><OutputType>Exe</OutputType><AssemblyName>Probe</AssemblyName><TargetFrameworkVersion>v4.8</TargetFrameworkVersion><OutputPath>bin\\</OutputPath></PropertyGroup><ItemGroup><Reference Include="System"/><Reference Include="System.Core"/><Reference Include="netstandard"/><Reference Include="Microsoft.VisualBasic"/>'''+refs+'<Compile Include="'+escape(str(source))+'''"/></ItemGroup><Import Project="$(MSBuildToolsPath)\\Microsoft.CSharp.targets"/></Project>''',encoding='utf-8')
msbuild,_=v.base.tools()
assert v.base.command(RUN,'diagnostic-probe-build',[msbuild,str(project),'/t:Build','/v:minimal'])==0
assert v.base.command(RUN,'diagnostic-probe-run',[str(work/'bin/Probe.exe'),str(ROOT),str(ROOT/'evidence/04A11/defect-numerical-deltas.txt')])==0
v.base.isolation.emit(ROOT/'evidence/04A11/diagnostic-probe-provenance.json',dict(status='NOT_GOLDEN',native=False,source_sha256=v.base.isolation.sha(source),project_sha256=v.base.isolation.sha(project),
    core_sha256=v.base.isolation.sha(ROOT/'BESHStat.Core/bin/Debug/netstandard2.0/BESHStat.Core.dll'),common_sha256=v.base.isolation.sha(ROOT/'BESHStat.Excel.Common/bin/Debug/netstandard2.0/BESHStat.Excel.Common.dll'),
    output_sha256=v.base.isolation.sha(ROOT/'evidence/04A11/defect-numerical-deltas.txt'),scope='Existing Core/Common calls only. Exact CI deltas; unchanged coefficients/SE/fitted and all numeric cells in model/ANOVA/df/VIF/covariance tables. No estimator or independent scientific review.'))
