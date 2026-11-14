"""Publish independent copy/adverse receipts under a separate outer manifest."""
import json,sys
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3];V6=ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'
sys.path.insert(0,str(V6))
from common import command,clean,RUNROOT,pin
from baseline_report import python_receipt
def save(n,v):
 with (HERE/n).open('x',encoding='utf-8',newline='\n') as f:json.dump(v,f,ensure_ascii=False,indent=2);f.write('\n')
def main():
 jobs=[('w1-public-copy-reader',[str(HERE/'check_public_copy.py')]),('w1-publication-adverse',['-m','unittest','discover','-s',str(HERE),'-p','test_publication.py','-v'])]
 def run(job):label,args=job;return dict(label=label,receipt=clean(command(label,[sys.executable,'-B',*args])))
 with ThreadPoolExecutor(max_workers=2) as pool:results=list(pool.map(run,jobs))
 assert all(r['receipt']['exit_code']==0 for r in results)
 tests=python_receipt('mmrm38-v6-w1-publication-adverse');assert tests['total']==12 and tests['exit_code']==0
 save('publication-validation.json',dict(status='INDEPENDENT_PUBLIC_COPY_AND_ADVERSE_PASS',commands=results,adverse=tests,new_tests=dict(F=70,W1=18,publication=12,total=100),unchanged_previous_tests=457,total_validated_focused_passes=557,
  earlier_failed_fixture='17nominalPASS/1ERROR retained and not accepted; corrected18PASS proof and exact raw identities in validation.json',public_copy=json.loads((RUNROOT/'commands/mmrm38-v6-w1-public-copy-reader/stdout.log').read_bytes()),
  scope='Bounded F+census+honest stop publication, not W3 READY acceptance',verified_predecessor_manifest=pin(HERE/'publication-manifest-v1.json')))
 save('review-copy-plan.json',dict(filename='BESHstatNG_04A38_W1_ApprovalF_20261109_review.zip',kind='DERIVATIVE_REVIEW_ZIP',payload_inventory='file-inventory-v2.json plus REVIEW_COPY_BOUNDARY.json in ZIP',
  inclusions='Every intended current W1/F and predecessor payload, all41 current UDF sources',private_exclusions='Existing private raw diagnostics, isolated binaries/raw logs/cache, unrelated historical paths outside publication inventory, Git metadata and retained TestResults',
  boundary='ZIP does not prove live Git/all4695 unarchived paths or ignored history; original dependency rebuild requires full pinned repo/offline cache',receipt='Detached under ignored exports; real SHA/size/entry count/CRC and every payload-byte verification; not included in the ZIP or source manifest to avoid self-hashing recursion'))
 paths={r['path'] for r in json.loads((HERE/'publication-manifest-v1.json').read_bytes())['entries']}
 for folder in [HERE,V6]:paths.update(p.relative_to(ROOT).as_posix() for p in folder.rglob('*') if p.is_file())
 manifest=(HERE/'publication-manifest-v2.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory-v2.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory])
 save('file-inventory-v2.json',dict(paths=sorted(paths),scope='Complete intended current W1/F plus unchanged protected predecessor publication payloads',private_exclusions='Existing raw private diagnostics and isolated binaries/raw logs/cache; unrelated paths and TestResults not newly traversed',review_zip='Derivative with explicit boundary; detached receipt avoids recursion'))
 save('publication-manifest-v2.json',dict(status='NOT_READY_04A38',reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',scope='FINAL_W1_F_GATE_CENSUS_CONSOLIDATED_STOP_WITH_INDEPENDENT_AUDIT_RECEIPTS',predecessor=pin(HERE/'publication-manifest-v1.json'),entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]))
 from verify_publication import inspect
 print(json.dumps(inspect(version='v2')))
if __name__=='__main__':main()
