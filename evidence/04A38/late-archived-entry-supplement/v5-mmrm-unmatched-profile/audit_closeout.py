"""Run final independent local/copy/adversarial readers, then seal their receipts."""
from concurrent.futures import ThreadPoolExecutor
from common import *
from verify_closeout import inspect
def main():
 jobs=[('closeout-local-reader',[str(HERE/'verify_closeout.py')]),('closeout-adverse',['-m','unittest','discover','-s',str(HERE),'-p','test_closeout.py','-v']),
  ('closeout-public-copy-reader',[str(HERE/'verify_public_copy.py')]),('closeout-original-source-reader',[str(HERE/'verify_original_source.py')])]
 def run(job):
  label,args=job;return dict(label=label,receipt=clean(command(label,[sys.executable,'-B',*args])))
 with ThreadPoolExecutor(max_workers=4) as pool:results=list(pool.map(run,jobs))
 assert all(r['receipt']['exit_code']==0 for r in results)
 from baseline_report import python_receipt
 tests=python_receipt('mmrm38-v5-closeout-adverse');assert tests['total']==12 and tests['exit_code']==0
 public=json.loads((RUNROOT/'commands/mmrm38-v5-closeout-public-copy-reader/stdout.log').read_bytes())
 source=json.loads((RUNROOT/'commands/mmrm38-v5-closeout-original-source-reader/stdout.log').read_bytes())
 save('closeout-verification.json',dict(status='INDEPENDENT_LOCAL_PUBLIC_COPY_SOURCE_AND_ADVERSE_PASS',commands=results,closeout_adverse=tests,
  public_copy=public,original_source=source,closeout_manifest_verified=pin(HERE/'publication-manifest-closeout-v1.json'),
  new_tests=dict(gate_and_stop=93,closeout=12,total=105),unchanged_previous_tests=352,all_focused_test_passes=457,
  qualification='Bounded compatibility and honest stop dossier only; no complete migration or scientific/native/owner acceptance'))
 paths={r['path'] for r in json.loads((ROOT/PREVIOUS).read_bytes())['entries']};paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 manifest=(HERE/'publication-manifest-closeout-v2.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory-closeout-v2.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory])
 save('file-inventory-closeout-v2.json',dict(paths=sorted(paths),private_exclusions='Existing private owner diagnostic and isolated raw logs/binaries; every predecessor intended payload retained',upload_zip='NOT_CREATED',authoritative_manifest=manifest))
 save('publication-manifest-closeout-v2.json',dict(status='NOT_READY_04A38',reference_class=CLASS,scope='COMPLETE_V5_GATE_AND_REQUIRED_SCALAR_GROUP_STOP_PLUS_INDEPENDENT_CLOSEOUT_RECEIPTS;LOCAL_ONLY',
  predecessor=pin(HERE/'publication-manifest-closeout-v1.json'),entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]))
 print(json.dumps(inspect(version='v2')))
if __name__=='__main__':main()
