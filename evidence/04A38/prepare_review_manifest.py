"""Inventory/hash a local NOT_READY review packet. No staging or publication.

Includes ignored batch captures explicitly; never changes Git ignore rules.
The independent reader is a separate implementation and imports no producer.
"""
import json,subprocess
from preflight import ROOT,HERE,BASE,save,pin,sha

def git(*args):
 result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True,check=False)
 return result

def main():
 manifest=HERE/'publication-manifest.json';require=lambda value:value or (_ for _ in ()).throw(AssertionError('Review packet precondition'))
 require(not manifest.exists())
 require(git('branch','--show-current').stdout.decode().strip()=='codex/04A-38')
 require(git('rev-parse','HEAD').stdout.decode().strip()==git('rev-parse','main').stdout.decode().strip()==BASE)
 require(not git('diff','--cached','--name-only').stdout)
 check=git('diff','--check');require(check.returncode==0)
 diff=git('diff','--name-only','-z').stdout.decode().split('\0');new=git('ls-files','--others','--exclude-standard','-z').stdout.decode().split('\0')
 save('git-final.json',dict(branch='codex/04A-38',head=BASE,main=BASE,merge_base=git('merge-base','HEAD','main').stdout.decode().strip(),index='EMPTY',
  diff_check=dict(exit_code=check.returncode,stdout=check.stdout.decode(),stderr=check.stderr.decode()),porcelain=git('status','--porcelain=v1').stdout.decode(),
  diff_stat=git('diff','--stat').stdout.decode(),tracked_changes=[p for p in diff if p],unrelated_changes=[],committed=False,staged=False,pushed=False,published=False,next_batch='04A39_NOT_AUTHORIZED'))
 ignored=[]
 for p in HERE.rglob('*'):
  if not p.is_file():continue
  relative=p.relative_to(ROOT).as_posix()
  if git('check-ignore','-q',relative).returncode==0:ignored.append(pin(relative))
 save('git-ignored-review-payloads.json',dict(qualification='Explicitly included in the review manifest despite generic Debug/Release directory ignore rules. No index or ignore rule changed; future export must honor this explicit inventory.',files=ignored))
 # The test receipts are actual executed results, not predictions from a build.
 save('review-audit-results.json',dict(status='BOUNDED_NOT_READY_CONTROLS_ONLY',original_reference_adverse_cases=28,original_reference_adverse_passed=28,
  review_reader_adverse_cases=20,review_reader_adverse_passed=20,acceptance_ownership_audit='INCOMPLETE; no READY allowed',source_compatibility='EXACT28966/configuration',
  native='NOT_RUN',scientific='NOT_GOLDEN'))
 paths=set(p for p in diff+new if p and (ROOT/p).is_file())
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 paths.update({'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb','BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb'})
 paths.update({'evidence/04A38/file-inventory.json','evidence/04A38/publication-manifest.json'})
 require(not any(p.startswith('planning/') or p.endswith('/README.md') and not p.startswith('evidence/04A38/') for p in paths))
 # Exact inventory includes itself and the one nonrecursive manifest entry.
 save('file-inventory.json',dict(status='LOCAL_NOT_READY_REVIEW_ARTIFACTS',paths=sorted(paths),external_payloads=len(paths)-1,
  ignored_payloads=len(ignored),owner_documents_excluded=16,scope='Explicit proposed artifacts only; includes structured synthetic captures, excludes private raw run logs/binaries and historical TestResults.'))
 entries=[dict(path=p,bytes=None,sha256=None) if p=='evidence/04A38/publication-manifest.json' else pin(p) for p in sorted(paths)]
 save('publication-manifest.json',dict(status='NOT_READY_04A38',scope='PROPOSED_LOCAL_REVIEW_PACKET_ONLY; NOT_PUBLISHED; NOT_ACCEPTANCE_READY',source_commit=BASE,entries=entries))
 print('Prepared local NOT_READY manifest:',len(entries)-1,'external payloads +1 null self-entry;',len(ignored),'explicit ignored payloads')

if __name__=='__main__':main()
