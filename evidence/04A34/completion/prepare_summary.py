"""Retain partial receipts; derive final summaries from actual new isolated runs."""
from work import *
s=(HERE.parent/'summarize.py').read_text(encoding='utf-8')
s='import sys\nfrom pathlib import Path\nsys.path.insert(0,str(Path(__file__).resolve().parent.parent))\n'+s
s=s.replace("f'executions/final-","f'executions/completed-")
s=s.replace("f'executions/completed-{config}-{suite}/cases.json'","f'executions/review3-{config}-{suite}/cases.json'")
s=s.replace("f'executions/completed-{config}-focused-{suite}/cases.json'","f'executions/review3-{config}-focused-{suite}/cases.json'")
s=s.replace("python_run('final',suffix)","python_run('completed',suffix)")
s=s.replace("    command_name=f'{label}-Debug-{suffix}'","    command_name=f'{label}-Debug-{suffix}'\n    if label=='completed' and suffix=='python-scripts':command_name='review-Debug-python-scripts'")
s=s.replace("['before','final']","['before','completed']").replace("r['label']=='final'","r['label']=='completed'")
s=s.replace("('final-'+k)","('completed-'+k)")
s=s.replace("'managed-test-identities.csv'","'managed-test-identities-final.csv'")
# The accepted reference remains accepted33's historical filename.
s=s.replace("ROOT/'evidence/04A33/managed-test-identities-final.csv'","ROOT/'evidence/04A33/managed-test-identities.csv'")
for name in ['nonpasses','source-and-node-validation','command-outcomes']:
    s=s.replace("'"+name+".json'","'"+name+"-final.json'")
s=s.replace("return 'UNCLASSIFIED'","return 'UNCLASSIFIED'")
s=s.replace("decision='NOT_READY_04A34'","decision='VALIDATION_PENDING_FINAL_PACKET'")
s=s.replace("'Unfinished member dispositions and unexercised options remain OPEN_BLOCKER. Source/fixture numeric tests are software compatibility, not scientific independence.'","'All candidate dispositions reconciled; software compatibility and source evidence are not scientific independence or native rendering.'")
s=s.replace("'No R/native/WASM UI run; no claim of complete seventeen-family extraction.'","'No R/native/Office/WASM UI run; seventeen basic family responsibilities accounted for; specialized later-batch work remains reserved.'")
# No numerical or historical assertion is altered: this handles an authorized
# additional private duplicate helper removal in the same source-only test.
s=s.replace("ComputeYAxisBounds and private HasNaturalZero moved to SpcAxisBoundsCore.","ComputeYAxisBounds/private HasNaturalZero and basic optional/constant/axis preparation moved to Core/Common; detailed native annotations unchanged.")
needle="            elif 'test_04a26_planning_agreement.PlanningAgreement26Contracts.test_old_paired_and_import_transport_authority_unchanged' in identity:"
s=s.replace(needle,"""            elif 'test_04a26_planning_agreement.PlanningAgreement26Contracts.test_new_portable_code_is_strict_and_host_free' in identity:
                paths=['BESHStat.Excel.Common/src/Analysis/AgreementSeriesPreparation34.vb'];classification='AUTHORIZED_ADDITIVE_POLICY_CENSUS_RETAINED_FAIL';proof='scripts/tests/test_04a26_planning_agreement.py:129 expects exactly eight Agreement/SampleSize sources; consumed basic AgreementSeriesPreparation34 makes nine. Existing eight are unchanged; dependency/Strict checks independently pass.'
"""+needle)
write('evidence/04A34/completion/summarize_final.py',s)
p=HERE.parent/'preservation.py';s=p.read_text(encoding='utf-8').replace('Authorized partial04A34','Authorized completed04A34').replace('AUTHORIZED_04A34_PARTIAL','AUTHORIZED_04A34').replace("status='PARTIAL_BATCH_INVENTORY'","status='COMPLETE_BATCH_INVENTORY'").replace('source-and-node-validation.json','source-and-node-validation-final.json')
write(p.relative_to(ROOT),s)
print('Prepared final receipt reader; historical/partial raw receipts retained')
