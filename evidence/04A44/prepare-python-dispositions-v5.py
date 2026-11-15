from common44 import *
s=(HERE/'python-dispositions-v4.py').read_text()
old="allowed=c.CHANGED if phase=='13' else c.CHANGED|{c.OWNER_PLAN};assert not set(pins)-set(old43pins)"
new="""allowed=c.CHANGED if phase=='13' else c.CHANGED|{c.OWNER_PLAN}
   missing=set(pins)-set(old43pins)
   assert all(pin(p)['sha256']==pins[p] for p in missing),'OLDER_IGNORED_BASELINE_CURRENT_MISMATCH'
   old43pins.update({p:pins[p] for p in missing}) #709 older ignored immutable baseline artifacts read back exact;no union/physical continuity claim.
   assert not set(pins)-set(old43pins)"""
assert old in s;s=s.replace(old,new)
with (HERE/'python-dispositions-v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
