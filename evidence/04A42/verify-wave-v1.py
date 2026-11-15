from common42 import *
import shutil,re,copy
def canonical(value):
    if isinstance(value,list):return [canonical(x) for x in value]
    if isinstance(value,dict):return {k:canonical(v) for k,v in value.items()}
    if isinstance(value,str):
        value=re.sub(r'\b(PCA|FA|KMEANS|HCLUST|CA|MCA|DA|PSM):[0-9a-f]{32}\b',r'\1:<SESSION_ID>',value)
        value=re.sub(r'\b\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}Z\b','<CREATED_UTC>',value)
    return value
if __name__=='__main__':
    lock();wave=sys.argv[1];run=runner();build=run.build('Debug','a42-wave-'+wave+'-v1');save(PRIVATE/('wave-'+wave+'-build-v1.json'),build);assert not any(build['exit_codes']) and build['source_unchanged']
    r=run.test('Debug','a42-wave-'+wave+'-v1','Common','a42-wave-'+wave+'-Common-v1','FullyQualifiedName~UdfMultivariate42Tests'+('|FullyQualifiedName~UdfPayload42Tests' if wave.startswith('b') else ''));save(PRIVATE/('wave-'+wave+'-tests-v1.json'),r);assert not r['outcomes'].get('Failed',0)
    folder=PRIVATE/('fixtures-wave-'+wave+'-v1');folder.mkdir()
    for p in (PRIVATE/('builds/a42-wave-'+wave+'-v1/Debug/legacy')).glob('*.dll'):shutil.copy2(p,folder/p.name)
    shutil.copy2(PRIVATE/'fixtures-before-v1/Observe42.exe',folder/'Observe42.exe')
    r=command('fixture-wave-'+wave,[str(folder/'Observe42.exe'),str(folder/'observations.json')]);assert r['exit_code']==0
    before=load(PRIVATE/'fixtures-before-v1/observations.json');after=load(folder/'observations.json');left=canonical(before);right=canonical(after)
    diffs=[dict(id=a['id'],before=a,after=b) for a,b in zip(left['rows'],right['rows']) if a!=b];save(PRIVATE/('wave-'+wave+'-fixture-differences.json'),dict(differences=diffs));assert not diffs,'CURRENT_MAIN_BEHAVIOR_DIFFERENCE'
    save('waves/wave-'+wave+'-verification-v1.json',dict(build_exits=build['exit_codes'],tests=load(PRIVATE/('wave-'+wave+'-tests-v1.json'))['outcomes'],cases=len(left['rows']),typed_statistical_shape_error_order_exact=True,only_excluded_runtime_fields=['Generated session GUID text','Created UTC timestamp cells'],before=pin(PRIVATE/'fixtures-before-v1/observations.json'),after=pin(folder/'observations.json'),scientific='NOT_GOLDEN'))
    print('Wave',wave,'selected tests and',len(left['rows']),'typed current-main cases PASS',flush=True)
