"""Independent byte/row comparison to fixed pre-production references.

No expected output generation; all case keys, types, bounds, IEEE payloads and
exception metadata are already encoded in the sealed source recorder rows.
"""
import csv,hashlib,json,sys
from collections import Counter
from pathlib import Path
from preflight import ROOT,HERE,save,pin

COUNTS={'original-v1':24732,'public-options-v1':2508,'public-primitives-v1':226,'model-inputs-v2':882,'math-edges-v1':618}

def rows(path):
 with path.open(encoding='utf-8',newline='') as f:return list(csv.DictReader(f,delimiter='\t'))

def compare(label):
 results=[]
 for config in ['Debug','Release']:
  for corpus,count in COUNTS.items():
   reference=(HERE/'preedit/frozen-v2'/('original-'+config+'.tsv') if corpus=='original-v1' else HERE/'preedit/attempts'/corpus/('capture-'+config+'-original-v1.tsv'))
   current=HERE/'compatibility'/label/config/corpus/'v1/current.tsv'
   a=rows(reference);b=rows(current);assert len(a)==count and len(b)==count
   assert len({r['case_id'] for r in a})==count
   mismatch=[dict(ordinal=i,reference_case=x['case_id'],current_case=y['case_id'],fields=[k for k in x if x[k]!=y[k]]) for i,(x,y) in enumerate(zip(a,b)) if x!=y]
   exact=reference.read_bytes()==current.read_bytes()
   execution=json.loads((current.parent/'execution-current.json').read_bytes())
   results.append(dict(configuration=config,corpus=corpus,rows=count,exact_bytes=exact,mismatches=mismatch,
     reference=pin(reference.relative_to(ROOT).as_posix()),current=pin(current.relative_to(ROOT).as_posix()),outcomes=dict(Counter(r['outcome'] for r in b)),native_attempts=execution.get('native_attempts')))
 result=dict(status='EXACT_SOURCE_COMPATIBILITY_ONLY' if all(r['exact_bytes'] for r in results) else 'NOT_READY_04A38',label=label,rows_per_configuration=sum(COUNTS.values()),
  comparison='Exact UTF-8 TSV bytes, original order and every recorded field; separate Debug/Release oracles; no tolerance or NaN normalization; GUID entropy only under the frozen identity policy.',
  chronology='References sealed before first production edit; current executions after implementation; no late reference exception',results=results,
  registration='See per-configuration inspection receipts; native Excel registration NOT_RUN',scientific='NOT_GOLDEN',production_ready=False)
 save('compatibility/'+label+'/comparison.json',result)
 assert all(r['exact_bytes'] for r in results),'Current output differs from frozen reference'
 print('Exact28966 rows/configuration; compatibility does not establish implementation completeness')

if __name__=='__main__':compare(sys.argv[1])
