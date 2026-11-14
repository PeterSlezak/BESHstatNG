from common import *
r=command('final-python-privacy13-after-encoding',[sys.executable,'-B','-m','unittest','discover','-s','scripts/tests',
    '-p','test_04a13_survival_workflow.py','-k','test_complete_existing_publication_set_has_no_unresolved_private_values','-v'])
save('privacy13-after-encoding-command.json',dict(command=clean(r),scope='One unchanged inherited privacy assertion after exact-byte relocation of the new C5 UTF16 diagnostic; raw FAIL remains a FAIL, never waived',
    original_queries=0,gate='Use actual identity/outcome/occurrence replacement only if full-suite raw record shows the known encoding/cached-path ERROR'))
