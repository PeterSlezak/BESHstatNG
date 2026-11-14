"""Relocate only this run's unrelated copied preimages, preserving exact bytes.

No prior A-G/W1 evidence, tracked file, source/test file, protected capture or
TestResults content is moved. Empty directories are deliberately retained.
"""
from capture_current import *
from common import git

def main():
 keep={r['before']['path'] for r in json.loads((HERE/'source-transitions.json').read_bytes())['files'] if r['before']}
 inventory=json.loads((HERE/'source-before-inventory.json').read_bytes());tracked=set(git('ls-files').decode().splitlines())
 base=HERE/'source-before';private=RUNROOT/'w2-unrelated-source-preimages';private.mkdir(exist_ok=False);rows=[]
 for record in inventory['sources']:
  row=record['before'];source=ROOT/row['path']
  assert source.resolve().is_relative_to(base.resolve()) and row['path'] not in tracked
  assert pin(source)==row
  if row['path'] in keep:continue
  target=private/source.relative_to(base);assert target.resolve().is_relative_to(private.resolve()) and target.resolve().is_relative_to(ROOT.resolve()) and not target.exists()
  target.parent.mkdir(parents=True,exist_ok=True)
  source.replace(target)
  assert library_pin(target)['sha256']==row['sha256'] and target.stat().st_size==row['bytes']
  rows.append(dict(original_record=row,retained_private=target.relative_to(RUNROOT).as_posix(),bytes_unchanged=True))
 with (HERE/'preimage-location-transition.json').open('x',encoding='utf-8') as f:json.dump(dict(status='BYTE_PRESERVED_PRIVATE_PREIMAGE_RELOCATION',reason='Broad new W2 copy included legacy-encoded unrelated source, causing two inherited privacy tests to ERROR instead of FAIL. Raw attempt remains; no decoding/redaction/expectation change.',moved=len(rows),retained_public=sorted(keep),records=rows,source_before_raw_inventory='Immutable chronology snapshot; moved paths resolve through this explicit alias ledger',old_evidence_changes=0,production_changes=0,TestResults='NOT_TOUCHED'),f,indent=2)
 runner=m.Runner(RUNROOT);tests=[]
 for tag,file,method in [('13','test_04a13_survival_workflow.py','test_complete_existing_publication_set_has_no_unresolved_private_values'),('14','test_04a14_mixed_workflow.py','test_entire_proposed_publication_set_privacy')]:
  r=runner.command('w2-privacy-artifact-correction-'+tag,[sys.executable,'-B','-m','unittest','discover','-s','scripts/tests','-p',file,'-k',method,'-v']);tests.append(clean(r))
 with (HERE/'privacy-targeted-reruns.json').open('x',encoding='utf-8') as f:json.dump(dict(commands=tests,expectation='Each inherited privacy identity remains FAIL; must not ERROR from new artifact encoding. No test assertions changed.'),f,indent=2)
 print('Preserved unrelated preimages privately:',len(rows))
if __name__=='__main__':main()
