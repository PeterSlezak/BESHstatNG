"""Use Windows extended-length paths for unchanged A adverse fixtures."""
from common import *
view=RUNROOT/'w2-before-v2';start='\\\\?\\'+str(view/'evidence/04A38/late-archived-entry-supplement/v1')
r=command('early-reader-A-longpath-v2',[sys.executable,'-B','-m','unittest','discover','-s',start,'-p','test_*.py','-v'],cwd=view)
save('early-A-longpath-replay.json',dict(command=clean(r),original_queries=0,prior_attempt='early-reader-A: Windows long-path fixture errors retained',source_or_expectation_changes=False,environment='Only extended-length Windows pathname spelling; same source files/fixture conditions'))
