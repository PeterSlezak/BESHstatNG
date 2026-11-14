"""Seal a local blocked continuation packet; no reference execution/publication."""
import json
from check_checkpoint import ROOT,HERE,pin,save

def main():
 checkpoint=json.loads((HERE/'entry.json').read_bytes())
 for row in checkpoint['checkpoint_payloads']:assert pin(row['path'])==row
 paths={row['path'] for row in checkpoint['checkpoint_payloads']}
 paths.add('evidence/04A38/publication-manifest.json')
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 self_path='evidence/04A38/continuation/v1/publication-manifest.json'
 inventory_path='evidence/04A38/continuation/v1/file-inventory.json'
 paths.update([self_path,inventory_path])
 save('file-inventory.json',dict(status='BLOCKED_CONTINUATION_LOCAL_REVIEW_ONLY',paths=sorted(paths),checkpoint_external_files=611,
  ignored_checkpoint_payloads_included=79,owner_documents_excluded=16,reference_generation='NONE'))
 entries=[dict(path=p,bytes=None,sha256=None) if p==self_path else pin(p) for p in sorted(paths)]
 save('publication-manifest.json',dict(status='NOT_READY_04A38',scope='VERSIONED_BLOCKED_CONTINUATION; NO_PRODUCTION_OR_REFERENCE_CHANGES',
  checkpoint_manifest=checkpoint['checkpoint_manifest'],entries=entries))
 print('Sealed blocked continuation:',len(entries)-1,'external hashes +1 null self-entry; root611-payload checkpoint unchanged')

if __name__=='__main__':main()
