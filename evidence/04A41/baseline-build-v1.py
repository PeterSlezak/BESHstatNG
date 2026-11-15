from common41 import *
lock()
r=runner().build('Debug','a41-entry-v1')
save(PRIVATE/'baseline-Debug-build-v1.json',r)
assert not any(r['exit_codes']) and r['source_unchanged']
save('entry/baseline-Debug-build-review-v1.json',dict(kind='SANITIZED_BUILD_RECEIPT;NOT_ORIGINAL_TRANSCRIPT',configuration='Debug',exit_codes=r['exit_codes'],source_unchanged=r['source_unchanged'],raw=pin(PRIVATE/'baseline-Debug-build-v1.json'),output_directory='CONTROLLED_OWNER_LOCAL',runner=pin(ROOT/'scripts/test_suite_rationalization.py')))
print('Current-main pre-edit Debug build graph PASS',flush=True)
