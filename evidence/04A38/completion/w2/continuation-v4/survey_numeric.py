from common import *
old=json.loads((PREVIOUS/'historical-ownership.json').read_bytes());rows=[]
def body(r):return {k:r[k] for k in ['member_name','parameter_types','implementation_flags','locals','init_locals','exceptions','instructions']}
data={c:(json.loads((HERE/'metadata-v6/parametric-v4'/('original-'+c+'.json')).read_bytes()),json.loads((HERE/'metadata-v6/parametric-v4'/('current-'+c+'.json')).read_bytes())) for c in ['Debug','Release']}
for h in old['members']:
 if h['proof']:continue
 proofs=[]
 for c,(o,n) in data.items():
  originals=[m for m in o['methods'] if m['member_name']==h['name'] and m['declaring_type'] in h['canonical_original_id']]
  targets=[m for m in n['methods'] if m['member_name']==h['name'] and m['declaring_type']=='BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38']
  if len(originals)!=1 or len(targets)!=1:break
  a,b=originals[0],targets[0];current=next((m for m in n['methods'] if m['method']==a['method']),None)
  if current is None or body(a)!=body(b) or b['exceptions'] or b['type_initializer'] or b['nonliteral_static_fields']:break
  ot=next(t for t in o['types'] if t['type']==a['declaring_type']);nt=next(t for t in n['types'] if t['type']==a['declaring_type'])
  if ot!=nt:break
  octors=[m for m in o['methods'] if m['declaring_type']==a['declaring_type'] and m['member_name'] in ['.ctor','.cctor']];nm={m['method']:m for m in n['methods']}
  if any(m['method'] not in nm or body(m)!=body(nm[m['method']]) for m in octors):break
  calls=[i['operand'] for i in current['instructions'] if i['op'] in ['call','callvirt']]
  if 'METHOD:'+b['method'] not in calls:break
  proofs.append(dict(configuration=c,original_method=a['method'],portable_method=b['method'],native_wrapper=current['method'],native_type_and_initializers_exact=True,entire_instructions_locals_EH_and_parameters_exact=True,portable_static_state=False,calls=[i['operand'] for i in b['instructions'] if i['op'] in ['call','callvirt','newobj']]))
 if len(proofs)==2:rows.append(dict(original_id=h['canonical_original_id'],path=h['path'],name=h['name'],configurations=proofs))
save('numeric-candidates.json',dict(scope='Conservative full-body/real-consumer/native-context equality candidates; dependency/source review still required before closure',rows=rows))
print('candidates',len(rows),[r['name'] for r in rows])
