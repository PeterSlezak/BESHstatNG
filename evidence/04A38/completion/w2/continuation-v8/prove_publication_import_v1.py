"""Directed actual-process import binding control, separate from test populations."""
from common import *
if len(sys.argv)>1:
    if sys.argv[1]=='wrong':
        import importlib.util
        path=W2/'continuation-v6/verify_validation_v1.py';spec=importlib.util.spec_from_file_location('verify_validation_v1',path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);sys.modules['verify_validation_v1']=module
    try:import verify_c8_publication_v4 as publication
    except ValueError as error:
        assert sys.argv[1]=='wrong' and str(error)=='Wrong inherited validation helper module';print('REJECTED_WRONG_CACHED_HELPER');raise SystemExit(0)
    assert sys.argv[1]=='positive';print('EXACT_C7_HELPER_PATH_AND_SEALED_HASH');raise SystemExit(0)
rows=[]
for mode in ['positive','wrong']:
    r=command('publication-import-'+mode+'-v1',[sys.executable,'-B',str(HERE/'prove_publication_import_v1.py'),mode]);rows.append(clean(r));assert r['exit_code']==0
save('publication-import-controls-v1.json',dict(status='EXACT_INHERITED_BINDING_AND_WRONG_CACHED_MODULE_REJECTION_PASS',commands=rows,verifier=pin(HERE/'verify_c8_publication_v4.py'),actual_C7_helper=pin(PREVIOUS/'verify_validation_v1.py'),production_changes=0,original_queries=0))
