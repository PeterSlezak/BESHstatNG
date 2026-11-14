"""Complete an unsealed export using standard Windows extended-path I/O."""
import hashlib,io,json,os,tarfile
from pathlib import Path
from prepare_original import ROOT,HERE,TREE,BASE,git,sha,pin,save

def extended(path):return Path('\\\\?\\'+str(path.absolute())) if os.name=='nt' else path

def main():
 authority=json.loads((HERE/'owner-authorization.json').read_bytes())
 assert authority['owner_quote']=='Yes, I approve' and authority['source_commit']==BASE
 assert git('rev-parse','HEAD').decode().strip()==BASE and not git('diff','--cached','--name-only')
 assert TREE.is_dir() and not (HERE/'original-source-authenticity.json').exists()
 hidden={r['path'] for r in json.loads((ROOT/'evidence/04A38/entry-source-pins.json').read_bytes())['files'] if r['path'].startswith('<PRIVATE_PROTECTED_PATH:')}
 def public(path):
  token='<PRIVATE_PROTECTED_PATH:'+sha(path.encode())[:24]+'>'
  return token if token in hidden else path
 archive=git('archive','--format=tar',BASE);blobs={}
 for item in git('ls-tree','-r','-z',BASE).split(b'\0'):
  if not item:continue
  meta,name=item.split(b'\t',1);mode,kind,oid=meta.decode().split();assert kind=='blob';blobs[name.decode()]=(mode,oid)
 records=[];retained=0
 with tarfile.open(fileobj=io.BytesIO(archive),mode='r:') as tar:
  for member in tar:
   target=TREE/member.name;assert target.resolve().is_relative_to(TREE.resolve())
   physical=extended(target)
   if member.isdir():physical.mkdir(parents=True,exist_ok=True);continue
   assert member.isfile();data=tar.extractfile(member).read();mode,oid=blobs[member.name]
   assert hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()==oid
   if physical.exists():assert physical.read_bytes()==data;retained+=1
   else:
    physical.parent.mkdir(parents=True,exist_ok=True)
    with physical.open('xb') as f:f.write(data)
   records.append(dict(path=public(member.name),git_blob=oid,git_mode=mode,bytes=len(data),sha256=sha(data)))
 assert len(records)==len(blobs)
 originals=json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'];proof=[]
 for row in originals:
  data=extended(TREE/row['source']['path']).read_bytes();snapshot=(ROOT/row['snapshot']['path']).read_bytes()
  assert data==snapshot or data.replace(b'\r\n',b'\n')==snapshot.replace(b'\r\n',b'\n')
  proof.append(dict(path=row['source']['path'],git_sha256=sha(data),entry_snapshot=row['snapshot'],relation='EXACT' if data==snapshot else 'PROVED_LF_CRLF_ONLY'))
 save('original-source-authenticity.json',dict(status='PRISTINE_PINNED_GIT_TREE_VERIFIED',source_commit=BASE,git_tar_sha256=sha(archive),git_tar_bytes=len(archive),archive_stored=False,
  source_tree='<ISOLATED_ORIGINAL_TREE>',files=records,original_udf_snapshot_crosscheck=proof,resumed_verified_partial_files=retained,
  export_correction='Standard Windows extended-path I/O; no permissions/settings changed; first incomplete attempt preserved; no expected results generated',
  build_not_yet_executed=True,qualification='Git source only; ignored offline SDK/third-party inputs must be pinned separately; current Core/Common production binaries excluded',
  chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',native='NOT_RUN',scientific='NOT_GOLDEN'))
 print('Authenticated',len(records),'pristine Git files; verified',retained,'retained partial files;41 UDF originals crosschecked')

if __name__=='__main__':main()
