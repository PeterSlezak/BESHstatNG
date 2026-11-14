import json,hashlib
from pathlib import Path
from collections import Counter
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
def need(v,m):
 if not v:raise ValueError(m)
def sha(b):return hashlib.sha256(b).hexdigest()
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {}
 def raw(p):return overrides[p] if p in overrides else (root/p).read_bytes()
 table_bytes=raw(PREFIX+'text-call-sites-v2.json');table=json.loads(table_bytes);calls=table['calls']
 old=json.loads(raw('evidence/04A38/completion/w2/continuation-v5/text-call-sites-v2.json'))
 sig=lambda r:(r['path'],r['parent_name'],r['kind'],r['left_expression'],r['right_literal'],tuple(r['choices']))
 a=Counter(sig(r) for r in old['calls']);b=Counter(sig(r) for r in calls)
 need(not a-b,'Lost inherited comparison domain')
 extra=[r for r in calls if (b-a)[sig(r)]]
 need(len(extra)==11 and all(r['path']=='BESHStat.Excel.Common/src/Results/UdfInferencePreparation38.vb' for r in extra),'Unreviewed comparison frontier expansion')
 need(Counter(r['parent_name'].rsplit('.',1)[1] for r in extra)==Counter(NestedTableSelection=8,ApplyRepeatedCorrection=1,BuildRepeatedMcpTables=1,IndependentTTableSelection=1),'Unexpected new comparison parent')
 need(len(calls)==len({r['site_id'] for r in calls})==220,'Duplicate/missing comparison site')
 for s in table['sources']:
  data=raw(s['path']);need((len(data),sha(data))==(s['bytes'],s['sha256']),'Stale live Common source universe')
 for r in calls:
  values=[r['right_literal']] if r['kind']=='EqualText' else r['choices']
  need(all(isinstance(v,str) and all(32<=ord(c)<=126 for c in v) for v in values),'Reachable null/ignorable/dynamic RHS')
  snippet=raw(r['path']).decode('utf-8-sig').encode('utf-16-le')[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le')
  need(r['right_expression'] in snippet,'Wrong actual comparison source span')
 domain=json.loads(raw(PREFIX+'text-caller-domains-v2.json'))
 need({r['site_id'] for r in domain['calls']}=={r['site_id'] for r in calls} and all(not r['normalization_to_be_independently_classified'] for r in domain['calls']),'Unclassified caller domain')
 need(sha(raw('BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb'))=='d42882ab96ae198e6e8c2c260f38d3e59439ab1adbe7c49e1c4f9347fb1c46cf','Global text policy changed')
 rows=[]
 for config in ['Debug','Release']:
  p=json.loads(raw(PREFIX+'text-domains-families-v2-v6-'+config+'.json'))
  previous=json.loads(raw('evidence/04A38/completion/w2/continuation-v5/text-domains-cells-v5-v6-'+config+'.json'))
  need(p['source_table_sha256']==sha(table_bytes),'Probe used different source frontier')
  values=p['values_utf16'];need(len(values)==len(set(values))==p['values_per_site'] and set(previous['values_utf16'])<=set(values),'Lost/duplicated runtime input values')
  need((p['direct_sites'],p['direct_source_lines'],p['CaseToken_sites'],p['CaseToken_choice_occurrences'])==(153,62,67,788),'Stale top-level source counters')
  result=p['results'];need(len(result)==440 and {(r['site_id'],r['culture']) for r in result}=={(r['site_id'],c) for r in calls for c in ['Invariant','sk-SK']},'Runtime site/culture rows missing/duplicated')
  need(all(r['checked_values']==len(values) and r['differences']==0 and not r['mismatches'] for r in result),'Current source-domain difference')
  need(p['total_site_value_observations']==440*len(values) and p['source_domain_differences']==0,'Forged runtime observation count')
  need(p['synthetic']==previous['synthetic'],'Erased/changed inherited synthetic adverse results')
  need(p['comparison_IL_sha256']==previous['comparison_IL_sha256'],'Different Framework comparison operation')
  rows.append(dict(configuration=config,values=len(values),observations=p['total_site_value_observations'],source_domain_differences=0,raw_synthetic_differences=8))
 return dict(status='CURRENT_CALLER_DOMAIN_EQUIVALENCE_PASS_NOT_GLOBAL_TEXT_IDENTITY',sites=220,direct=153,direct_lines=62,CaseToken=67,choices=788,rows=rows,policy_unchanged=True,original_queries=0)
if __name__=='__main__':print(json.dumps(inspect()))
