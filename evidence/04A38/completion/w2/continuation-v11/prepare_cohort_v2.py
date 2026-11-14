from common import *
source=(HERE/'inspect_cohort_v1.py').read_text(encoding='utf-8')
source=source.replace("['FA_DROP','KMEANS_DROP','HCLUST_DROP','CA_DROP']", "['FA_FIT','FA_DROP','KMEANS_DROP','HCLUST_DROP','CA_DROP']")
source=source.replace("'cohort-diagnostic-v1.json'", "'cohort-diagnostic-v2.json'")
with (HERE/'inspect_cohort_v2.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(source)
save('cohort-scope-v2.json',dict(before=pin(HERE/'inspect_cohort_v1.py'),after=pin(HERE/'inspect_cohort_v2.py'),
    change='Explicitly include the already-supported unchanged FA_FIT as the real model/score/cache publication dependency, in addition to all family DROP lifecycle paths. No unrelated new closure.',original_queries=0))
