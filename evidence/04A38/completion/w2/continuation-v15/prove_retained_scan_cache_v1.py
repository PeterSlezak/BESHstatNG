"""Adverse controls and original exception equivalence using fresh synthetic files."""
from common import *
import copy,importlib.util
from retained_scan_cache_v1 import validated_rows
sys.path.append(str(W2/'continuation-v6'))
fixture=PRIVATE/'retained-cache-controls-v1';fixture.mkdir(exist_ok=False);(fixture/'scripts').mkdir()
for name in ['13','14']:
 target=fixture/'scripts'/('verify-04a'+name+'-publication.py')
 with target.open('xb') as f:f.write((ROOT/'scripts'/target.name).read_bytes())
with (fixture/'safe.txt').open('xb') as f:f.write(b'plain synthetic data\n')
with (fixture/'bad.bin').open('xb') as f:f.write(b'\xbe\x00')
spec=importlib.util.spec_from_file_location('original_fixture_policy',fixture/'scripts/verify-04a13-publication.py');original=importlib.util.module_from_spec(spec);spec.loader.exec_module(original)
safe=original.scan(['safe.txt']);errors=[]
try:original.scan(['safe.txt','bad.bin'])
except UnicodeDecodeError as e:errors.append((type(e).__name__,str(e)))
else:raise AssertionError('Original synthetic binary did not raise')
hook=dict(ROOT=fixture,pin=pin,sha=sha,os=os,Path=Path)
exec(compile((HERE/'scanner-hook-ast-v1.py').read_text(),'EXACT_C6_PREFIX_SYNTHETIC_EXCEPTION_CONTROL','exec'),hook)
spec=importlib.util.spec_from_file_location('cached_fixture_policy',fixture/'scripts/verify-04a13-publication.py');cached=importlib.util.module_from_spec(spec);spec.loader.exec_module(cached)
key=(hook['authority'],str(fixture),os.environ.get('COMPUTERNAME'),'safe.txt',sha((fixture/'safe.txt').read_bytes()));hook['cache'][key]=safe
try:cached.scan(['safe.txt','bad.bin'])
except UnicodeDecodeError as e:errors.append((type(e).__name__,str(e)))
else:raise AssertionError('Cached sequence translated/omitted the original binary error')
assert len(errors)==2 and errors[0]==errors[1] and hook['stats']['reused']==1 and len(hook['cache'])==1
binding=json.loads((PREVIOUS/'publication-git-index-binding-v1.json').read_bytes());failed=binding['snapshot'];base=PRIVATE/'python-v1/1-owner-replacement-v1/parallel-scan-1/1';plan=json.loads((base/'input.json').read_bytes());report=json.loads((base/'raw-result.json').read_bytes());controls=[]
for label,mutate in [('changed authority',lambda r:r['authority'].update(scan_ast_sha256='0'*64)),('dropped prefix row',lambda r:r['rows'].pop(0)),('duplicate prefix row',lambda r:r['rows'].append(r['rows'][0])),('changed content hash',lambda r:r['rows'][0].update(sha256='0'*64)),('false complete marker',lambda r:r.update(complete=True)),('changed exception class',lambda r:r.update(error='Different exception'))]:
 bad=copy.deepcopy(report);mutate(bad)
 try:validated_rows(bad,plan,failed)
 except (AssertionError,OSError):controls.append(dict(name=label,status='REJECTED'))
 else:raise AssertionError('Invalid retained result accepted '+label)
doc=dict(status='ORIGINAL_UNICODE_EXCEPTION_TYPE_MESSAGE_AND_ORDER_PRESERVED_WITH_EXACT_CACHE_REUSE',synthetic_original_exception=errors[0],synthetic_cached_exception=errors[1],reused=hook['stats']['reused'],failed_candidate_cached=False,controls=controls,source=pin(Path(__file__)),cache=pin(PRIVATE/'final-drafts/retained_scan_cache_v1.py'),original_scanner=pin(ROOT/'scripts/verify-04a13-publication.py'),prefix=pin(HERE/'scanner-hook-ast-v1.py'),test_assertions_changed=0,historical_scanner_calls=0,parent_tests_executed=0,original_statistical_queries=0)
with (PRIVATE/'final-drafts/retained-cache-controls-v1.json').open('x',encoding='utf-8') as f:json.dump(doc,f,indent=2)
print('Original/cache exception equality and six adverse controls PASS')
