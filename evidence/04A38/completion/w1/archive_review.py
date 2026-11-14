"""Non-destructive derivative review ZIP; source/evidence manifests unchanged."""
import hashlib,json,sys,zipfile
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3];W='evidence/04A38/completion/w1/'
sys.path.insert(0,str(ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'))
from common import RUNROOT
from verify_publication import inspect
def main():
 proof=inspect(version='v2');manifest=ROOT/(W+'publication-manifest-v2.json');raw=manifest.read_bytes();rows=json.loads(raw)['entries']
 exports=RUNROOT/'exports';exports.mkdir(exist_ok=True);archive=exports/'BESHstatNG_04A38_W1_ApprovalF_20261109_review.zip';assert not archive.exists()
 boundary=dict(kind='COMPACT_DERIVATIVE_REVIEW_COPY_NOT_FULL_REPOSITORY',authoritative_manifest=W+'publication-manifest-v2.json',authoritative_manifest_sha256=hashlib.sha256(raw).hexdigest(),included_payloads=len(rows),
  includes='All current W1/F artifacts, all41 current UDF sources, full intended predecessor publication payloads, frozen cases/source snapshots/traces/readers and attempts',
  excludes=['Raw private environment diagnostic already excluded by prior publication','Private isolated compiled binaries/package cache/raw logs','Unrelated tracked files outside authoritative publication inventory, including private/excluded owner paths and older unrelated help/evidence','Retained ignored TestResults history','Git index/history/worktree metadata'],
  limitation='Cannot prove live Git, all4695 live paths, private binaries/package cache, unarchived files or native/scientific behavior. Rebuilding original dependencies requires the pinned full repository and existing offline toolchain/cache.',
  receipt_policy='ZIP hash/CRC/size receipt is detached under exports; excluding it prevents a ZIP hashing itself. No source/statistical input/output/trace bytes redacted.')
 with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
  for r in rows:z.write(ROOT/r['path'],r['path'])
  z.writestr('REVIEW_COPY_BOUNDARY.json',json.dumps(boundary,indent=2)+'\n')
 with zipfile.ZipFile(archive) as z:
  names=z.namelist();assert len(names)==len(set(n.casefold() for n in names))==len(rows)+1
  assert z.testzip() is None
  for r in rows:
   b=z.read(r['path']);expected=raw if r['path']==W+'publication-manifest-v2.json' else (ROOT/r['path']).read_bytes()
   assert b==expected
   if r['sha256'] is not None:assert len(b)==r['bytes'] and hashlib.sha256(b).hexdigest()==r['sha256']
 h=hashlib.sha256()
 with archive.open('rb') as stream:
  for chunk in iter(lambda:stream.read(4*1024*1024),b''):h.update(chunk)
 receipt=dict(status='ZIP_CRC_AND_EVERY_PAYLOAD_BYTE_VERIFIED',filename=archive.name,relative_path=archive.relative_to(ROOT).as_posix(),bytes=archive.stat().st_size,sha256=h.hexdigest(),entries=len(names),CRC='PASS_ALL_ENTRIES',manifest_sha256=hashlib.sha256(raw).hexdigest(),boundary=boundary,publication=proof)
 with archive.with_suffix('.zip.receipt.json').open('x',encoding='utf-8') as f:json.dump(receipt,f,indent=2)
 print(json.dumps(receipt))
if __name__=='__main__':main()
