from common import *
s=(HERE/'test_readers.py').read_text(encoding='utf-8');assert '\\r\\n' in s;s=s.replace('\\r\\n','\\n')
with (HERE/'test_readers_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/deep-mutation-fixture-v1.json',dict(status='29PASS_1INVALID_FIXTURE_FAIL_RETAINED',error='ALL-order mutation assumed CRLF but the protected existing Common file uses LF; bytes were unchanged, so intended reader condition was not reached',correction='V2 fixture uses actual retained LF. No production/source/original-output normalization occurred; first failed control is not counted as a passing adverse test.'))
