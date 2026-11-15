from common40_v3 import *
import platform,locale
sys.path.append(str(ROOT/'evidence/04A38'));from baseline_report import clean
lock();records=[]
for label,args in [('dotnet-info',['dotnet','--info']),('node-version',['node','--version']),('msbuild-version',[runner().msbuild,'/nologo','/version'])]:
    r=command('final-'+label,args);assert r['exit_code']==0
    folder=PRIVATE/'commands'/('a40-final-'+label);records.append(dict(label=label,command=clean(r),stdout=clean((folder/'stdout.log').read_text(encoding='utf-8-sig')),raw_stdout=pin(folder/'stdout.log')))
code="[pscustomobject]@{Culture=[Globalization.CultureInfo]::CurrentCulture.Name;UICulture=[Globalization.CultureInfo]::CurrentUICulture.Name;OS=[Environment]::OSVersion.VersionString;PowerShell=$PSVersionTable.PSVersion.ToString()} | ConvertTo-Json"
r=command('final-os-culture',['powershell','-NoProfile','-NonInteractive','-Command',code]);assert r['exit_code']==0
context=load(PRIVATE/'commands/a40-final-os-culture/stdout.log')
save('entry/tool-runtime-final-v1.json',dict(utc=utc(),mode='END_AUTHENTICATED;ACTUAL_BUILD_COMMANDS_LOGGED;NO_FABRICATED_PRELAUNCH_RECEIPT',commands=records,Windows=context,
    Python=dict(version=sys.version,implementation=platform.python_implementation(),platform=platform.platform(),locale=locale.getlocale(),executable_name=Path(sys.executable).name,executable_sha256=sha(Path(sys.executable).read_bytes())),
    dependency_inventory=pin(PRIVATE/'python-input-freeze-v2.json'),test_cultures=['InvariantCulture','sk-SK'],source_of_other_compatibility_cultures='Unchanged genuine frozen input rows and recorders, not invented combinations.'))
print('Actual SDK/Node/MSBuild/Python/OS/culture/dependency context recorded',flush=True)
