"""Adverse controls modify copies, then call the actual independent register reader."""
from common39 import *
import copy
import importlib.util

spec=importlib.util.spec_from_file_location('register_reader39',HERE/'verify-register-v3.py')
reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
d=load(HERE/'register/defects-v4.json')
summary=reader.verify(ROOT,d)
controls=[]
def reject(name,mutation):
    value=copy.deepcopy(d);mutation(value)
    try:reader.verify(ROOT,value,False)
    except (AssertionError,KeyError,ValueError) as error:controls.append(dict(id=name,outcome='PASS_REJECTED',reason=str(error)[:300]))
    else:raise AssertionError('ADVERSE_NOT_REJECTED '+name)
reject('MISSING_CANONICAL_ENTRY',lambda v:v['issues'].pop(0))
reject('DUPLICATE_CANONICAL_ENTRY',lambda v:v['issues'].append(copy.deepcopy(v['issues'][0])))
reject('CHANGED_ORIGINAL_FACET',lambda v:v['issues'][0]['obligation_facets'][0]['record'].update(capture_authorized=True))
reject('LOST_ORIGINAL_FACET',lambda v:v['issues'][0]['obligation_facets'].pop(0))
reject('MISSING_RESTRICTED_CONTROL',lambda v:v['crosswalk'].remove(next(x for x in v['crosswalk'] if x['kind']=='RESTRICTED_CONTROL')))
reject('DUPLICATE_OBLIGATION_MAP',lambda v:v['crosswalk'].append(copy.deepcopy(next(x for x in v['crosswalk'] if x['kind']=='ORIGINAL_OBLIGATION'))))
reject('GATE_RELEASE',lambda v:v['issues'][0].update(remaining_gate='P=YES'))
reject('PREMATURE_04A48_EXIT',lambda v:v['issues'][0].update(exit_disposition_04A48='FIXED_AND_VERIFIED'))
reject('RESERVED_STATUS',lambda v:v['issues'][0].update(status='RETAINED_RESTRICTED'))
reject('LOST_REPORT_HISTORY',lambda v:next(x for x in v['issues'] if x['report_occurrences'])['report_occurrences'].pop(0))
reject('FALSE_DISCHARGE',lambda v:v.update(no_discharge=False))
save('register/validation-v3.json',dict(summary=summary,reader=pin(HERE/'verify-register-v3.py'),controls=controls))
print('Register validation/adverse controls:',summary,len(controls),flush=True)
