"""No fixture edits: correct path spelling for remaining historical readers."""
from common import *
view=RUNROOT/'w2-before-v2';records=[]
for label,relative in [('D','evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards'),('E','evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile'),('F','evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'),('G','evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide'),('W1','evidence/04A38/completion/w1')]:
 start='\\\\?\\'+str(view/relative)
 r=command('early-reader-'+label+'-longpath-v2',[sys.executable,'-B','-m','unittest','discover','-s',start,'-p','test_*.py','-v'],cwd=view);records.append(dict(label=label,command=clean(r)))
save('early-longpath-replays.json',dict(results=records,original_queries=0,source_expectations_changed=False,method='Only Windows extended-length namespace. Same authenticated pre-W2 inputs/readers; first normal-path setup errors retained. Broad W1 includes its deliberately archived invalid v1 fixture; distinguish it from canonical v2.'))
