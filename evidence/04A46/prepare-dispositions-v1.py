from common46 import *
lock();s=(ROOT/'evidence/04A45/disposition-python-v1.py').read_text(encoding='utf-8-sig').replace('common45','common46').replace('evidence/04A44/final/git-final-v1.json','evidence/04A45/final/git-final-v1.json').replace("('codex/04A-44','codex/04A-45')","('codex/04A-45','codex/04A-46')")
s=s.replace(";assert len(changes)==15",";assert len(changes)>0").replace('ALL15_CHANGED_DIAGNOSTICS_EXACTLY_ACCOUNTED_NO_PARENT_TRANSITIONS','ALL_CHANGED_DIAGNOSTICS_EXACTLY_ACCOUNTED_NO_PARENT_TRANSITIONS').replace('15 exact diagnostic argument changes','exact diagnostic argument changes')
with (HERE/'disposition-python-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
