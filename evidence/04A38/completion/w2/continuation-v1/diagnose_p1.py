from common import *
report=json.loads((HERE/'compatibility-p1-v4.json').read_bytes())
for r in report['results']:
 if r['exact']:continue
 a=(ROOT/r['original']['path']).read_text().splitlines();b=(ROOT/r['current']['path']).read_text().splitlines()
 diffs=[dict(row=i+1,original=x,current=y) for i,(x,y) in enumerate(zip(a,b)) if x!=y]
 print(r['configuration'],r['corpus'],len(diffs),json.dumps(diffs[:3]))
 save('attempts/p1-v4-'+r['configuration']+'-'+r['corpus']+'-differences.json',dict(case_count=r['cases'],mismatches=diffs,original=r['original'],current=r['current']))
