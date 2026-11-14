"""Read-only public-copy check; explicitly does not claim copied Git/live history."""
import json,os,uuid
from pathlib import Path
from verify_inputs import ROOT,PREFIX,need,load
from verify_closeout import inspect
def main():
 view=ROOT/'.test-rationalization-04A38.tmp'/('m5-public-'+uuid.uuid4().hex[:8]);view.mkdir()
 manifest=load(ROOT/(PREFIX+'publication-manifest-closeout-v1.json'))
 for row in manifest['entries']:
  p=row['path'];target=view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(ROOT/p,target)
 proof=inspect(view);need(proof['status']=='VALID_NOT_READY_UNMATCHED_PROFILE_CLOSEOUT','Copy reader failed')
 print(json.dumps(dict(proof=proof,copy_method='Hardlinked immutable public inventory; read-only verification, no ZIP created',copied_entries=len(manifest['entries']),
  exclusions='Existing private raw environment diagnostic, isolated binaries/raw logs, unrelated4695 historical paths not listed by publication manifest, retained TestResults',
  boundary='Published bytes/case/source snapshot/seal/branch data verified; live Git and all4695 historical paths are separate live-only checks')))
if __name__=='__main__':main()
