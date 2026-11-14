"""Verify live entry paths omitted from archive proof, without TestResults traversal."""
import json
from collections import Counter
from check_checkpoint import ROOT,HERE,BASE,pin,digest,git,save

def main():
 entry=json.loads((ROOT/'evidence/04A38/entry-source-pins.json').read_bytes())['files']
 checkpoint=json.loads((HERE/'entry.json').read_bytes())
 transitions=json.loads((ROOT/'evidence/04A38/source-transitions-validation-v2.json').read_bytes())['transitions']
 changed={r['path']:r for r in transitions}
 tracked=[p for p in git('ls-files','-z').stdout.decode().split('\0') if p]
 private={'<PRIVATE_PROTECTED_PATH:'+digest(p.encode())[:24]+'>':p for p in tracked}
 counts=Counter();records=[]
 for row in entry:
  path=row['path'];real=private[path] if path.startswith('<PRIVATE_PROTECTED_PATH:') else path
  actual=pin(real);actual['path']=path
  expected=changed[path]['current'] if path in changed else row
  assert actual==expected,('Unexpected live change or missing path',path)
  category='PRIVACY_REDACTED' if path.startswith('<PRIVATE_PROTECTED_PATH:') else 'GITHUB' if path.startswith('.github/') else 'TEST_DEPENDENCY' if path.startswith('BESHStatNG.Test/packages/') else 'OTHER'
  counts[category]+=1
  records.append(dict(path=path,status='EXACT_CHECKPOINT_TRANSITION' if path in changed else 'EXACT_ENTRY',category=category,current=actual))
 assert len(records)==4695 and len(changed)==32
 for row in checkpoint['checkpoint_payloads']:assert pin(row['path'])==row,('Checkpoint payload mutated',row['path'])
 previous=json.loads((ROOT/'evidence/04A38/preservation-validation-v2.json').read_bytes())
 for row in previous['lineage_pins']:assert pin(row['path'])==row['current']
 for row in previous['protected_mixed_model_captures']+previous['owner_documents_excluded']:assert pin(row['path'])==row
 assert len(previous['lineage_pins'])==278 and len(previous['protected_mixed_model_captures'])==100 and len(previous['owner_documents_excluded'])==16
 check=git('diff','--check');assert check.returncode==0
 assert not git('diff','--cached','--name-only').stdout
 save('live-preservation.json',dict(status='LIVE_REPOSITORY_PRESERVATION_PASS',base=BASE,tracked_entry_files=4695,exact_entry=4663,authorized_checkpoint_transitions=32,
  categories=dict(counts),records=records,checkpoint_payloads_exact=611,lineage_pins=278,protected_captures=100,owner_documents=16,
  archive_qualification='Owner reports389 entry paths absent from the independently inspected ZIP. This receipt checks all4695 actual live paths, including privacy-redacted paths, test dependencies and .github files; it does not claim those bytes were in the ZIP.',
  index='EMPTY',diff_check=dict(exit_code=0,stdout=check.stdout.decode(),stderr=check.stderr.decode()),continuation_production_changes=0,
  historical_test_results='NOT_TRAVERSED_OR_REHASHED',native='NOT_RUN',scientific='NOT_GOLDEN'))
 print('Live4695 paths verified:4663 entry-exact +32 checkpoint transitions;',dict(counts),';278 pins/100 captures/16 owner docs preserved')

if __name__=='__main__':main()
