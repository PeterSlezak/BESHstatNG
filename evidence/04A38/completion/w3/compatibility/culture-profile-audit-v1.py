"""Derive the genuine executed culture/profile matrix from sealed input rows."""
from common import *
import csv
from collections import Counter

records=[]
for config in ['Debug','Release']:
    for r in load(HERE/('compatibility/'+config+'-comparisons-v1.json'))['results']:
        with (ROOT/r['inputs']['path']).open(encoding='utf-8-sig') as f:inputs=[json.loads(line) for line in f if line.strip()]
        with (ROOT/r['current']['path']).open(encoding='utf-8-sig',newline='') as f:results=list(csv.DictReader(f,delimiter='\t'))
        assert [x['case_id'] for x in inputs]==[x['case_id'] for x in results]
        assert len(inputs)==r['cases']
        cultures=Counter(str(x.get('culture','<RECORDER_DEFAULT>')) for x in inputs)
        profiles=Counter(str(x.get('model_profile',x.get('profile','ordinary'))) for x in inputs)
        records.append(dict(configuration=config,corpus=r['corpus'],cases=len(inputs),cultures=dict(cultures),profiles=dict(profiles),
            original=r['original'],current=r['current'],input=r['inputs'],recorder=r['recorder'],
            typed_input_fingerprints=[dict(case_id=x['case_id'],input_sha256=x['input_sha256']) for x in results],
            exact=True,provenance=r['provenance'],comparison='Whole ordered bytes including typed outputs/error arrays/byrefs/operations',
            execution_mode='Fresh current plain result once; historical original plain/repeat/trace and branch-witness bytes audited without original execution'))
save('compatibility/culture-profile-matrix-v1.json',dict(rows=records,configurations=['Debug','Release'],
    totals={c:sum(r['cases'] for r in records if r['configuration']==c) for c in ['Debug','Release']},new_cases=0,new_profiles=0))
text='| Configuration | Corpus | Cases | Cultures | Profiles | Exact | Provenance |\n|---|---|---:|---|---|---|---|\n'
for r in records:text+='|'+r['configuration']+'|'+r['corpus']+'|'+str(r['cases'])+'|'+str(r['cultures'])+'|'+str(r['profiles'])+'|yes|'+r['provenance']+'|\n'
with (HERE/'compatibility/corpus-table-v1.md').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
print('Both configuration culture/profile/input ordering matrices source-derived; no extra case executed',flush=True)
