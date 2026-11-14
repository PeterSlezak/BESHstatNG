from common import *
text=(HERE/'write_reviews.py').read_text(encoding='utf-8')
for name in ['validation','ownership-summary','fragment-coverage','remaining-work-queue','body-contracts','current-ownership','historical-ownership','original-fragment-parent-crosswalk','per-family-ownership','readiness','command-receipts','git-closeout','preservation-final','nonpasses']:
    text=text.replace(name+'-v2.json',name+'-v3.json')
text=text.replace('source-transitions-v3.json','source-transitions-v4.json').replace('cells-v4','cells-v5').replace('verify_slice_v2.py','verify_slice_v4.py').replace('verify_ownership_v2.py','verify_ownership_final.py').replace('verify_preservation_v2.py','verify_preservation_v3.py')
text=text.replace('independent-controls-final.json','independent-controls-boundary.json').replace('retained-C4-controls.json','retained-C4-controls-v2.json')
text=text.replace('inspection/final-v3/','inspection/final-v4/').replace('dependency-metadata-v8/current-','dependency-metadata-v9/current-').replace('current-universe-final-v3.json','current-universe-final-v4.json')
text=text.replace('production-consumer-edges-v2.json','production-consumer-edges-v3.json').replace('entry-discrepancies-v2.json','entry-discrepancies-v3.json')
text=text.replace('three actual Windows consumers','eight actual Windows consumer edges: three conversion/classification consumers and five prepared phases')
text=text.replace('1,773','1,763').replace('except exactly the three deliberately changed native conversion/classification methods','except the13 explicitly changed native/public portable methods. One compiler-generated closure type is rebound by an exact name alias; every field, constructor, callback body and caller matches after that single substitution')
text=text.replace('11 added PASS','14 added PASS').replace('|27 PASS|','|32 PASS|').replace('27 final C5','32 final C5')
text=text.replace('run_full_validation_v3.py managed','run_full_validation_v4.py managed').replace('run_full_validation_v3.py common-python','run_full_validation_v4.py common-python').replace('run_full_validation_v2.py serial','run_full_validation_v4.py serial').replace('focused_v2.py cells-v5','focused.py cells-v5')
marker='The complete scalar/header/grouped/alpha/format/table contracts'
text=text.replace(marker,'The no-reset prepared phases also preserve output state after native acquisition; existing public Common APIs retain their original entry-reset behavior. '+marker)
marker='Full grouped extraction preserves'
paragraph='''A reentrant-conversion probe found a source-backed extraction defect in TryGetBinary01, TryGetWholeNumber and TryGetOptionalPositiveInteger: a custom converter can write the caller output before failed validation, and the delegated Common API reset erased that write. The original native body has no second reset. Final prepared phases preserve those writes while existing public portable APIs retain their reset wrappers. All10 current-only alias cases/configuration now retain17; adverse pre-fix results of-1,0 or10 remain recorded. No archived-original method was invoked. Grouped/nested native acquisition uses the same no-second-reset boundary. Five whole public API bodies are token-identical after inlining their prepared phase; four prepared bodies also match complete former CIL after removing only the leading reset, including rebased branch and existing For Each disposal-Finally regions. Earlier C5 assertions that no observable operation intervened were wrong; those provisional proofs remain as adverse attempts and are superseded by the final source/CIL/runtime proof.

'''
text=text.replace(marker,paragraph+marker)
marker='The unmodified canonical verify-beshstatng.ps1 wrappers'
paragraph='''The raw final Python aggregate was765 PASS/179 FAIL/12 ERROR. One privacy identity hit the early C5 UTF16 diagnostic/cached former path. Its exact bytes were relocated locally, with an identical-value UTF8 equivalent and explicit ledger; the unchanged targeted assertion was rerun and actually FAILS, restoring its inherited outcome. The corrected final aggregate is765/180/11, with the only remaining change from C4 being the same unordered-SPC identity moving FAIL to ERROR. Every raw attempt and occurrence remains in python-encoding-correction-final.json; no privacy assertion was waived. Diagnostic-encoding-relocation.json and the local diagnostic preservation proof bind the omitted raw bytes.

'''
text=text.replace(marker,paragraph+marker)
text=text.replace('Four previously modified source/test files plus one new Common source','Seven existing source/test files plus one new Common source')
text=text.replace('Exact retained preimages for the four','Exact retained preimages for the seven')
# Reader/export uses a frozen byte-key privacy cache only in the live producer;
# the copied executable always performs its own cold scan.
text=text.replace('The external continuation-v5-publication-receipt-v1.json records','The external continuation-v5-publication-receipt-v1.json records')
for old,new in [('supports33','supports 33'),('incomplete41-source','incomplete 41-source'),('All3,777','All 3,777'),('all268','all 268'),('all4,695','all 4,695'),('all35','all 35'),('All35','All 35'),('all41','all 41'),('all704','all 704'),('All704','All 704'),('all4695','all 4695'),('All4695','All 4695'),('Remaining505','Remaining 505'),('Remaining538','Remaining 538'),('remaining505','remaining 505'),('remain505','remain 505'),('remain717','remain 717'),('original41','original 41'),('full41','full 41'),('all24','all 24'),('All24','All 24'),('all100','all 100'),('no04A','no 04A'),('or04A','or 04A'),('and04A','and 04A'),('and41','and 41'),('and717','and 717'),('and16','and 16'),('and source','and source'),('fixed.05','fixed 0.05'),('only2','only 2'),('byref','byref')]:
    text=text.replace(old,new)
with (HERE/'write_reviews_final.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
