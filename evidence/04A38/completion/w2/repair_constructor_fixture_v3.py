"""Use an actual cache-owning module constructor; retain invalid v2 control."""
from capture_current import *
s=(HERE/'test_independent_slice_v2.py').read_text(encoding='utf-8').replace("r['method']=='BESHStatNG.UdfDataImport..cctor()'","r['method']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs..cctor()'")
with (HERE/'test_independent_slice_v3.py').open('x',encoding='utf-8') as f:f.write(s)
with (HERE/'adverse-fixture-attempt-v2.json').open('x',encoding='utf-8') as f:json.dump(dict(status='INVALID_CTOR_FIXTURE_NOT_ACCEPTED',executed=17,nominal_pass=16,errors=1,reason='Constructor corruption control selected UdfDataImport, which has no static constructor. StopIteration was a test setup error, not an accepted rejection.',repair='Target actual MultivariateAnalysisUDFs cache-owning module constructor; keep all assertions and complete intact-reader preflight.'),f,indent=2)
r=m.Runner(RUNROOT).command('w2-independent-slice-adverse-v3',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_independent_slice_v3.py','-v']);assert r['exit_code']==0
