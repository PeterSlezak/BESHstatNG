"""Rebind earlier whole-body evidence to actual current IL/context, not old flags."""
import json
from pathlib import Path
from verify_slice_v3 import ROOT,HERE,need,semantic
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def load(p):return json.loads(overrides[p] if p in overrides else (root/p).read_bytes())
 allowed={('BESHStatNG.UdfLinearAlgebra','TryInvertMatrix'),('BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38','TryInvertMatrix'),*[('BESHStatNG.WorksheetFunctions.ParametricUDFs',n) for n in ['ANOVA1','ANOVA1_WELCH','ANOVA1_MCP']]}
 results=[]
 for c in ['Debug','Release']:
  old=load('evidence/04A38/completion/w2/continuation-v3/metadata-v5/names-v3/current-'+c+'.json');new=load(base+'metadata-v6/parametric-v4/current-'+c+'.json');nm={m['method']:m for m in new['methods']};nt={t['type']:t for t in new['types']};checked=0
  for t in old['types']:need(t['type'] in nt and t==nt[t['type']],'Retained type/field/initializer context changed')
  for m in old['methods']:
   if (m['declaring_type'],m['member_name']) in allowed:continue
   need(m['method'] in nm and semantic(m)==semantic(nm[m['method']]),'Retained full method/dependency changed '+m['method']);checked+=1
  former={m['method']:m for m in load('evidence/04A38/completion/w2/method-bodies-v2-current-'+c+'.json')};current={m['method']:m for m in load(base+'inspection/parametric-v4/MethodBodies38-'+c+'.json')};carried=load('evidence/04A38/completion/w2/independent-delegated-proof-v2.json')['rows'];count=0
  for r in carried:
   if r['status'].startswith('OPEN'):continue
   ids=[r['method'],r['target']];ctor=r['method'].split('(')[0].rsplit('.',1)[0]+'..cctor()'
   if ctor in former:ids.append(ctor)
   need(all(i in current and current[i]==former[i] for i in ids),'Earlier39 whole IL proof lost');count+=1
  need(count==39,'Missing carried proof')
  results.append(dict(configuration=c,existing_current_methods_exact=checked,existing_types_exact=len(old['types']),changed_methods=sorted('::'.join(x) for x in allowed),prior_whole_CIL_proofs_exact=count))
 return dict(status='FRESH_RETAINED_METHOD_FIELD_STATIC_GENERIC_CONTEXT_PASS_NOT_READY',configurations=results,raw_IL_metadata_tokens='Raw hashes retained; only token-bearing raw hash excluded from semantic equality. All resolved instructions/operands/branches/locals/EH/params/fields/initializers remain mandatory.',original_queries=0)
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
