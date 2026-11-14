from common import *
source=HERE/'owner-review.md';old=source.read_bytes();target=HERE/'source-provisional/owner-review-preseal-v1.md';target.parent.mkdir(exist_ok=True)
with target.open('xb') as f:f.write(old)
text=old.decode().replace('entry-reconciliation-v1.json (or the exact retained entry receipts)','v12-independent-entry-reconciliation-v1.json').replace('inherited-failure-mutation-ledger-v1.json give','inherited-failure-mutation-ledger-v2.json give')
with source.open('w',encoding='utf-8',newline='\n') as f:f.write(text)
save('preseal-wording-transition-v1.json',dict(before=pin(target),after=pin(source),scope='Before checkpoint seal: resolve exact v12 entry receipt name and final append-only attempt ledger. Original unsealed wording retained byte-identically.',original_queries=0))
