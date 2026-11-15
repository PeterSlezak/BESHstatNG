from common43 import *
lock();config=sys.argv[1];result=runner().build(config,'a43-entry-v1');save(PRIVATE/('entry-'+config+'-build-v1.json'),result)
assert not any(result['exit_codes']) and result['source_unchanged']
print(config,'entry nine-step graph PASS',flush=True)
