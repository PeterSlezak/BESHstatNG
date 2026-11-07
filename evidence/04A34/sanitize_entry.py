"""Correct new entry-ledger path privacy only; preserve captured source digests."""
from entry import ROOT,HERE,save,sha,public
import json
if __name__=='__main__':
    p=HERE/'entry-pins.json'
    private=ROOT/'.test-rationalization-04A34.tmp/entry-pins-original-private.json'
    original=private.read_bytes() if private.exists() else p.read_bytes()
    if not private.exists():private.write_bytes(original)
    value=json.loads(original);redacted=[]
    for row in value['files']:
        old=row['path'];new=public(old)
        if new!=old:row['path']=new;redacted.append(dict(public_path=new,captured_source_sha256=row['sha256']))
    save('entry-pins.json',value)
    save('entry-ledger-privacy-correction.json',dict(status='CORRECTED_NEW_ARTIFACT_PATH_REDACTION',original_public_artifact_sha256=sha(original),redacted_public_artifact_sha256=sha(p.read_bytes()),redacted_paths=redacted,source_hashes_changed=False,private_original_retained='ISOLATED_RUN_ONLY;not a publication artifact',raw_baseline_effect='Two real privacy FAILs in raw before-product Python execution were caused by this new ledger. Their old receipts are preserved. They are not inherited product failures. Targeted and full final reruns are required.'))
    print('Sanitized',len(redacted),'path labels; captured source digests unchanged')
