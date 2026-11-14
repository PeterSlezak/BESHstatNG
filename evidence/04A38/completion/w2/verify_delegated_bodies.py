"""Independent whole-CLR-body proof for pure UDF delegations.

Records every instruction/literal/local/exception arm. Only NOPs/branch encodings
and an explicitly published method-relocation graph are canonicalized. No result
normalization, tolerance, percentage or mere call edge can close a body.
"""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4];W='evidence/04A38/completion/w2/'
def need(v,m):
 if not v:raise ValueError(m)
def load(p):return json.loads(p.read_bytes())
def source(root,row):
 data=(root/row['path']).read_bytes();need(hashlib.sha256(data).hexdigest()==row['source_sha256'],'Current source/body inventory stale')
 return data.decode('utf-8-sig').encode('utf-16-le')[2*row['span_start']:2*(row['span_start']+row['span_length'])].decode('utf-16-le').replace('\r\n','\n')
def only_return(s):
 m=re.search(r'\)\s+As\s+[^\n]+\n',s)
 if not m:return None
 b=s[m.end():s.rindex('End Function')].strip()
 if not re.fullmatch(r'Return\s+Global\.BESHStatNG\.(?:ExcelCommon|WorksheetCompatibility)\.[\s\S]+',b):return None
 # Entire body must be one return invocation; forbid extra statements/colon.
 if re.search(r'(?im)^\s*(?:Dim|If|For|While|Try|Throw|Return|End|Select|With|Exit)\b',b[len('Return'):]):return None
 return b
def inspect(root=ROOT):
 root=Path(root);u=load(root/(W+'current-universe-profiles-v1.json'));historical=load(root/'evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members']
 original_by_key={r['git_original_id'].split('@')[0]:r for r in historical}
 current_il={c:{r['method']:r for r in load(root/(W+'method-bodies-current-'+c+'.json'))} for c in ['Debug','Release']}
 original_il={c:{r['method']:r for r in load(root/(W+'method-bodies-original-'+c+'.json'))} for c in ['Debug','Release']}
 compiled=load(root/(W+'compiled-Debug-profiles-v1.json'))['methods'];candidates=[]
 for row in u['members']:
  if row['kind']!='FunctionBlock' or row['symbol_id'].split('@')[0] not in original_by_key:continue
  if only_return(source(root,row)) is None:continue
  methods=[m for m in compiled if m['name']==row['name'] and m['declaring_type'].replace('+','.')==row['owner']]
  if len(methods)!=1:continue
  method=methods[0];calls=[c for c in method['calls'] if c['assembly'] in ['BESHStat.Core','BESHStat.Excel.Common']]
  if len(calls)!=1 or method['method'] not in original_il['Release']:continue
  target=calls[0]['target']
  if target not in current_il['Release']:continue
  candidates.append(dict(original_id=original_by_key[row['symbol_id'].split('@')[0]]['canonical_original_id'],current_id=row['symbol_id'],method=method['method'],target=target))
 aliases={r['method']:r['target'] for r in candidates}
 def body(r,relocate):
  # Context-sensitive reflection and implicit newly introduced static state
  # cannot be justified by opcode similarity.
  if r['type_initializer'] or r['nonliteral_static_fields']:return None
  for i in r['instructions']:
   o=i['operand']
   if isinstance(o,str) and any(t in o for t in ['GetCurrentMethod','GetExecutingAssembly','GetCallingAssembly','StackTrace','StackFrame']):return None
  out={k:v for k,v in r.items() if k not in ['method','type_initializer','nonliteral_static_fields']}
  if relocate:
   out=json.loads(json.dumps(out))
   for i in out['instructions']:
    if isinstance(i['operand'],str) and i['operand'].startswith('METHOD:'):
     old=i['operand'][7:]
     if old in aliases:i['operand']='METHOD:'+aliases[old]
  return out
 rows=[]
 for c in candidates:
  exact=[];already=[];dependencies=set()
  for config in ['Debug','Release']:
   original=original_il[config][c['method']];target=current_il[config].get(c['target']);wrapper=current_il[config].get(c['method'])
   ob=body(original,True);tb=body(target,False) if target else None
   # Already portable bodies are compared as existing wrappers, not to a
   # statistical engine. The accepted engine itself is outside this migration.
   raw=body(original,False);wb=body(wrapper,False) if wrapper else None
   already.append(raw is not None and raw==wb)
   exact.append(ob is not None and ob==tb)
   for i in original['instructions']:
    o=i['operand']
    if isinstance(o,str) and o.startswith('METHOD:') and o[7:] in aliases and o[7:]!=c['method']:dependencies.add(o[7:])
  status='ALREADY_PORTABLE_FULL_BODY_IL_EXACT' if all(already) else 'RELOCATED_FULL_BODY_IL_EXACT' if all(exact) else 'OPEN_NO_FULL_IL_EQUIVALENCE'
  rows.append(dict(c,Debug_body_exact=exact[0],Release_body_exact=exact[1],Debug_already_portable=already[0],Release_already_portable=already[1],dependencies=sorted(dependencies),status=status))
 # A full-body relocation whose callee proof is open remains open itself.
 changed=True
 while changed:
  changed=False;open_methods={r['method'] for r in rows if r['status'].startswith('OPEN')}
  for row in rows:
   if row['status']=='RELOCATED_FULL_BODY_IL_EXACT' and open_methods.intersection(row['dependencies']):row['status']='OPEN_CALLEE_BODY_NOT_PROVED';changed=True
 closed=[r for r in rows if not r['status'].startswith('OPEN')]
 return dict(status='BOUNDED_DELEGATED_BODY_PROOF_NOT_BATCH_READY',candidates=len(rows),closed=len(closed),open=len(rows)-len(closed),rows=rows,
  semantics='Full instruction/literal/local/exception-arm match in both configurations; no source-body closure from a call edge alone',native='NOT_RUN')
if __name__=='__main__':
 try:
  report=inspect();print(json.dumps(report))
 except (ValueError,OSError,KeyError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
