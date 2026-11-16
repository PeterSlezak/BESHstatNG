from common46 import *
lock();plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';snapshot=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.22.md'
source=(HERE/'plan-and-freeze-v1.py').read_text(encoding='utf-8');source=source[source.index("pins=runner()") :]
source=source.replace("assert pins[name]==entry[name]","key=next(p for p in pins if p.casefold()==name.casefold());assert pins[key]==entry[key]")
exec(compile(source,str(HERE/'plan-and-freeze-v1.py'),'exec'))
save('matrix/source-freeze-v2.json',dict(load(HERE/'matrix/source-freeze-v1.json'),generation=2,first_preparation_failure='Case-sensitive dictionary lookup used a differently cased Windows project spelling;source/plan bytes retained;no heavy campaign ran against the failed preparation. Operationalv1 file is the first completed freeze,not an overwritten failure.'))
save('waves/failed-freeze-v1.json',dict(status='FAILED_PREPARATION_RETAINED',cause='Windows project spelling in source-pin dictionary;no source/test defect',successful=pin(HERE/'matrix/source-freeze-v2.json'),failed_recipe=pin(HERE/'plan-and-freeze-v1.py'),native='NOT_RUN'))
