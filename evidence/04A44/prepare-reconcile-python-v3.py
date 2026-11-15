from common44 import *
s=(HERE/'reconcile-python-v2.py').read_text()
s=s.replace("assert all(next(p['outcome'] for p in current if p['id']==g)=='FAIL' for g in guards)","assert all(sum(p['id']==g for p in current)==1 for g in guards) # Actual current outcomes retained,never forced to historical43 outcomes.")
s=s.replace("disposition='HISTORICAL_SOURCE_DIFF_GUARD_FAIL_RETAINED;CURRENT_AUTHORIZED_UDF_PATH_ARGUMENTS_CHANGED'","disposition='HISTORICAL_SOURCE_DIFF_GUARD_FAIL_RETAINED;CURRENT_SCOPE_EXECUTION_SEPARATELY_RECORDED'")
s=s.replace("current_outcome='FAIL'","current_outcome=next(p['outcome'] for p in current if p['id']==key[0]),historical43_outcome='FAIL'")
with (HERE/'reconcile-python-v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
