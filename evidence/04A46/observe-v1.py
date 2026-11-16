from common46 import *
lock();folder=PRIVATE/'observer-v1';folder.mkdir();locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0];framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';exe=folder/'Observe46.exe'
r=command('observer-compile-v1',[csc,'/nologo','/out:'+str(exe),*[('/r:'+str(framework/p)) for p in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']],str(HERE/'fixtures/Observe46.cs')]);assert not r['exit_code']
results=[]
for c in ['Debug','Release']:
 out=folder/(c+'.json');r=command('observer-'+c+'-v1',[str(exe),str(PRIVATE/'builds/a46-final-v1'/c/'legacy'),str(out)]);assert not r['exit_code'];d=load(out);assert len(d['rows'])==22;results.append(dict(configuration=c,rows=d['rows'],raw=pin(out)))
assert results[0]['rows']==results[1]['rows']
save('fixtures/observer-v1.json',dict(kind='SOURCE_DERIVED_FAKE_SOFTWARE_NOT_NATIVE_NOT_GOLDEN',source=pin(HERE/'fixtures/Observe46.cs'),results=results,case_count_per_configuration=22,methods=4,original_production_source_unchanged=True))
d=load(HERE/'scope/coverage-v1.json')
for r in d['members']:
 if r['classification']=='SOURCE_AND_COMPILED_ONLY':r.update(classification='OFFLINE_FAKE_EXECUTED',evidence='evidence/04A46/fixtures/observer-v1.json',proof='Four remaining copied option/format helpers invoked only on closed fake projection types;22 observed/asserted positive/boundary/rejection cases per configuration.')
from collections import Counter
d.update(counts=dict(Counter(r['classification'] for r in d['members'])),additional_fake_observer_cases_per_configuration=22,observer_is_not_new_managed_identity=True,initial_coverage=pin(HERE/'scope/coverage-v1.json'))
save('scope/coverage-v2.json',d)
print('22 observed source option/format cases/config exact;four helper gaps accounted without changing frozen source',flush=True)
