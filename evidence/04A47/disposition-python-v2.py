from common47 import *
lock();s=(HERE/'disposition-python-v1.py').read_text()
needle=" r=dict(row)\n if r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW':"
replacement=""" r=dict(row)
 if r['classification'] in ['NEW_NONPASS','MISSING_NONPASS_EVENT']:
  pair=pairs[r['stable_event_id']]
  if r['classification']=='NEW_NONPASS':
   assert pair['current']['outcome']=='ERROR' and 'FileNotFoundError' in pair['current']['traceback'] and 'multivariate-spc-same-core-comparisons-v1.json' in pair['current']['message']
  else:assert pair['before']['outcome']=='FAIL' and pair['current'] is None
  r.update(disposition='MISSING_HISTORICAL_SPC23_PAYLOAD_ERROR_AND_PRIOR_FAIL_RETAINED_UNWAIVED',proof=dict(missing_path='evidence/04A23/multivariate-spc-same-core-comparisons-v1.json',source_test_unchanged=True,assertions_modified=False,raw_error_preserved=True,prior_FAIL_not_fixed_or_waived=True,search='evidence/04A47/preservation/missing-fixture-search-v1.json')))
  rows.append(r);continue
 if r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW':"""
assert needle in s;s=s.replace(needle,replacement)
s=s.replace("assert not d['summary']['changed_parents'] and len(d['parents'])==956","assert len(d['summary']['changed_parents'])==2 and len(d['parents'])==956")
s=s.replace("status='ALL_CHANGED_DIAGNOSTICS_EXACTLY_ACCOUNTED_NO_PARENT_TRANSITIONS'","status='DIAGNOSTICS_ACCOUNTED_TWO_MISSING_PAYLOAD_PARENT_TRANSITIONS_NOT_READY'")
s=s.replace("changed_parents=[],parents=956","changed_parents=d['summary']['changed_parents'],readiness_blocker='MISSING_HISTORICAL_SPC23_COMPARISON_PAYLOAD',parents=956")
s=s.replace("print('Python956:834PASS/111FAIL/11ERROR;exact diagnostic argument changes;zero parent transitions;all historical FAILs unwaived',flush=True)","print('Python956:833PASS/110FAIL/13ERROR;two missing-payload transitions retained;diagnostics accounted;NOT_READY',flush=True)")
exec(compile(s,str(HERE/'disposition-python-v1.py'),'exec'))
