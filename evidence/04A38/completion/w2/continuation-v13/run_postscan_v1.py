"""Append-only sequential proof runner. No validation campaign is repeated."""
from common import *
import runpy

def run(label,name,*args,private=False):
    source=PRIVATE/'bookkeeping-code-v1'/name if private else HERE/name
    result=command(label,[sys.executable,'-B',str(source),*args])
    print(label,result['exit_code'],flush=True)
    assert result['exit_code']==0,'Retain unsuccessful attempt: '+label

if __name__=='__main__':
    if sys.argv[1]=='install':
        runpy.run_path(str(PRIVATE/'bookkeeping-code-v1/install_postscan_tools_v1.py'),run_name='__main__')
    else:run(sys.argv[1],sys.argv[2],*sys.argv[3:])
