"""Retain the first external handover syntax failure and version its exact correction."""
import sys,ast
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v10'))
from common import *
old=W2/'continuation-v10-finalize-handover-v2.py';source=old.read_text(encoding='utf-8');needle="Final Git/source/protection check follows this index.'))";assert needle in source;source=source.replace(needle,"Final Git/source/protection check follows this index.')")
point="inputs=[pin(W2/n) for n in names]"
source=source.replace(point,point+"\ninputs.extend(pin(RUNROOT/'commands/w2c10-postseal-handover-v2'/n) for n in ['command.json','stdout.log','stderr.log'])\ninputs.extend([pin(W2/'continuation-v10-handover-correction-v1.json'),pin(Path(__file__))])")
ast.parse(source);new=W2/'continuation-v10-finalize-handover-v3.py'
with new.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
record=dict(first=pin(old),corrected=pin(new),command=pin(RUNROOT/'commands/w2c10-postseal-handover-v2/command.json'),stdout=pin(RUNROOT/'commands/w2c10-postseal-handover-v2/stdout.log'),stderr=pin(RUNROOT/'commands/w2c10-postseal-handover-v2/stderr.log'),exit_code=1,error='SyntaxError unmatched closing parenthesis in an external post-seal bookkeeping dictionary; no code executed and no handover/artifact output was created.',resolution='Remove exactly one closing parenthesis, independently ast.parse before execution, retain the first raw diagnostic and source; sealed source/tests/evidence/ZIP/parts are unchanged.',original_queries=0)
with (W2/'continuation-v10-handover-correction-v1.json').open('x',encoding='utf-8',newline='\n') as f:json.dump(record,f,ensure_ascii=False,indent=2);f.write('\n')
handover=command('postseal-handover-v3',[sys.executable,'-B',str(new)]);assert handover['exit_code']==0
seal=command('postseal-live-git-v3',[sys.executable,'-B',str(W2/'continuation-v10-postseal-v3.py')]);assert seal['exit_code']==0
print(json.dumps(dict(handover=clean(handover),final_live_seal=clean(seal))))
