from common import *
source=PREVIOUS/'verify_c12_preservation_v1.py';value=source.read_text(encoding='utf-8')
value=value.replace("C12=W+'continuation-v12/'","C12=W+'continuation-v12/';C13=W+'continuation-v13/'")
old="transitions=load(root/(C12+'source-transitions-final-v1.json'))['files']";assert old in value
value=value.replace(old,"c12trans=load(root/(C12+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C13+'source-transitions-final-v1.json'))['files']").replace('c11trans+c10trans','c12trans+c11trans+c10trans')
manifest=PREVIOUS/'publication-manifest-v1.json';row=pin(manifest);count=len(json.loads(manifest.read_bytes())['entries']);assert count==8015
marker=" for row in load(root/(C12+'entry-authority-pins-v1.json'))['authorities']:check(row,transition=False)";assert marker in value
insert=" data=(root/"+repr(row['path'])+").read_bytes();need(sha(data)=="+repr(row['sha256'])+",'C12 immutable complete manifest changed')\n ledger=json.loads(data)['entries'];need(len(ledger)==8015,'C12 logical population changed')\n for row in ledger:\n  if row['sha256'] is not None:check(row)\n"
insert+=""" private_manifest=load(root/(W+'continuation-v12-retained-private-preservation-v1.json'))
 private_rows={r['path']:r for r in private_manifest['private_pins']}
 for row in load(root/(C13+'entry-authority-pins-v1.json'))['authorities']:
  if row['path'].startswith('.test-rationalization-04A38.tmp/') and not live:
   need(row==private_rows.get(row['path']),'Unattested omitted raw private authority')
  else:check(row,transition=False)
"""
value=value.replace(marker,insert+marker).replace('prior_C11_complete_logical_payloads=7584,','prior_C11_complete_logical_payloads=7584,prior_C12_complete_logical_payloads=8015,')
target=HERE/'verify_c13_preservation_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('preservation-reader-source-crosswalk-v1.json',dict(before=pin(source),after=pin(target),prior_complete_manifest=pin(manifest),
    scope='Same immutable manifests/versions and original/owner/lineage/capture/Git checks, extended by C13 transitions and complete C12 inventory. Copied reviews explicitly omit already8801-pinned raw private entry command files; live verification checks them. No omission from live preservation.',original_queries=0))
source=W2/'continuation-v12-retained-private-reader-v1.py';value=source.read_text(encoding='utf-8')
value=value.replace("parent/'continuation-v12'","parent/'continuation-v13'").replace("TARGET=W2/'continuation-v12-retained-private-preservation-v1.json'","TARGET=W2/'continuation-v13-retained-private-preservation-v1.json'")
value=value.replace("W2/'continuation-v11-retained-private-preservation-v4.json'","W2/'continuation-v12-retained-private-preservation-v1.json'").replace("W2/'continuation-v11-retained-private-reader-v4.py'","W2/'continuation-v12-retained-private-reader-v1.py'")
value=value.replace('==5661','==8801').replace('inherited_private_count=5661','inherited_private_count=8801').replace("preservation-prior-v11-v1","preservation-prior-v12-v1")
value=value.replace('continuation-v12-review-parts-index-v1.json','continuation-v13-review-parts-index-v1.json').replace('C12_','C13_').replace('inherited5661','inherited8801').replace('All5661','All8801').replace('bounded C12','bounded C13').replace('fresh isolated C11','fresh isolated C12')
target=W2/'continuation-v13-retained-private-reader-v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('private-preservation-reader-source-crosswalk-v1.json',dict(before=pin(source),after=pin(target),inherited_private_pins=8801,inherited_physical_archives=45,
    scope='Actual v12 authoritative reader executed in its isolated documented environment; every inherited pin/archive/giant retained plus bounded current files. No traversal of historical TestResults.',original_queries=0))
print('Versioned live/copy and explicit inherited8801 private readers prepared')
