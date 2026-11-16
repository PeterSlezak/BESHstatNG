from common47 import *
lock()
software=load(HERE/'final/software-verification-v1.json');software.update(additional_observer_cases_per_configuration=147,helper_observer=pin(HERE/'fixtures/helpers-observer-v2.json'),family_map=pin(HERE/'scope/family-review-v3.json'));save('final/software-verification-v2.json',software)
attempt=load(HERE/'final/failed-attempts-v1.json');attempt['attempts']=[dict(recipe=load(p).get('recipe'),type=load(p).get('type'),raw=pin(p)) for p in sorted((PRIVATE/'failed-attempts').glob('*.json'))];attempt['helper_observer']='Failed boxed-structure identity expectation retained;ByRef value-type defaults asserted in v2. Report import NameError retained;v3 resumes completed successful observer rows without rerunning heavy suites.';save('final/failed-attempts-v2.json',attempt)
for name in ['neutral_privacy_replay.py']:
 with (HERE/name).open('xb') as f:f.write((ROOT/'evidence/04A46'/name).read_bytes())
s=(ROOT/'evidence/04A46/publication46.py').read_text().replace('common46','common47').replace('publication46','publication47').replace('run46.py','run47.py').replace('reader46','reader47').replace('neutral46','neutral47').replace('READY_FOR_OWNER_04A46_SAFE_GUI_WIRING_REVIEW__NATIVE_GATE_NOT_AUTHORIZED','NOT_READY_04A47_SAFE_SOFTWARE').replace('BESHStatNG_04A46_SAFE_GUI_WIRING_REVIEW_','BESHStatNG_04A47_SAFE_LONGRUN_GUI_WIRING_REVIEW_')
s=s.replace("for population in evidence['raw_populations']:","for population in evidence['raw_populations']+evidence['accepted45_46_populations']:")
start=s.index(" exact(evidence['accepted45_whole_store'])");end=s.index(' for r in load(ROOT/\'.test-rationalization-04A39',start);s=s[:start]+s[end:]
s=s.replace('full41-45 previously','full41-46 previously').replace("'final/software-verification-v1.json'","'final/software-verification-v2.json'").replace("'scope/family-review-v2.json'","'scope/family-review-v3.json'")
s=s.replace('MasterPlan2.22.md','MasterPlan2.23.md')
s=s.replace("['final/owner-review-v1.md','final/readiness-v1.json'","['final/owner-review-v1.md','final/readiness-v1.json'")
with (HERE/'publication47.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(ROOT/'evidence/04A46/deliver-v1.py').read_text().replace('common46','common47').replace('publication46.py','publication47.py').replace('run46.py','run47.py').replace('BESHStatNG_04A46_SAFE_GUI_WIRING_REVIEW_','BESHStatNG_04A47_SAFE_LONGRUN_GUI_WIRING_REVIEW_')
with (HERE/'deliver-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
source=(ROOT/'evidence/04A24/frozen-capture-hashes.json');target='evidence/04A23/multivariate-spc-same-core-comparisons-v1.json'
save('entry/missing-payload-authority.review.json',dict(authority=pin(source),original_record=load(source)['files'][target],target=target,actual_presence=(ROOT/target).exists(),first_current_error='FileNotFoundError',old_FAIL_unwaived=True,original_test_sources=[pin('scripts/tests/test_04a23_multivariate_spc.py'),pin('scripts/tests/test_04a24_multivariate_payload.py')],required_next_action='Owner-retained byte-exact copy;authenticate hash/size and original ignore policy before any exclusive read-only recovery. Preserve first campaign;retry only affected unchanged parents.'))
print('Prepared exact-delivery tools;readiness remains NOT_READY;native NOT_RUN',flush=True)
