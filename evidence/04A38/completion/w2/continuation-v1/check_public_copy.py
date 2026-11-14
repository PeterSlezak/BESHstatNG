import json,shutil,uuid
from pathlib import Path
from verify_publication import ROOT,HERE,SELF,load,inspect
def main():
 import sys
 manifest=sys.argv[1] if len(sys.argv)>1 else SELF
 view=Path('\\\\?\\'+str(ROOT/'.test-rationalization-04A38.tmp'/('w2c1-export-copy-'+uuid.uuid4().hex[:8])));view.mkdir()
 for row in load(ROOT/manifest)['entries']:
  dest=view/row['path'];dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(ROOT/row['path'],dest)
 proof=inspect(view,manifest);print(json.dumps(dict(status='ISOLATED_PUBLIC_COPY_READER_PASS_NOT_BATCH_READY',entries=len(load(ROOT/manifest)['entries']),proof=proof,limitation='Exact included payloads copied and independently read; no live Git/private omitted binary/owner/ignored-tree claim')))
if __name__=='__main__':main()
