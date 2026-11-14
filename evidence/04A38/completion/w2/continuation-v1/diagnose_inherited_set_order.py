"""Retain both observed outcomes of an unchanged inherited source audit."""
from common import *
from baseline_report import python_receipt
records=[]
for seed in ['0','1']:
 label='inherited-spc-order-seed'+seed
 r=command(label,[sys.executable,'-B','-m','unittest','test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact','-v'],cwd=ROOT/'scripts/tests',env={'PYTHONHASHSEED':seed})
 records.append(dict(seed=seed,command=clean(r),receipt=python_receipt('w2c1-'+label)))
save('inherited-spc-order-diagnostic.json',dict(status='INHERITED_UNORDERED_SOURCE_TEST_NONPASS_NOT_PRODUCT_EDIT',records=records,first_complete_run='ERROR:KeyError AddZoneLine',final_complete_run='FAIL:ComputeYAxisBounds assertion',source_reason='The unchanged test loops for name in set(a)-changed. Different hash iteration order encounters either the preexisting missing method or the preexisting changed method first. Current Spc renderer is protected unchanged by the live4695 ledger.',expectations_modified=False,normalization='NONE;final766PASS/180FAIL/10ERROR is reported exactly, not rewritten to the older767/178/11 receipt.'))
