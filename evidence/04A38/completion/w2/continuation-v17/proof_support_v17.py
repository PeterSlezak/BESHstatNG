"""Read-only exact identity and source/Debug binding; never loads an original DLL."""
from common import *
import re,copy
sys.path.extend([str(PREVIOUS),str(W2/'continuation-v15'),str(W2/'continuation-v14')])
from proof_support_v16 import body,source_part,tokens,phase,unwrap_cases,bind,closure,definitions,NESTED_CACHE_DECLARATIONS,portable_bindings,instructions
load=lambda p:json.loads(Path(p).read_bytes())
FINAL=HERE/'final-v3'
def setup(meta):
 for t in meta['types']:
  name=t['type'] or ''
  if '+' in name and '_Closure$' not in name:
   owner,child=name.split('+',1)
   if '+' not in child:NESTED_CACHE_DECLARATIONS.setdefault(owner,set()).add(child)
def same(a,b,label):assert tokens(a)==tokens(b),'Entire body mismatch '+label
def normalize(s):return s.replace('\r','')
def untext(s):
 s=unwrap_cases(s)
 s=re.sub(r'LegacyUdfText38\.EqualText\((\w+),\s*("(?:[^"\n]|"")*"|String\.Empty)\)',r'\1 = \2',s)
 s=re.sub(r'LegacyUdfText38\.NotEqualText\((\w+),\s*("(?:[^"\n]|"")*"|String\.Empty)\)',r'\1 <> \2',s)
 return s.replace('Global.BESHStatNG.regression.','regression.')
def method(meta,owner,name,parameters=None):
 rows=[r for r in meta['methods'] if (r['declaring_type'],r['member_name'])==(owner,name) and (parameters is None or r['parameter_types']==parameters)]
 assert len(rows)==1,(owner,name,len(rows));return rows[0]
def lambda_bind(meta,node,universe,shapes):
 members={r['symbol_id']:r for r in universe['members']};parent=members[node['parent_member']]
 pm=lambda_bind(meta,parent,universe,shapes)['method'] if parent['kind'].endswith('LambdaExpression') else bind(parent,meta,shapes)
 siblings=sorted([r for r in universe['members'] if r['parent_member']==parent['symbol_id'] and r['kind'].endswith('LambdaExpression')],key=lambda r:r['span_start'])
 defs=definitions(meta);sites=[(i,x) for i,x in enumerate(pm['instructions']) if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'][7:] in defs and defs[x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
 assert len(sites)==len(siblings),(parent['name'],len(sites),len(siblings))
 ordinal=siblings.index(node);i,x=sites[ordinal];target=defs[x['operand'][7:]]
 assert pm['instructions'][i+1]['op']=='newobj'
 return dict(method=target,parent=pm,ordinal=ordinal,construction=pm['instructions'][i:i+2])
def map_native(before,after,common):
 result={};moved=[]
 for n in before:
  candidates=[r for r in after if r['signature_key']==n['signature_key']]
  if n['kind'].endswith('LambdaExpression'):
   op=next(r for r in before if r['symbol_id']==n['parent_member'])
   if op['name']=='TryResolveMmrmFormulaRequiredXColumns':
    parent=next(r for r in common if r['name']=='ResolveMmrmFormulaColumns' and r['kind']=='FunctionBlock')
    pool=[r for r in common if r['parent_member']==parent['symbol_id']]
    candidates=[r for r in pool if r['body_sha256']==n['body_sha256'] or r['body_token_sha256']==n['body_token_sha256']]
    assert len(candidates)==1
    result[n['symbol_id']]=candidates[0];moved.append(n['symbol_id']);continue
   parent=result[n['parent_member']]
   candidates=[r for r in candidates if r['parent_member']==parent['symbol_id'] and r['body_sha256']==n['body_sha256']]
   if len(candidates)>1:
    siblings=sorted([r for r in before if r['parent_member']==n['parent_member'] and r['signature_key']==n['signature_key'] and r['body_sha256']==n['body_sha256']],key=lambda r:r['span_start'])
    candidates=sorted(candidates,key=lambda r:r['span_start'])
    assert len(siblings)==len(candidates)
    candidates=[candidates[siblings.index(n)]]
  if len(candidates)>1:
   candidates=[r for r in candidates if r['body_sha256']==n['body_sha256']]
  assert len(candidates)==1,(n['symbol_id'],len(candidates));result[n['symbol_id']]=candidates[0]
 assert len(result)==726 and len(after)==723 and len(moved)==3
 assert len({r['symbol_id'] for r in result.values()})==726
 return result,moved
