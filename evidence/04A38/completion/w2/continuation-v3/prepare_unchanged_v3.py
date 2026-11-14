from common import *
s=(HERE/'verify_unchanged_v2.py').read_text(encoding='utf-8')
old="native=next(m for m in now['methods'] if m['declaring_type']=='BESHStatNG.UdfScalarUtilities' and m['member_name']==n);calls=[i['operand'] for i in native['instructions'] if i['op']=='call'];need(len(calls)==2 and '.AsString(' in calls[0] and '.'+n+'(' in calls[1],'Actual acquisition-before-normalization compiled order changed')"
new="""native=next(m for m in now['methods'] if m['declaring_type']=='BESHStatNG.UdfScalarUtilities' and m['member_name']==n);calls=[i['operand'] for i in native['instructions'] if i['op']=='call']
   prior=next(m for m in o['methods'] if m['declaring_type']=='BESHStatNG.UdfScalarUtilities' and m['member_name']==n);priorcalls=[i['operand'] for i in prior['instructions'] if i['op']=='call']
   need(len(calls)==len(priorcalls)==3 and '.GetObjectValue(' in calls[0] and '.AsString(' in calls[1] and '.'+n+'(' in calls[2] and calls[:2]==priorcalls[:2],'Original boxed-Object acquisition/exception prefix or normalization order changed')"""
assert old in s;s=s.replace(old,new)
with (HERE/'verify_unchanged_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/unchanged-reader-v2.json',dict(status='FAILED_TOO_NARROW_CIL_PREFIX_EXPECTATION',reader=pin(HERE/'verify_unchanged_v2.py'),error='Native compiler emits System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue before AsString in BOTH original/current. V3 explicitly requires all3 calls and exact original/current first2 rather than ignoring boxing/context.',source_or_oracle_change='NONE'))
