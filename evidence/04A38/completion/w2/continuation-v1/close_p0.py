"""Append the verified P0 mini-dossier, then continue the existing W2 queue."""
from common import *
from verify_p0 import inspect
from baseline_report import python_receipt
def main():
 proof=inspect();test=python_receipt('w2c1-p0-adverse');assert test['total']==9 and test['exit_code']==0
 save('p0/independent-proof.json',proof);save('p0/adverse-tests.json',test)
 save('p0/readiness.json',dict(status='P0_CONTRACT_PASS_NOT_BATCH_READY',W2='IN_PROGRESS',W3='NOT_STARTED',historical_proof_supported=49,historical_OPEN=655,source_transitions=pin(HERE/'source-transitions-p0-v2.json'),proof=pin(HERE/'p0/independent-proof.json'),adverse=pin(HERE/'p0/adverse-tests.json'),source_reference_class='EXISTING_FROZEN_REFERENCES_ONLY',new_original_queries=0,failed_build_attempts=['build-p0-v1-Debug.json','build-p0-v1-Release.json','attempts/p0-common-name-resolution-v1.vb.txt']))
 p=HERE/'p0/owner-review.md'
 with p.open('x',encoding='utf-8') as f:f.write('''# P0 description/mask preparation PASS;04A.38 NOT_READY

`FormatMmrmAtProfileDescription` and the complete mask-preparation contract now reside in Common, consumed by actual Windows wrappers. Model arrays are obtained lazily by native callbacks; formatting is invoked through the existing native wrapper only after a positive mask. The formatter itself calls the unchanged accepted Core FormatProfileValue. It retains visit-first/insertion-order labels, null/empty behavior, nonfinite empty values, significant digits, signed zero, and original overflow/exception timing. Source-operation proof covers both full bodies and the previously verified Core mask loop; it does not infer closure from an edge alone.

Debug/Release production builds and268-export checks pass. Each configuration executes32 Common and17 actual shipping-adapter tests, including the two culture-specific native formatter routes. Nine independent temporary-copy adverse controls pass. Current-only replay compares all28966 genuine pre-edit and458 separately authorized late cases/configuration byte-exactly. No original model/query was run. The namespace-resolution build failure and its source/receipts remain preserved.

This adds two proof-supported historical bodies to the47 checkpoint:49/704 supported,655 OPEN. Supporting/statement/inline closure remains unfinished. W2 continues directly to profile application, grouped/nested imports and report preparation under existing authority. W3 is NOT_STARTED. P=NO;N1=NO GO;N2=NO;C2=UNMET;C3=UNAPPROVED;NOT_GOLDEN. No native Excel/R/Mac/manual/install/deploy or Git publication occurred.
''')
 print('P0 mini-dossier closed; continue W2 without permission stop')
if __name__=='__main__':main()
