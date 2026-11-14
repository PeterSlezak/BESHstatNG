"""Additive final handover, then the last live repository/index/protection seal."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v10'))
from common import *
handover=command('postseal-handover-v2',[sys.executable,'-B',str(W2/'continuation-v10-finalize-handover-v2.py')]);assert handover['exit_code']==0
seal=command('postseal-live-git-v3',[sys.executable,'-B',str(W2/'continuation-v10-postseal-v3.py')]);assert seal['exit_code']==0
print(json.dumps(dict(handover=clean(handover),final_seal=clean(seal),status='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED')))
