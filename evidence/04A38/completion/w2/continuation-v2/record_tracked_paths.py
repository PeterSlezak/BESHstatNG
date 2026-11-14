from common import *
old=json.loads((PREVIOUS/'tracked-path-preservation.json').read_bytes());tracked=[p for p in git('ls-files','-z').decode().split('\0') if p];hidden={'<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>':p for p in tracked};assert len(tracked)==4695
allowed={r['path']:r for r in json.loads((HERE/'source-transitions-final.json').read_bytes())['files']};original=json.loads((ROOT/'evidence/04A38/entry-source-pins.json').read_bytes())['files'];entry={r['path']:r for r in original};records=[]
for r in old['records']:
 p=r['path'];live=hidden.get(p,p);current=pin(ROOT/live);current['path']=p;oldpin=r['current'];changed=current!=oldpin
 if changed:
  assert live in allowed and current['sha256']==allowed[live]['after']['sha256'] and oldpin['sha256']==allowed[live]['before']['sha256']
 exact=current==entry[p]
 records.append(dict(path=p,prior_checkpoint=oldpin,current=current,continuation='AUTHORIZED_V2_SOURCE_TRANSITION' if changed else 'EXACT_C1_CHECKPOINT',original_entry_exact=exact))
save('tracked-path-preservation.json',dict(scope='LIVE4695_TRACKED_PATHS_ONLY;NO_IGNORED_HISTORY_TRAVERSAL',total=len(records),original_entry_exact=sum(r['original_entry_exact'] for r in records),authorized_transitions=sum(not r['original_entry_exact'] for r in records),continuation_further_transitions=sum(r['continuation']=='AUTHORIZED_V2_SOURCE_TRANSITION' for r in records),records=records))
