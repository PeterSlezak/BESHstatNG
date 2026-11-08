"""Append separately sealed label witnesses; do not alter the first late corpus."""
from pathlib import Path
import hashlib,json,itertools
HERE=Path(__file__).resolve().parent;EXT=HERE/'label-extension-v1'
EXT.mkdir(exist_ok=False)
source=(HERE/'Harness37.cs').read_text(encoding='utf-8')
source=source.replace('int count=state=="short"?Math.Max(0,n-1):state=="long"?n+1:n;return Enumerable.Range(0,count).Select(i=>state=="blank"?',
 'int count=state=="short"?Math.Max(0,n-1):state=="long"?n+1:n;if(state=="custom_duplicates")return Enumerable.Range(0,count).Select(i=>i%2==0?"duplicate":" custom ").ToArray();return Enumerable.Range(0,count).Select(i=>state=="blank"?',1)
needle='static string[] Groups(int n,string state){if(state=="none")return null;'
assert needle in source
source=source.replace(needle,needle+'if(state=="case_width")return Enumerable.Range(0,n).Select(i=>new[]{"B","\\uff22","b","A","a","",null}[i%7]).ToArray();')
(EXT/'Harness37.cs').write_text(source,encoding='utf-8',newline='\n')
cases=[]
for culture,op,k,ids,labels,groups in itertools.product(['','sk-SK'],['ScorePlot2D','ScorePlot3D','Biplot'],[0,1,2,3],['known','missing','short','duplicate'],['none','blank','mixed','custom_duplicates'],['none','case_width','short','long']):
    for scale in (['0','0.25','0.5','0.75','1'] if op=='Biplot' else ['1']):
        d=dict(family='PCA',operation=op,culture=culture,components=k,observations=7,variables=4,scale=scale,eigen='positive_unequal',row_ids=ids,row_labels=labels,groups=groups,variable_labels='mixed',matrix='valid',cohort='explicit_width_and_custom_identity')
        d['case_id']='PCA.'+op+'.'+hashlib.sha256(json.dumps(d,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24]
        d['expected_stage']='PUBLIC_GUARD_OR_PRE_CHART_THEN_RECORDED_LATE_NATIVE_BOUNDARY';cases.append(d)
with (EXT/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
    for c in cases:f.write(json.dumps(c,sort_keys=True,separators=(',',':'))+'\n')
(EXT/'specification.json').write_text(json.dumps(dict(provenance='CAPTURED_AFTER_INITIAL_IMPLEMENTATION_FROM_PINNED_ARCHIVED_ENTRY',cases=len(cases),reason='Main case_width profile aliases mixed and n4 does not contain a width variant. This extension explicitly supplies seven full-width/case/missing groups and all-custom nonblank duplicated labels with independent missing/short model IDs.',production_changes=False,main_reference_unchanged=True),indent=2)+'\n',encoding='utf-8',newline='\n')
print('Created append-only label extension:',len(cases),'cases; no main reference change')
