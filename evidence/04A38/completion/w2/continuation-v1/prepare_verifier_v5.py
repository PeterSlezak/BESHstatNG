from common import *
s=(HERE/'verify_p1_v4.py').read_text()
s=s.replace("originals=load(root/'evidence/04A38/preedit/entry-source-snapshot.json')['files']", "for record in before_records:before(record['live']['path'])\n originals=load(root/'evidence/04A38/preedit/entry-source-snapshot.json')['files']")
with (HERE/'verify_p1_v5.py').open('x',encoding='utf-8') as f:f.write(s)
s=(HERE/'test_p1.py').read_text().replace('import verify_p1_v4 as reader','import verify_p1_v5 as reader')
with (HERE/'test_p1_v2.py').open('x',encoding='utf-8') as f:f.write(s)
save('attempts/p1-adverse-reader-v4.json',dict(exit_code=1,tests=14,passed=13,failed=1,failed_identity='test_p1.P1Tests.test_stale_preimage',test_source=pin(HERE/'test_p1.py'),reader=pin(HERE/'verify_p1_v4.py'),observed='Expected ValueError was not raised when only the pre-P1 Common profile preimage changed. Reader used the host preimages but did not read that particular portable preimage.',correction='v5 checks every before-record digest, including portable preimages, before source operation comparisons. Same 14 adverse conditions retained.',product_changed=False))
