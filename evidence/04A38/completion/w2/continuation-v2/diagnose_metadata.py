from common import *
def stable(r):return {k:v for k,v in r.items() if k!='raw_IL_sha256'}
x=json.loads((HERE/'metadata/b1-v1/original-Debug.json').read_bytes());y=json.loads((HERE/'metadata/b1-v1/current-Debug.json').read_bytes());ys={r['method']:r for r in y['methods']}
names={'IsMissingArg','IsBlankCell','AsString','TryGetHandleKey','TryGetCachedHandle','PS_CLEANUP','TryGetPsmHandle','LoggedUdfError','LoggedUdfExceptionText'}
for r in x['methods']:
 if r['member_name'] in names or r['member_name'].endswith('_DROP') or r['member_name'].startswith('TryGet') and 'Handle' in r['member_name'] and 'Multivariate' in r['declaring_type']:
  v=ys.get(r['method']);print(r['declaring_type'],r['member_name'],bool(v and stable(v)==stable(r)))
  if v and stable(v)!=stable(r):print('differ',[k for k in r if k!='raw_IL_sha256' and r[k]!=v[k]])
  calls=[i['operand'] for i in r['instructions'] if i['op'] in ['call','callvirt','newobj']];print('calls',calls)
for t in x['types']:
 if t['root_owner'].endswith('SampleSizeUDFs'):print('SAMPLE_FIELDS',t['fields'])
