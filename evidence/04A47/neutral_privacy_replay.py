"""Standalone neutral-identity replay of both exact original scanners on ZIP bytes.

This is not the required real owner-local full-Git-candidate/physical scan.
Positive synthetic matches are expected controls,not new private findings.
"""
import base64,hashlib,json,os,sys,types,uuid,zipfile
from pathlib import Path
EXPECTED={'13':'b26596e5bf9644e81a479fad4f28d3db8d8af23611175b8e582c759007009a03','14':'b4b4fd3567947d807c73e63022a0128ded37199248d0067f786cb472811d5a25'}
GUARD='ad17f82adca850afb94fa60d3c0ca5866b446860a7724dc77a5e5b3111c9ce5f'
class Ancestor:
 def __init__(self,name):self.name=name;self.parent=types.SimpleNamespace(name='Users')
class Member:
 def __init__(self,data):self.data=data
 def is_file(self):return self.data is not None
 def read_text(self,encoding):assert encoding=='utf-8-sig';return self.data.decode(encoding)
class Root:
 name='neutral-review'
 def __init__(self,files,identity):self.files=files;self.parents=[Ancestor(identity)]
 def __truediv__(self,name):return Member(self.files.get(name))
def execute(source,name,filename):
 m=types.ModuleType(name);m.__file__=filename;exec(compile(source,filename,'exec'),vars(m));return m
def replay(archive):
 with zipfile.ZipFile(archive) as z:files={n:z.read(n) for n in z.namelist()}
 guard=files['evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py'];assert hashlib.sha256(guard).hexdigest()==GUARD
 proxy=execute(guard,'neutral_guard','guard.py');token='review'+uuid.uuid4().hex
 while any(token.encode() in b for b in files.values()):token='review'+uuid.uuid4().hex
 neutral_host='host'+uuid.uuid4().hex
 while any(neutral_host.encode() in b for b in files.values()):neutral_host='host'+uuid.uuid4().hex
 old=os.environ.get('COMPUTERNAME');os.environ['COMPUTERNAME']=neutral_host
 results=[]
 try:
  for phase in ['13','14']:
   name='scripts/verify-04a'+phase+'-publication.py';source=files[name];assert hashlib.sha256(source).hexdigest()==EXPECTED[phase]
   m=execute(source,'neutral_scanner_'+phase,str(Path('virtual/scripts/scanner.py').resolve()));m.re=proxy.RegexProxy();m.ROOT=Root(files,token)
   findings,_=m.scan(sorted(files));assert not findings
   positive=[]
   variants=[token,token.swapcase(),''.join(chr(ord(c)+0xfee0) if 33<=ord(c)<=126 else c for c in token),base64.b64encode(('public '*20+token+' suffix '*20).encode()).decode()]
   for variant,mode in zip(variants,['literal','case','NFKC','Base64']):
    m.ROOT=Root({'positive.json':json.dumps(dict(value=variant)).encode()},token);found,_=m.scan(['positive.json']);assert any(f['category']=='known_private_identifier' for f in found)
    positive.append(dict(mode=mode,expected_positive_detected=True))
   m.ROOT=Root({'negative.txt':b'ordinary public text'},token);found,_=m.scan(['negative.txt']);assert not found
   results.append(dict(scanner=phase,members=len(files),neutral_findings=0,positive_controls=positive,clean_negative=True,actual_owner_scan=False))
 finally:
  if old is None:os.environ.pop('COMPUTERNAME',None)
  else:os.environ['COMPUTERNAME']=old
 return dict(kind='NEUTRAL_REVIEW_REPLAY_NOT_OWNER_IDENTITY_PROOF',reports=results,tokens_published=False)
if __name__=='__main__':print(json.dumps(replay(sys.argv[1]),sort_keys=True))
