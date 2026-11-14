"""Preserve pre-W2 source bytes before authorized migration; never copy TestResults."""
import hashlib,json,os,shutil,subprocess,sys
from pathlib import Path
from datetime import datetime,timezone
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
G=ROOT/'evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide'
sys.path.insert(0,str(G))
from common import git,pin,clean,RUNROOT
from verify_supplement import inspect
from verify_packet import inspect as packet

def save(name,obj):
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:json.dump(obj,f,indent=2,ensure_ascii=False);f.write('\n')

def main():
 assert json.loads((G/'APPROVAL_G_GATE_PASS.json').read_bytes())['status']=='APPROVAL_G_COMBINED_GATE_PASS'
 inspect();packet(manifest_name=(G/'publication-manifest-closeout-v1.json').relative_to(ROOT).as_posix(),inventory_name=(G/'file-inventory-closeout-v1.json').relative_to(ROOT).as_posix())
 assert git('branch','--show-current').decode().strip()=='codex/04A-38' and not git('diff','--cached','--name-only')
 checked=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert checked.returncode==0
 manifest=json.loads((G/'publication-manifest-closeout-v1.json').read_bytes())
 view=RUNROOT/'w2-pre-change-view';view.mkdir(exist_ok=False)
 rows=[]
 # Copies, not hardlinks: a later source edit must not alter the historical view.
 paths={r['path'] for r in manifest['entries']}
 paths.update(git('ls-files').decode().splitlines())
 for project in ['BESHStat.Core','BESHStat.Excel.Common','BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHstatNG','BESHStatNG.Test']:
  for directory,dirs,files in os.walk(ROOT/project):
   dirs[:]=[d for d in dirs if d.casefold() not in {'bin','obj','testresults','.git'}]
   for name in files:
    p=Path(directory)/name
    if p.suffix.lower() in {'.vb','.cs','.vbproj','.csproj','.props','.targets','.config'}:paths.add(p.relative_to(ROOT).as_posix())
 for name in sorted(paths):
  source=ROOT/name
  if not source.is_file():continue
  target=view/name;target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(source,target)
  row=pin(source);assert target.read_bytes()==source.read_bytes();rows.append(row)
 originals=[]
 for row in rows:
  p=Path(row['path'])
  if p.suffix.lower() in {'.vb','.cs','.vbproj','.csproj','.props','.targets'} and p.parts[0] in {'BESHStat.Core','BESHStat.Excel.Common','BESHstatNG','BESHStatNG.Test','BESHStat.Core.Tests','BESHStat.Excel.Common.Tests'}:
   out=HERE/'source-before'/p;out.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(ROOT/p,out)
   originals.append(dict(live=row,before=pin(out)))
 save('source-before-inventory.json',dict(status='PRE_W2_SOURCE_BYTES_PRESERVED',recorded_utc=datetime.now(timezone.utc).isoformat(),sources=originals,private_historical_view_files=rows,TestResults='NOT_TRAVERSED',copy_policy='Independent byte copies, no mutable production hardlinks'))
 save('entry.json',dict(status='W2_STARTED_AFTER_APPROVAL_G_PASS',G_gate=pin(G/'APPROVAL_G_GATE_PASS.json'),branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=checked.returncode,source_before=pin(HERE/'source-before-inventory.json'),new_original_case_authority='EXHAUSTED',W3='NOT_STARTED',batch='NOT_READY_04A38'))
 print('W2 entry sealed; historical view',len(rows),'files; production/source copies',len(originals))
if __name__=='__main__':main()
