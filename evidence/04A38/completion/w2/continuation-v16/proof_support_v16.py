"""Private draft of exact source/constructed Debug binding helpers."""
from common import *
import re,copy
sys.path.append(str(PREVIOUS));sys.path.append(str(W2/'continuation-v14'))
from source_views import body,view
from proof_kernel_v16 import bind,closure,definitions,map_current,NESTED_CACHE_DECLARATIONS
from proof_kernel_v4 import parameter_matches
from verify_c14_composition_v2 import tokens,phase,unwrap_cases

def load(p):return json.loads(Path(p).read_bytes())
def cil(m):return {k:v for k,v in m.items() if k not in ['raw_IL_sha256','signature']}
def instructions(m):return {k:m[k] for k in ['parameter_types','attributes','implementation_flags','locals','init_locals','exceptions','instructions'] if k in m}
def source_part(universe,node,data):
 source=next(s for s in universe['sources'] if s['path']==node['path'])
 text,policy=view(data,source['sha256'])
 return text.encode('utf-16-le')[node['span_start']*2:(node['span_start']+node['span_length'])*2].decode('utf-16-le'),policy
def configure_native():
 for owner,names in [('CoxUDFs',{'CoxModelHandle'}),('GEEUDFs',{'GeeHandle','GeeLsmEstimateProfileValue','GeeLsmEstimateComponent','GeeLsmEstimateAtProfile'}),('GLMUDFs',{'GlmHandle'}),('GLMNbUDFs',{'GlmNbHandle'}),('LinearModelUDFs',{'LinearModelHandle'}),('LmmUDFs',{'LmmHandle','LmmImportedInputs'})]:NESTED_CACHE_DECLARATIONS['BESHStatNG.WorksheetFunctions.'+owner]=names
def native_bind(n,meta,shapes,members,shape_node=None):
 if n['kind'] in ['PropertyBlock','GetAccessorBlock']:
  prop=n if n['kind']=='PropertyBlock' else members[n['parent_member']]
  owner=prop['owner'].replace('.LmmImportedInputs','+LmmImportedInputs')
  rows=[r for r in meta['methods'] if (r['declaring_type'],r['member_name'])==(owner,'get_'+prop['name'])]
  assert len(rows)==1;return rows[0]
 return bind(n,meta,{**shapes,n['symbol_id']:shapes[(shape_node or n)['symbol_id']]})
def generic_names(header,name):
 offset=header.index(name)+len(name)
 if not header[offset:].lstrip().lower().startswith('(of '):return []
 start=header.index('(Of ',offset)
 depth=1;braces=0;first=start+4;chunks=[];i=first
 while i<len(header):
  ch=header[i]
  if ch=='(':depth+=1
  elif ch==')':
   depth-=1
   if depth==0:chunks.append(header[first:i]);break
  elif ch=='{':braces+=1
  elif ch=='}':braces-=1
  elif ch==',' and depth==1 and braces==0:chunks.append(header[first:i]);first=i+1
  i+=1
 assert depth==0
 return [part.strip().split()[0] for part in chunks]
def portable_bindings(n,meta,shapes,graph):
 shape=shapes[n['symbol_id']];parameters=shape['parameters'] or []
 name='.ctor' if n['kind']=='ConstructorBlock' else n['name']
 source_generic=generic_names(shape['header'],n['name']) if name!='.ctor' else []
 declared={r['type'] for r in meta['types'] if r['type']}
 output=[];reachable=set(graph['methods'])
 for m in meta['methods']:
  if m['method'] not in reachable or m['member_name']!=name or not m['declaring_type']:continue
  if m['declaring_type'].split('`',1)[0]!=n['owner']:continue
  signature=m['signature'];args=signature.get('generic_arguments') or []
  if signature['generic_definition']:continue
  if len(args)!=len(source_generic) or len(m['parameter_types'])!=len(parameters):continue
  substitute={key:value['name'].split(' @ ',1)[0] for key,value in zip(source_generic,args)}
  matched=True
  for p,a in zip(parameters,m['parameter_types']):
   if a is None:matched=False;break
   prepared=dict(p)
   for key,value in substitute.items():prepared['type']=re.sub(r'\b'+re.escape(key)+r'\b',lambda _:value,prepared['type'])
   if parameter_matches(prepared,a):continue
   refs=re.findall(r'[A-Za-z_][A-Za-z0-9_.$`]*(?:\+[A-Za-z0-9_.$`]+)+',a)
   if not refs or not all(ref in declared for ref in refs) or not parameter_matches(prepared,a.replace('+','.')):matched=False;break
  if matched:output.append(m)
 assert output,'No complete actual constructed source/Debug binding '+n['symbol_id']
 assert len({m['method'] for m in output})==len(output)
 return output

def write_json(folder,name,doc):
 path=Path(folder)/name
 assert path.is_file(), 'Missing sealed proof '+str(path)
 assert load(path)==doc, 'Independent proof recomputation differs '+name



def phase(text):
 text=text.replace('\r\n','\n')
 match=re.search(r'(?m)^\s*(?:(?:Public|Private|Friend|Protected|Shared|Overloads|Overrides|Overridable|NotOverridable|MustOverride|Async|Iterator|Static)\s+)*(Function|Sub)\s+(\w+)',text)
 assert match is not None,'Missing exact method declaration'
 def balanced(start):
  depth=0;quoted=False;i=start
  while i<len(text):
   ch=text[i]
   if ch=='"':
    if quoted and i+1<len(text) and text[i+1]=='"':i+=2;continue
    quoted=not quoted
   elif not quoted:
    if ch=='(':depth+=1
    elif ch==')':
     depth-=1
     if depth==0:return i
   i+=1
  raise AssertionError('Unbalanced actual declaration')
 start=text.index('(',match.end());end=balanced(start)
 if text[start:start+4].lower()=='(of ':
  start=text.index('(',end+1);end=balanced(start)
 first=text.index('\n',end)+1;last=text.rfind('End '+match[1]);assert last>=first
 return text[first:last].strip()
