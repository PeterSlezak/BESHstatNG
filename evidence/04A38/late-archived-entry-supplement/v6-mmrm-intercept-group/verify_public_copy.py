"""Independent bounded F public-copy reader; no claim of copied live Git."""
import json,os,uuid
from verify_inputs import ROOT,PREFIX,load
from verify_packet import inspect as packet
from verify_supplement import inspect
def main():
 view=ROOT/'.test-rationalization-04A38.tmp'/('f6-public-'+uuid.uuid4().hex[:8]);view.mkdir()
 rows=load(ROOT/(PREFIX+'publication-manifest.json'))['entries']
 for row in rows:
  target=view/row['path'];target.parent.mkdir(parents=True,exist_ok=True);os.link(ROOT/row['path'],target)
 proof=inspect(view);publication=packet(view)
 print(json.dumps(dict(status='PUBLIC_COPY_F_GATE_VERIFIED',exact={k:v['exact'] for k,v in proof['configurations'].items()},publication=publication,copied_entries=len(rows),boundary='Published bytes only; live Git/4695 paths and ignored history are separate live checks')))
if __name__=='__main__':main()
