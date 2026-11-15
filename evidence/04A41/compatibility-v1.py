from common41 import *
import types,csv,collections
lock();config=sys.argv[1];assert config in ['Debug','Release']
target=PRIVATE/'compatibility-current-v1';target.mkdir(exist_ok=True)
freeze=target/'production-test-freeze-v1.json'
if not freeze.exists():save(freeze,load(HERE/'matrix/source-freeze-v1.json'))
accepted=ROOT/'evidence/04A38/completion/w2/continuation-v17-postcorrection-v1/compatibility_Debug_v3.py'
source=accepted.read_text(encoding='utf-8');edits=[("assert config=='Debug'","assert config in ['Debug','Release']"),("for config in ['Debug']:","for config in ["+repr(config)+"]:"),("for c in ['Debug']","for c in ["+repr(config)+"]"),("run=PRIVATE/stage/config","run=PRIVATE/config"),("print('Current Debug preedit28966 + separate late458 EXACT; Release deferred',stage)","print('41 current-only29424 exact',stage)")]
for a,b in edits:assert source.count(a)==1;source=source.replace(a,b)
shared=types.ModuleType('common');shared.__dict__.update(globals());shared.HERE=target;shared.PRIVATE=PRIVATE/'compiled';shared.DEFERRED='NOT_APPLICABLE41_CONFIG_SEPARATELY_MEASURED'
shared.save=lambda p,d:save(target/p,d);shared.clean=lambda v:v
shared.library_pin=lambda p:dict(name=Path(p).name,bytes=Path(p).stat().st_size,sha256=pin(p)['sha256'])
def compiler(name):
    locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
shared.compiler=compiler;sys.modules['common']=shared
driver=PRIVATE/('compatibility-'+config+'-driver-v1.py')
with driver.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save(PRIVATE/('compatibility-'+config+'-driver-provenance-v1.json'),dict(authority=pin(accepted),driver=pin(driver),path_configuration_only_substitutions=edits,original_queries=0,assertions_weakened=False))
stage='a41-'+config+'-v1';sys.argv=[str(driver),stage]
exec(compile(source,str(driver),'exec'),dict(__name__='__main__',__file__=str(driver)))
raw=target/('compatibility-'+stage+'.json');data=load(raw);assert data['new_original_queries']==0
rows=[];corpora=[]
for r in data['results']:
    for k in ['original','current','inputs','recorder']:exact(r[k])
    original=(ROOT/r['original']['path']).read_bytes();current=(ROOT/r['current']['path']).read_bytes();assert original==current
    lines=current.decode('utf-8-sig').splitlines();header=next(csv.reader([lines[0]],delimiter='\t'));parsed=list(csv.DictReader(lines,delimiter='\t'));assert len(parsed)==r['cases']
    rows.extend(dict(corpus=r['corpus'],case_id=row['case_id'],input_sha256=row.get('input_sha256'),provenance='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in list('ABCDEFG') else 'PRE_EDIT_FROZEN',row_sha256=sha(json.dumps(row,sort_keys=True).encode()),schema=header) for row in parsed)
    corpora.append({k:r[k] for k in ['corpus','cases','exact','original','current','inputs','recorder']})
previous=load(ROOT/('evidence/04A40/compatibility/'+config+'-current-only-v4.json'))
assert rows==previous['case_identities'] and len(rows)==29424
assert collections.Counter(r['provenance'] for r in rows)==dict(PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458)
save(PRIVATE/('case-identities-'+config+'-v1.json'),dict(rows=rows))
save('compatibility/'+config+'-v1.json',dict(status='FRESH_CURRENT_ONLY_29424_EXACT',configuration=config,results=corpora,cases=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,exact_typed_order_input_schema_identities=True,case_manifest=pin(PRIVATE/('case-identities-'+config+'-v1.json')),baseline40=pin(ROOT/('evidence/04A40/compatibility/'+config+'-current-only-v4.json')),raw_driver=pin(raw),original_queries=0,scientific='NOT_GOLDEN'))
print(config,'29424 exact bytes and typed ordered identities;no original query',flush=True)
