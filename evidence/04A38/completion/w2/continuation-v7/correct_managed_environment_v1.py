"""Retain first full run; correct only four environment-triggered fixture tests."""
from common import *
from collections import Counter
before=m.source_pins();runner=m.Runner(RUNROOT)
result=runner.test('Debug','w2c7-checkpoint-v1','Common','w2c7-corrected-fixture-env-v1','FullyQualifiedName~MigrationFixture04A02Tests.TypedService_CandidateObservationOnly')
save('managed-environment-correction-v1.json',dict(result=clean(result),first_full_run='managed-full-execution-v1.json',first_raw_TRX=pin(RUNROOT/'executions/w2c7-checkpoint-v1-Debug-serial-Common/results/Common.trx'),correction='Use unchanged standard runner; BESHSTAT_04A02_OBSERVATIONS is empty. The full-suite optimization in validate_managed_v1.py enabled that variable for two unrelated observation classes. Four MigrationFixture cases reject paths outside their explicit TestResults/04A-02-initial-fixtures-* contract. Only the four rejected cases are rerun; the full initial run and its18 nonpasses are retained.',source_unchanged=before==m.source_pins(),Release='NOT_RUN_OWNER_PREFERENCE'))
assert result['outcomes']==dict(Passed=4) and result['command']['exit_code']==0 and before==m.source_pins()
