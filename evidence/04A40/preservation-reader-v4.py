"""Authenticate committed owner line-ending transitions and preserve exact originals."""
from pathlib import Path
here=Path(__file__).parent
s=(here/'preservation-v2.py').read_text(encoding='utf-8')
s=s.replace('physical_drift=[];checkpoint=', 'physical_drift=[];owner_line_endings=[];checkpoint=')
old="assert pin(r['path'])=={k:r[k] for k in ('path','bytes','sha256')}"
new="""now=pin(r['path'])
    if now!={k:r[k] for k in ('path','bytes','sha256')}:
        original=(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint'/r['path']).read_bytes()
        live=(ROOT/r['path']).read_bytes();committed=git('show',MAIN+':'+r['path'])
        assert sha(original)==r['sha256'] and live==committed and original.replace(b'\\r\\n',b'\\n')==live.replace(b'\\r\\n',b'\\n'),'UNAUTHORIZED_HISTORICAL_CHANGE'
        owner_line_endings.append(dict(original={k:r[k] for k in ('path','bytes','sha256')},committed=now,exact_original_copy=pin(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint'/r['path']),authorization='Prior explicit owner line-endings authorization; already in authenticated main before40, not changed by40.'))"""
assert s.count(old)==2;s=s.replace(old,new,1)
s=s.replace("assert pin(r['path'])=={k:r[k] for k in ('path','bytes','sha256')},'PROTECTED_RECORD_CHANGED'", "assert pin(r['path'])=={k:r[k] for k in ('path','bytes','sha256')},'PROTECTED_RECORD_CHANGED'")
s=s.replace("historical-readback-v2.json", "historical-readback-v3.json").replace("physical-file-id-drift-v2.json", "physical-file-id-drift-v3.json")
s=s.replace('original_bytes_exact=True', "original_bytes_preserved=True,live_original_exact=468-len(owner_line_endings),owner_line_ending_transitions=owner_line_endings")
s=s.replace("'NOT_VERIFIABLE_FILE_ID_DRIFT;OWNER_PROVENANCE_REVIEW_REQUIRED'", "'LIVE_FILE_IDS_DIFFER_FROM_PRECOMMIT_RECEIPT;CONTINUOUS_IDENTITY_NOT_VERIFIED;EXACT_ORIGINAL_BYTES_RETAINED' ")
(here/'preservation-v3.py').open('x',encoding='utf-8').write(s)
