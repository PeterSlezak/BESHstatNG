from common40_v3 import *
lock()
run=runner();result=run.build('Debug','a40-entry-v1')
save('baseline/Debug-build-v1.json',result)
assert not any(result['exit_codes']) and result['source_unchanged']
lock()
print('Fresh current-main Debug baseline build PASS; version-generator/installer targets not run',flush=True)
