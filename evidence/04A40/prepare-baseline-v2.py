"""Use the same canonical Survival record factory as shipping UDF; retain v1 malformed direct records."""
from common40_v3 import *
source=(HERE/'fixtures/Observe40.cs').read_text()
needle='var km=new Survival_KM_LR(rec);'
new="""string error=null;
  rec=Survival.CreatSurvivalData(times,Enumerable.Range(0,times.Length).Select(i=>noevents?0:(i==0||i==7?0:1)).ToArray(),
    Enumerable.Range(0,times.Length).Select(i=>i<6?\"A\":\"B\").ToArray(),
    Enumerable.Range(0,times.Length).Select(i=>stratified?(i%2==0?\"S1\":\"S2\"):\"ALL\").ToArray(),ref error);
  if(rec==null)throw new ArgumentException(error);
  var km=new Survival_KM_LR(rec);"""
assert source.count(needle)==1
with (HERE/'fixtures/Observe40v2.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(source.replace(needle,new))
script=(HERE/'capture-baseline-v1.py').read_text().replace('before-v1','before-v2').replace('before_v1','before_v2').replace('Observe40.cs','Observe40v2.cs').replace('baseline-fixture-compile-v1','baseline-fixture-compile-v2').replace('baseline-fixture-execute-v1','baseline-fixture-execute-v2').replace('frozen-fixtures-v1','frozen-fixtures-v2')
with (HERE/'capture-baseline-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(script)
save('baseline/invalid-direct-survival-records-v1.json',dict(original=pin(PRIVATE/'before-v1.json'),driver=pin(HERE/'fixtures/Observe40.cs'),
    disposition='Retain original48 observations, including direct-record group coding exceptions. Do not classify those malformed-record exceptions as the C13 defect. Add canonical-factory current-main cases before any production edit.',
    GEE_supplied_start='NullReferenceException reproduced independently in both cultures; unapproved Core correction remains dormant.'))
