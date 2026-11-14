"""Version inventory reconciliation; preserve exact C7 OPEN queue ordering."""
from common import *
source=(PREVIOUS/'reconcile_v2.py').read_text(encoding='utf-8')
source=source.replace('from verify_checkpoint_template_v2 import compute','from verify_c8_checkpoint_template_v3 import compute').replace('C7_FULL','C8_FULL').replace('C7_ALL','C8_ALL').replace('canonical_v6_support_id','canonical_v7_support_id')
start=source.index('priority=[r for r in remaining');end=source.index("save('remaining-work-queue-v1.json'",start)
source=source[:start]+"priority=remaining # Exact inherited machine order, filtered only by proved CLOSED IDs.\n"+source[end:]
source=source.replace("new_current_only_helpers=1","new_current_only_helpers=2")
source=source.replace("first_action='Continue remaining sample-size bodies in the exact C6 machine queue order with their report/input/option dependencies, then remaining range-reader parents. All24 C6 priority identities are proved. No new original reference queries.'", "first_action='Continue only ordinary approved W2 using this exact filtered C7 canonical queue. All24 C7 priorities are proved; no source-position guesses, new reference queries, W3 or04A39 authority.'")
source=source.replace('verified=compute()','verified=proof # Draft counts; sealed reader will independently check ledgers and validation at final boundary.')
source=source.replace("['status','historical','current','support','fragments','W3']","['status','historical','current','support','fragments','W3']")
with (HERE/'reconcile_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('reconciliation-tool-crosswalk-v1.json',dict(before=pin(PREVIOUS/'reconcile_v2.py'),after=pin(HERE/'reconcile_v1.py'),scope='All704 original canonical records, all726 current declarations,325 raw/303 distinct support and both full fragment populations derive from source censuses. Keep exact inherited OPEN queue order. Full sealed reader independently rechecks every ledger after validation.',original_queries=0))
