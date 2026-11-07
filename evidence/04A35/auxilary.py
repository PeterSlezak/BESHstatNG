"""Actual isolated auxiliary receipts; immutable fresh names per invocation."""
from run import m,ROOT
import sys
r=m.Runner(ROOT/'.test-rationalization-04A35.tmp');action=sys.argv[1]
if action=='boundary':
    results=[r.test(c,'final','Legacy','boundary-'+c,'FullyQualifiedName~MigrationRefEditBoundary10Tests.ActualFocusEnterContainsThrowingClearAndPreservesAddress') for c in ['Debug','Release']]
elif action=='seriallegacy':
    results=[r.test(c,'final','Legacy','serial-'+c+'-Legacy') for c in ['Debug','Release']]
elif action=='audits':results=[r.command(sys.argv[2]+'-packet-audits',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A35','-p','test_audit.py','-v'])]
elif action=='verify':results=[r.command(sys.argv[2]+'-independent-verifier',[sys.executable,'-B','evidence/04A35/verify_packet.py'])]
elif action=='old-audits':results=[r.command(sys.argv[2]+'-accepted34-audits',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A34','-p','test_audit.py','-v'])]
else:raise ValueError(action)
sys.exit(1 if m.failed(results) else 0)
