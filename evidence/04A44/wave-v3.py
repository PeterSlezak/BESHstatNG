from common44 import *
import shutil
lock();wave=sys.argv[1];label='a44-wave-'+wave+'-v3';r=runner().build('Debug',label);save(PRIVATE/('wave-'+wave+'-build-v3.json'),r);assert not any(r['exit_codes']) and r['source_unchanged']
parts=[]
for suite in ['Common','Legacy']:
 selection='|'.join('FullyQualifiedName~'+s for s in ['InfrastructurePolicy44','Preference','Help','Diagnostic','Fault','ExecutionCoordinator'])
 t=runner().test('Debug',label,suite,label+'-'+suite,selection);save(PRIVATE/(label+'-'+suite+'.json'),t);parts.append(dict(suite=suite,outcomes=t['outcomes'],raw=pin(PRIVATE/(label+'-'+suite+'.json'))));assert not t['outcomes'].get('Failed',0)
folder=PRIVATE/('wave-'+wave+'-fixtures-v3');folder.mkdir()
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(PRIVATE/'builds'/label/'Debug/legacy'/name,folder/name)
shutil.copy2(PRIVATE/'fixtures-before-v1/Observe44.exe',folder/'Observe44.exe');r=command('wave-'+wave+'-fixtures-v3',[str(folder/'Observe44.exe'),str(folder/'observations.json')]);assert r['exit_code']==0
assert load(folder/'observations.json')==load(PRIVATE/'fixtures-before-v1/observations.json')
save('waves/'+wave+'-v3.json',dict(build_exits=r.get('exit_codes',[]),parts=parts,current44_cases=591,exact_pre_edit_outputs_errors=True,raw=pin(folder/'observations.json'),host_actions='NOT_RUN',gates=GATES))
print('Wave',wave,'targeted tests and591 actual Windows/Common fixture outputs exact',flush=True)
