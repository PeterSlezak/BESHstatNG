"""Independent verification of diagnostic-label-only public derivation."""
import hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
def inspect(root=ROOT):
 root=Path(root);here=root/HERE.relative_to(ROOT);r=json.loads((here/'diagnostic-public-derivative.json').read_bytes());public=(root/r['public']['path']).read_bytes();assert hashlib.sha256(public).hexdigest()==r['public']['sha256'];data=json.loads(public)
 for c in r['changes']:
  x=data
  for part in c['pointer']:x=x[part]
  assert c['pointer'][-1]=='test_name' and hashlib.sha256(x.encode()).hexdigest()==c['public_label_sha256']
 source=root/r['raw_local']['path']
 if source.is_file():
  raw=source.read_bytes();assert hashlib.sha256(raw).hexdigest()==r['raw_local']['sha256'];old=json.loads(raw)
  for c in r['changes']:
   x=old
   for part in c['pointer'][:-1]:x=x[part]
   key=c['pointer'][-1];assert hashlib.sha256(x[key].encode()).hexdigest()==c['original_label_sha256'];x[key]=c['public_label']
  assert old==data
 return dict(status='DIAGNOSTIC_DISPLAY_LABEL_DERIVATIVE_PASS',changed_test_display_labels=len(r['changes']),raw_local_rechecked=source.is_file(),scientific_reference_byte_changes=0,copy_limitation='Raw diagnostic intentionally excluded; its original digest alone cannot reproduce omitted bytes outside live repository')
if __name__=='__main__':print(json.dumps(inspect()))
