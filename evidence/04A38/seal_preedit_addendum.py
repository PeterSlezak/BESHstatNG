"""Seal additional genuine pre-production coverage without changing first freeze."""
from datetime import datetime,timezone
import json
from preflight import ROOT,HERE,BASE,GATES,git,pin,save
from preedit_addendum_reader import inspect,EXPECTED


def main():
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==BASE
    assert git('branch','--show-current').decode().strip()=='codex/04A-38'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    assert not git('diff','--check')
    result=inspect(ROOT)
    files=[]
    for version in list(EXPECTED)+['shipping-helpers-Debug-v2','shipping-helpers-Release-v2']:
        files.extend(pin(p.relative_to(ROOT).as_posix()) for p in sorted((HERE/'preedit/attempts'/version).rglob('*')) if p.is_file())
    for relative in ['preedit_addendum_reader.py','public_option_cases.py','primitive_export_cases.py','model_input_cases_v2.py','capture_model_inputs_v2.py','math_edge_cases.py','capture_extra_cases.py','shipping_helper_capture.py','member_preparation_plan.py','udf-member-preparation-plan-v1.csv','member-preparation-plan-v1.json','seal_preedit_addendum.py']:
        files.append(pin('evidence/04A38/'+relative))
    save('preedit/addendum-frozen-v1/receipt.json',dict(status='PRE_EDIT_FROZEN_ADDITIONAL_COVERAGE',source_commit=BASE,
        sealed_at_utc=datetime.now(timezone.utc).isoformat(),chronology='CAPTURED_AND_SEALED_BEFORE_FIRST_04A38_PRODUCT_EDIT',
        first_valid_freeze=pin('evidence/04A38/preedit/frozen-v2/receipt.json'),first_freeze_preserved=True,
        original_rows_per_configuration=24732,additional_rows_per_configuration=4234,total_rows_per_configuration=28966,
        source_unchanged_at_seal=True,index_empty=True,late_capture_exception=False,
        independent_controls=result,files=files,gates=GATES,native='NATIVE_EXCEL_NOT_RUN',scientific='NOT_GOLDEN',
        scope='Public option consumer branches, typed public scalar cases, independent fit/weight/offset/formula/identity transactions, numerical guard/overflow edges and actual safe shipping-helper corroboration.',
        qualification='Additional references were captured after the first freeze receipt but BEFORE any production edit. This is not a late archived-entry oracle and uses no04A37 exception. Earlier attempts, first freeze and all negative results remain unchanged.',
        readiness='NOT_READY_04A38; preparation plan has626 pending extraction/consumption reviews; implementation and final packet/validation remain required.'))
    print('SEALED authentic pre-production additional coverage;',pin('evidence/04A38/preedit/addendum-frozen-v1/receipt.json')['sha256'])


if __name__=='__main__':main()
