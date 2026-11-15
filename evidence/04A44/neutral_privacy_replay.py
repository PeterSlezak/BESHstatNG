"""Read-only neutral-token replay of unchanged scanners. No Git or actual owner identities.
Run with repository/review-tree root. This is independent of required owner-context scans."""
from pathlib import Path
import base64,importlib.util,json,os,sys,hashlib
EXPECTED={'13':'b26596e5bf9644e81a479fad4f28d3db8d8af23611175b8e582c759007009a03','14':'b4b4fd3567947d807c73e63022a0128ded37199248d0067f786cb472811d5a25'}
class Member:
 def __init__(self,text):self.text=text
 def is_file(self):return True
 def read_text(self,encoding):assert encoding=='utf-8-sig';return self.text
class NeutralRoot:
 parents=(Path('Users')/'review-fixture-user',);name='review-tree'
 def __init__(self,text):self.text=text
 def __truediv__(self,p):return Member(self.text)
def run(root):
 rows=[];prior=os.environ.get('COMPUTERNAME');os.environ['COMPUTERNAME']='review-fixture-host'
 try:
  for phase in ['13','14']:
   path=root/('scripts/verify-04a'+phase+'-publication.py');assert hashlib.sha256(path.read_bytes()).hexdigest()==EXPECTED[phase]
   spec=importlib.util.spec_from_file_location('neutral_scanner_'+phase,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
   controls=[]
   for token in ['review-fixture-user','review-fixture-host']:
    for mode,value in [('literal',token),('case',token.upper()),('NFKC',''.join(chr(ord(c)+0xfee0) if 33<=ord(c)<=126 else c for c in token)),('base64',base64.b64encode(('A'*90+' '+token+' '+'Z'*90).encode()).decode())]:
     m.ROOT=NeutralRoot(json.dumps(dict(value=value)));found,_=m.scan(['probe.json']);assert any(r['category']=='known_private_identifier' for r in found);controls.append(dict(mode=mode,detected=True))
   m.ROOT=NeutralRoot(json.dumps(dict(value='benign review probe')));f,_=m.scan(['probe.json']);assert not f
   rows.append(dict(scanner=phase,source_sha256=EXPECTED[phase],positive_controls=controls,benign_negative_zero=True,scope='NEUTRAL_REPLAY_ONLY;NOT_OWNER_CONTEXT_OR_REPOSITORY_CANDIDATE_SCAN'))
 finally:
  if prior is None:os.environ.pop('COMPUTERNAME',None)
  else:os.environ['COMPUTERNAME']=prior
 return dict(status='PASS',rows=rows,files_written=0,Git_network_native_calls=0)
if __name__=='__main__':print(json.dumps(run(Path(sys.argv[1])),sort_keys=True))
