"""Separate identical KM lambdas by actual full unchanged parent bytes and UTF16 spans."""
from common import *
source=HERE/'entry_queue_v1.py';text=source.read_text(encoding='utf-8')
needle="    assert len(candidates)==1,'AMBIGUOUS_CANONICAL_ID_STOP '+h['canonical_original_id']"
assert needle in text
replacement="""    if len(candidates)>1 and o['parent_member']:
        op=om[o['parent_member']]
        original_parent,original_view=body((ROOT/snapshots[op['path']]['path']).read_bytes(),op)
        for parent_id in {n['parent_member'] for n in candidates}:
            cp=cm[parent_id];current_parent,current_view=body((ROOT/cp['path']).read_bytes(),cp)
            assert current_parent==original_parent and cp['body_sha256']==op['body_sha256'],'CHANGED_PARENT_CANNOT_RESOLVE_CALLBACK_STOP'
        relative=(o['span_start']-op['span_start'],o['span_length'])
        candidates=[n for n in candidates if (n['span_start']-cm[n['parent_member']]['span_start'],n['span_length'])==relative]
    assert len(candidates)==1,'AMBIGUOUS_CANONICAL_ID_STOP '+h['canonical_original_id']"""
target=HERE/'entry_queue_v2.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(text.replace(needle,replacement))
# Explicit diagnostic repeat of the unchanged rejected verifier retains actual
# raw command/stdout/stderr. It performs no unit tests or original execution.
result=command('entry-queue-v1-diagnostic-replay',[sys.executable,'-B',str(source)])
assert result['exit_code']==1
folder=RUNROOT/'commands/w2c13-entry-queue-v1-diagnostic-replay'
assert 'AMBIGUOUS_CANONICAL_ID_STOP' in (folder/'stderr.log').read_text(encoding='utf-8')
save('entry-queue-reconciliation-correction-v1.json',dict(status='UNDERDISCRIMINATED_CALLBACK_RESOLVER_REJECTION_RETAINED_SOURCE_BACKED_SUCCESSOR',before=pin(source),after=pin(target),
    first_attempt='Initial direct tool execution exited1 with the same AMBIGUOUS_CANONICAL_ID_STOP at entry_queue_v1.py line48, called at line71. Its exact tool output remains in conversation. The explicitly identified unchanged diagnostic repeat below retains raw process bytes; it is not relabelled as the first execution.',
    actual_diagnostic_repeat={name:pin(folder/name) for name in ['command.json','stdout.log','stderr.log']},
    source_authority='Both candidate parents have the same full source bytes and SHA256 as the archived parent. Exact relative UTF16 spans466:50 and1457:50 resolve the two bodies; the successor independently binds separate original/current source sibling positions to actual ldftn CIL targets.',
    guessed_ids=0,original_queries=0,production_edits=0,tests_rerun=0))
print('Exact full-parent/UTF16 callback discrimination added; original/current actual CIL binding remains mandatory')
