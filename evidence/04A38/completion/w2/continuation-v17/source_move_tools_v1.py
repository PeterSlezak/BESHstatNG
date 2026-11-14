"""Coherent exact-source report, MMRM row/design, ZIP and survival preparation moves."""
from common import *
import re
assert json.loads((HERE/'entry-authentication-v1.json').read_bytes())['status']=='ENTRY_GATES_PASS'
moves=[];before=[];cache={};report=[];prep=[]
REPORT='UdfW2CompletionReports38';PREP='UdfW2CompletionPreparation38'
def read(path):
 if path not in cache:
  raw=(ROOT/path).read_bytes();target=HERE/'remaining-packets-before'/path;target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as f:f.write(raw)
  cache[path]=[raw,raw.decode('utf-8-sig')];before.append(dict(path=path,live=pin(ROOT/path),preimage=pin(target)))
 return cache[path][1]
def function(path,name):
 s=read(path);m=re.search(r'(?m)^[ \t]*(?:Private|Friend|Public)(?: Shared)? (Function|Sub) '+re.escape(name)+r'\b',s);assert m,(path,name)
 end=re.search(r'(?m)^[ \t]*End '+m[1]+r'\b',s[m.start():]);assert end
 return s[m.start():m.start()+end.end()]
def norm(s):return s.replace('\r','')
def phase(s):
 s=norm(s);m=re.search(r'\b(Function|Sub)\s+\w+\(',s);assert m
 start=s.index('(',m.start());depth=0;quoted=False;i=start
 while i<len(s):
  ch=s[i]
  if ch=='"':
   if quoted and i+1<len(s) and s[i+1]=='"':i+=2;continue
   quoted=not quoted
  if not quoted:
   if ch=='(':depth+=1
   if ch==')':
    depth-=1
    if depth==0:break
  i+=1
 return s[s.index('\n',i)+1:s.rfind('End '+m[1])].strip()
def change(path,host,old,new,portable,aliases=None):
 text=read(path);assert text.count(old)==1,(path,host,old[:80]);cache[path][1]=text.replace(old,new,1)
 moves.append(dict(path=path,host=host,original_segment=old,native_call=new,portable=portable,aliases=aliases or [],original_queries=0))
def call_body(path,host,call):
 old=function(path,host);s=norm(old);start=s.index(phase(s));ending=list(re.finditer(r'(?m)^[ \t]*End (?:Function|Sub)\b',s))[-1];new=s[:start]+call+'\n'+s[ending.start():]
 eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n'
 return old,new.replace('\n',eol)
def lift(path,host,signature,call,dest,aliases=None):
 old=function(path,host);body=phase(old);pairs=aliases or []
 for a,b in pairs:body=body.replace(a,b)
 portable='        '+signature+'\n'+ '\n'.join('            '+line.lstrip() if not line.startswith(' ') else line for line in body.splitlines())+'\n        End '+('Sub' if ' Sub ' in signature else 'Function')
 (report if dest==REPORT else prep).append(portable)
 a,b=call_body(path,host,call);change(path,host,a,b,dict(owner=dest,signature=signature,body=body),[dict(before=x,after=y) for x,y in pairs])
def segment(path,host,start,end,signature,call,dest,aliases=None):
 f=function(path,host);a=f.index(start);b=f.index(end,a)+len(end);old=f[a:b];body=norm(old);pairs=aliases or []
 for x,y in pairs:body=body.replace(x,y)
 portable='        '+signature+'\n'+body+'\n        End '+('Sub' if ' Sub ' in signature else 'Function')
 (report if dest==REPORT else prep).append(portable)
 eol='\r\r\n' if '\r\r\n' in old else '\r\n' if '\r\n' in old else '\n'
 change(path,host,old,call.replace('\n',eol),dict(owner=dest,signature=signature,body=body),[dict(before=x,after=y) for x,y in pairs])

