"""Freeze live merged authority before production edits. Offline, project-relative receipts."""
from pathlib import Path
import csv,io,hashlib,json,subprocess,os,re
ROOT=Path(__file__).resolve().parents[2]; HERE=Path(__file__).resolve().parent
BASE='43d20f86b80934c49b2f3dec90824aa62af5aaee'
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode('utf-8-sig').strip()
def sha(b):return hashlib.sha256(b).hexdigest()
def pin(p):
 b=(ROOT/p).read_bytes();return dict(path=p,bytes=len(b),sha256=sha(b))
def save(n,v): (HERE/n).write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
def public(p):return '<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>' if re.search(r'DESKTOP-|LAPTOP-|S-1-5-|[A-Za-z]:',p,re.I) or os.environ.get('COMPUTERNAME','!unset!').lower() in p.lower() else p
if __name__=='__main__':
 assert git('rev-parse','HEAD','main').splitlines()==[BASE,BASE]
 assert git('branch','--show-current')=='codex/04A-33'
 assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
 assert not (HERE/'entry-state.json').exists()
 records=[]
 for p in git('ls-files').splitlines():
  if (ROOT/p).is_file():r=pin(p);r['path']=public(p);records.append(r)
 save('entry-pins.json',dict(schema='04A33-entry-pins-v1',files=records,scope='Tracked files only; historical ignored payload not rehashed'))
 qualified=[]
 for r in json.loads((ROOT/'evidence/04A32/publication-manifest.json').read_bytes())['files']:
  if r['sha256'] is None:continue
  b=(ROOT/r['path']).read_bytes(); cr=b.replace(b'\n',b'\r\n')
  variants={'DIRECT':b,'CRLF_RECONSTITUTION':cr,'CRLF_EXCEPT_FINAL_LITERAL_LF':cr[:-2]+b'\n' if cr.endswith(b'\r\n') else cr}
  if r['path'].endswith('.csv'):
   s=io.StringIO(newline='');csv.writer(s).writerows(csv.reader(io.StringIO(b.decode(),newline='')));variants['CSV_RECORD_CRLF_EMBEDDED_LF']=s.getvalue().encode()
  matches=[k for k,v in variants.items() if len(v)==r['bytes'] and sha(v)==r['sha256']]
  assert matches,r['path']
  assert b==subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+r['path']],cwd=ROOT)
  qualified.append(dict(path=r['path'],relation=matches[0],frozen_sha256=r['sha256'],merged=pin(r['path'])))
 save('entry-state.json',dict(schema='04A33-entry-v1',branch='codex/04A-33',head=BASE,main=BASE,merge_base=BASE,parent=git('rev-parse',BASE+'^'),index='EMPTY',owner_changes=[],entry_observation='Owner merge occurred during initial read-only inspection; rechecked stable clean merged main before branch creation. No owner changes overwritten.',previous_publication_qualification=qualified,qualification_limit='Digest-exact inverse EOL reconstruction plus committed blob equality; not a new original-archive-byte inspection. Accepted04A29 original-byte qualification remains frozen.',discrepancies=['Plan revision2.11 progress counters stale; owner acceptance through32 and live main govern','XYZ sentinel is -100000000000000.0; scatter sentinel is -100. Prompt historical -100 is not XYZ source authority.'],gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',releases=0,discharges=0)))
 members=list(csv.DictReader((ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig')))
 paths=['BESHstatNG/src/Graphics/'+p+'.vb' for p in ['XYZscatter','ScatterPlotMatrix','3Dobjects']]
 rows=[]
 for m in members:
  if m['path'] in paths:
   rows.append(dict(m,entry_sha256=pin(m['path'])['sha256'],range_authority='Historical04A29 range; current member source pinned separately',destination='Core numeric geometry; Windows chart/style/lifecycle and mutable option orchestration',prior_map='evidence/04A30/migration-operation-map-v1.csv',restrictions='NOT_GOLDEN;NATIVE_NOT_RUN;zero discharges'))
 with (HERE/'operation-map-entry.csv').open('w',encoding='utf-8',newline='') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
 run=ROOT/'.test-rationalization-04A33.tmp';run.mkdir(exist_ok=False)
 print('Frozen',len(records),'tracked files;',len(rows),'historical member links;',len(qualified),'predecessor files qualified')
