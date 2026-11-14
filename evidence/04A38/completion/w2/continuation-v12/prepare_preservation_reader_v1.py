"""Extend immutable predecessor/source transition graph; retain every prior authority."""
from common import *
source=PREVIOUS/'verify_c11_preservation_v1.py';value=source.read_text(encoding='utf-8')
value=value.replace("C11=W+'continuation-v11/'", "C11=W+'continuation-v11/';C12=W+'continuation-v12/'")
old="transitions=load(root/(C11+'source-transitions-final-v1.json'))['files']"
new="c11trans=load(root/(C11+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C12+'source-transitions-final-v1.json'))['files']"
assert old in value;value=value.replace(old,new).replace('c10trans+c9trans','c11trans+c10trans+c9trans')
manifests=[PREVIOUS/'publication-manifest-v1.json',W2/'continuation-v11-logical-publication-manifest-v2.json',W2/'continuation-v11-logical-publication-manifest-v3.json']
insert=''
for path in manifests:
    row=pin(path);doc=json.loads(path.read_bytes());count=len(doc['entries'])
    insert+=" data=(root/"+repr(row['path'])+").read_bytes();need(sha(data)=="+repr(row['sha256'])+",'C11 immutable manifest changed')\n"
    insert+=" ledger=json.loads(data)['entries'];need(len(ledger)=="+str(count)+",'C11 logical population changed')\n"
    insert+=" for row in ledger:\n  if row['sha256'] is not None:check(row)\n"
insert+=" for row in load(root/(C12+'entry-authority-pins-v1.json'))['authorities']:check(row,transition=False)\n"
marker=" for row in load(root/(C11+'entry-authority-pins-v1.json'))['authorities']:check(row,transition=False)"
assert marker in value;value=value.replace(marker,insert+marker)
value=value.replace('prior_C10_hashed_payloads=7074,','prior_C10_hashed_payloads=7074,prior_C11_complete_logical_payloads=7584,')
target=HERE/'verify_c12_preservation_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('preservation-reader-source-crosswalk-v1.json',dict(before=pin(source),after=pin(target),
    prior_logical_manifests=[pin(p) for p in manifests],scope='Same exact physical source/version resolution and live Git/owner/capture/original snapshot checks, extended by C12 transitions and all three immutable C11 inventories. No inherited evidence or source pin is rewritten.',original_queries=0))
print('C12 preservation reader carries all immutable manifests and current source successors')
