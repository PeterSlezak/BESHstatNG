"""Independently inspect the complete manifest in an isolated payload view."""
import json,os,uuid
from verify_publication import ROOT,W,SELF,load,inspect
def main():
 view=ROOT/'.test-rationalization-04A38.tmp'/('w2-public-'+uuid.uuid4().hex[:8]);view.mkdir()
 rows=load(ROOT/SELF)['entries']
 for row in rows:
  target=view/row['path'];target.parent.mkdir(parents=True,exist_ok=True);os.link(ROOT/row['path'],target)
 proof=inspect(view);print(json.dumps(dict(status='W2_PUBLIC_COPY_PASS_NOT_READY',entries=len(rows),proof=proof,scope='Copied source/results/recorded IL metadata only, not live Git or omitted loaded binaries/owner files')))
if __name__=='__main__':main()
