"""Exact manifest-only derivative ZIP; inspect physical inventory before copying."""
import shutil,zipfile,uuid
from common import *
from verify_publication import inspect_zip
manifest=sys.argv[1];name='BESHstatNG_04A38_W2_continuation_v4_20261110_review.zip';folder=RUNROOT/'exports';folder.mkdir(exist_ok=True);dest=folder/name;assert not dest.exists()
rows=json.loads((ROOT/manifest).read_bytes())['entries']
with zipfile.ZipFile(dest,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
 for row in rows:z.write(ROOT/row['path'],row['path'])
# Reader examines all physical members, not merely the producer manifest list.
r=command('export-physical-zip',[sys.executable,'-B',str(HERE/'verify_publication.py'),manifest,str(dest)]);assert r['exit_code']==0
physical=json.loads((RUNROOT/'commands/w2c4-export-physical-zip/stdout.log').read_bytes())
view=RUNROOT/'c4-public-review-copy';view.mkdir();copy=folder/'c4-independent-copy.zip';assert not copy.exists();shutil.copyfile(dest,copy);digest=sha(dest.read_bytes());assert sha(copy.read_bytes())==digest
# safe_path and complete member/hash/CRC checks have already succeeded.
with zipfile.ZipFile(copy) as z:
 for i in z.infolist():
  target=view/i.filename;assert target.resolve().is_relative_to(view.resolve());target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as f:f.write(z.read(i.filename))
 assert z.testzip() is None
# Run the included independent checker itself; no live producer is imported.
copied=command('export-copied-executable',[sys.executable,'-B',str(view/HERE.relative_to(ROOT)/'verify_publication.py'),manifest],cwd=view);assert copied['exit_code']==0
result=json.loads((RUNROOT/'commands/w2c4-export-copied-executable/stdout.log').read_bytes())
receipt=dict(status='PARTIAL_W2_NOT_READY_REVIEW_EXPORT_VERIFIED',filename=name,bytes=dest.stat().st_size,sha256=digest,entries=len(rows),CRC='PASS_ALL_ENTRIES',actual_physical_inventory_privacy=physical,physical_reader_command=clean(r),manifest=pin(ROOT/manifest),separate_zip_copy_sha256=sha(copy.read_bytes()),copied_executable=clean(copied),copied_reader_result=result,exclusions='See included file-inventory manifest; raw diagnostics/caches physically excluded. Live Git/index/omitted owner docs/private binaries/ignored history cannot be verified from this derivative.',no_owner_acceptance=True)
p=folder/'BESHstatNG_04A38_W2_continuation_v4_20261110_export-receipt.json'
with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(receipt,f,indent=2,ensure_ascii=False);f.write('\n')
print(json.dumps({k:receipt[k] for k in ['status','filename','bytes','sha256','entries','CRC']}))
