"""Recheck entry bytes, accepted lineage and protected small captures only.

Never enumerate/hash the ignored historical TestResults tree. Authoritative
post-merge file bytes are distinguished from inverse Git line-ending proofs.
"""
import json
from preflight import ROOT, HERE, BASE, git, pin, reconstruct, save, sha


def main():
    require = lambda value: value or (_ for _ in ()).throw(AssertionError('Entry preservation failed'))
    require(git('rev-parse','HEAD').decode().strip()==BASE)
    require(not git('diff','--name-only') and not git('diff','--cached','--name-only'))
    entry=json.loads((HERE/'entry-source-pins.json').read_bytes())
    preserved=[]
    for row in entry['files']:
        if row['path'].startswith('<PRIVATE_PROTECTED_PATH:'):
            # Preflight preserved these original pins; resolve privately from Git.
            matches=[p for p in git('ls-files','-z').decode().split('\0') if p and '<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>'==row['path']]
            require(len(matches)==1)
            actual=pin(matches[0]);actual['path']=row['path']
        else:actual=pin(row['path'])
        require(actual==row);preserved.append(actual)
    prior=json.loads((ROOT/'evidence/04A37/lineage-check.json').read_bytes())
    require(len(prior['pins'])==278)
    lineage=[]
    for row in prior['pins']:
        data=(ROOT/row['path']).read_bytes()
        # Same limited inverse encodings as the accepted merged-main entry proof.
        lf=data.replace(b'\r\n',b'\n');crlf=lf.replace(b'\n',b'\r\n')
        variants={'DIRECT':data,'LF':lf,'CRLF':crlf,'CRLF_EXCEPT_FINAL_LITERAL_LF':crlf[:-2]+b'\n' if crlf.endswith(b'\r\n') else crlf}
        matches=[name for name,value in variants.items() if sha(value)==row['final37_sha256']]
        require(matches)
        lineage.append(dict(path=row['path'],accepted37_sha256=row['final37_sha256'],merged_main=pin(row['path']),inverse_encoding_proof=matches))
    captures=json.loads((ROOT/'evidence/04A37/mixed-model-captures-check.json').read_bytes())['files']
    require(len(captures)==100)
    capture_records=[]
    for row in captures:
        actual=pin(row['path'])
        require(actual['sha256']==row['sha256'])
        if 'bytes' in row:require(actual['bytes']==row['bytes'])
        capture_records.append(actual)
    authority=json.loads((HERE/'entry-authority.json').read_bytes())
    # The receipt field name is an entry record; it is not inferred from current docs.
    owners=authority.get('owner_documents_excluded',authority.get('owner_documents',[]))
    if not owners:
        owners=json.loads((ROOT/'evidence/04A37/entry-authority.json').read_bytes())['owner_documents_excluded']
    require(len(owners)==16)
    for row in owners:require(pin(row['path'])==row)
    save('preservation-entry.json',dict(status='PASS_BEFORE_PRODUCT_EDITS',source_commit=BASE,
        tracked_entry_files=len(preserved),tracked_exact=len(preserved),lineage_pins=lineage,
        protected_mixed_model_captures=capture_records,owner_documents_excluded=owners,
        historical_test_results='NOT_REHASHED; metadata/pinned small captures only; no historical cleanup',
        native='NOT_RUN',scientific='NOT_GOLDEN'))
    print('PASS',len(preserved),'tracked entry files;278 lineage pins;100 protected captures;16 owner documents; no product edits')


if __name__=='__main__':main()
