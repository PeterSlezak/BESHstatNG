"""Append inherited pins and bounded frozen current evidence, without historical traversal."""
from common import *
oldpath=W2/'continuation-v15-retained-private-preservation-v2.json';postpath=W2/'continuation-v15-postseal-private-addendum-v2.json'
old=json.loads(oldpath.read_bytes());post=json.loads(postpath.read_bytes())
assert len(old['private_pins'])==21208 and len(old['retained_prior_physical_archives'])==101 and post['count']==31
rows={r['path']:r for r in old['private_pins']}
for r in post['postseal_private_pins']:
 if r['path'] in rows:assert rows[r['path']]==r
 rows[r['path']]=r
roots=[PRIVATE]
for folder in ['commands','builds','executions']:
 roots.extend(p for p in (RUNROOT/folder).glob('w2c16-*') if p.is_dir() and not p.name.startswith('w2c16-postseal-'))
bounded=[]
for root in sorted(roots):
 assert root.resolve().is_relative_to(RUNROOT.resolve())
 files=sorted(p for p in root.rglob('*') if p.is_file())
 names=[]
 for p in files:
  row=pin(p);names.append(row['path'])
  if row['path'] in rows:assert rows[row['path']]==row
  rows[row['path']]=row
 bounded.append(dict(path=root.relative_to(ROOT).as_posix(),files=names))
for r in json.loads((HERE/'review-exclusions-v1.json').read_bytes())['current_raw_excluded']:
 assert pin(ROOT/r['path'])==r;rows[r['path']]=r
archives={r['path']:r for r in old['retained_prior_physical_archives']}
indexpath=W2/'continuation-v16-review-parts-index-v1.json';index=json.loads(indexpath.read_bytes())
for p in index['parts']:
 for r in [{k:p[k] for k in ['path','bytes','sha256']},p['deterministic_reproduction']]:
  assert rows.get(r['path'])==r
  archives[r['path']]=r
doc=dict(status='IMMUTABLE_INHERITED_PINS_PLUS_BOUNDED_V16_CURRENT_EVIDENCE;ACTUAL_READER_REQUIRED',reader=pin(HERE/'verify_c16_private_v1.py'),previous=pin(oldpath),previous_postseal=pin(postpath),private_pins=sorted(rows.values(),key=lambda r:r['path']),private_pin_count=len(rows),inherited_private_count=21208,inherited_postseal_count=31,retained_physical_archives=sorted(archives.values(),key=lambda r:r['path']),retained_interrupted_aggregation=old['retained_interrupted_aggregation'],bounded_current_roots=bounded,parts_index=pin(indexpath),excluded_future_raw='Only explicitly separate w2c16-postseal command roots; later receipts and their pins form the final additive postseal manifest. Review-copy duplicate payload is independently bound by the physical archive index and actual copied-reader receipt.',ignored_TestResults='Never enumerated',original_queries=0)
path=W2/'continuation-v16-retained-private-preservation-v1.json'
with path.open('x',encoding='utf-8') as f:json.dump(doc,f,indent=2,ensure_ascii=False);f.write('\n')
print(json.dumps(dict(manifest=pin(path),private_pin_count=len(rows),physical_archives=len(archives),bounded_roots=len(bounded))),flush=True)
