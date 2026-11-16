from common46 import *
lock()
r=HERE/'review_reader.py';s=r.read_text(encoding='utf-8').replace('scope/family-review-v1.json','scope/family-review-v2.json').replace('fixtures/scenario-contract-v1.json','fixtures/scenario-contract-v2.json')
with r.open('w',encoding='utf-8',newline='\n') as f:f.write(s)
source=load(HERE/'final/source-review-v1.json');tracked={r['path']:r for r in load(PRIVATE/'tracked-entry-inventory-v1.json')['files']}
for p in sorted({r['source']['path'] for r in load(HERE/'fixtures/numeric-source-v2.json')['members']}):
 exact(tracked[p]);source['shipping_sources'].append(pin(p));source['entry_source_hashes'][p]=tracked[p]['sha256']
save('final/source-review-v2.json',source)
# Exact accepted neutral replay;the existing scanner sources/guard remain authoritative.
with (HERE/'neutral_privacy_replay.py').open('xb') as f:f.write((ROOT/'evidence/04A45/neutral_privacy_replay.py').read_bytes())
records=[]
import re
for p in sorted((PRIVATE/'commands').glob('*/command.json')):
 d=load(p);args=[]
 for i,a in enumerate(d['command']):
  s=str(a).replace(str(ROOT),'<repo>').replace(ROOT.as_posix(),'<repo>').replace(str(Path(sys.executable)),'<existing-python>')
  if i==0 and re.match(r'^[A-Za-z]:',s):s='<installed-tool>/'+Path(a).name
  if re.search(r'(?i)\b[a-z]:[/\\]',s):s=('/reference:' if s.startswith('/reference:') else '')+'<installed-reference>/'+Path(a).name
  args.append(s.replace('\\','/'))
 records.append(dict(label=p.parent.name,command=args,exit_code=d['exit_code'],seconds=d['seconds'],raw=pin(p),stdout_sha256=d['stdout_sha256'],stderr_sha256=d['stderr_sha256']))
save('matrix/actual-commands-review-v1.json',dict(scope='Actual commands;path-independent aliases only. Exact argv/cwd/output pinned in protected raw. Later publication and independent readback commands separately pinned in external receipt.',commands=records,unsafe_fixed_path_wrappers='NOT_RUN;verify-beshstatng.ps1 and -Legacy overwrite protected historical TRX. Unchanged isolated runner performed full graph/tests instead.',native='NOT_RUN',Node='INHERITED_916_PASS;NOT_RERUN',gates=GATES))
print('Final source supplement,neutral reader and sanitized command ledger prepared',flush=True)
