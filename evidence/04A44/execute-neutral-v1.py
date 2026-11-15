from common44 import *
r=command('neutral-scanner-replay-v1',[sys.executable,'-I','-B',str(HERE/'neutral_privacy_replay.py'),str(ROOT)]);assert r['exit_code']==0
m=module('neutral44',HERE/'neutral_privacy_replay.py');save('privacy/neutral-replay-v1.json',dict(m.run(ROOT),source=pin(HERE/'neutral_privacy_replay.py'),raw_command=r))
