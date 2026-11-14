"""Fresh live Git/archive/snapshot/preservation proof before MMRM capture."""
import io,json,re,tarfile,subprocess,sys
from datetime import datetime,timezone
from common import *

def main():
 state=dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),
            merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY' if not git('diff','--cached','--name-only') else 'NONEMPTY',
            status=git('status','--short').decode(),worktrees=clean(git('worktree','list').decode()))
 assert state['branch']=='codex/04A-38' and state['head']==state['main']==state['merge_base']==BASE and state['index']=='EMPTY'
 check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert check.returncode==0
 manifest=json.loads((ROOT/PREVIOUS).read_bytes());assert len(manifest['entries'])==891
 for row in manifest['entries']:
  if row['sha256'] is not None:assert pin(ROOT/row['path'])==row,row['path']
 # Retain private predecessor log byte identity without copying its private content.
 control=json.loads((V2/'private-environment-control.json').read_bytes())
 assert pin(ROOT/control['private_raw_receipt']['path'])==control['private_raw_receipt']
 raw=git('archive','--format=tar',BASE);records=[];blobs={}
 authority=json.loads((ROOT/'evidence/04A38/late-archived-entry-supplement/v1/original-source-authenticity.json').read_bytes())
 known={r['path']:r for r in authority['files']}
 with tarfile.open(fileobj=io.BytesIO(raw),mode='r:') as archive:
  for item in archive:
   if item.isdir():continue
   assert item.isfile();data=archive.extractfile(item).read();name=item.name
   physical=Path('\\\\?\\'+str((TREE/name).absolute()));assert physical.read_bytes()==data
   token='<PRIVATE_PROTECTED_PATH:'+sha(name.encode())[:24]+'>';public=token if token in known else name
   record=dict(known[public]);assert record['bytes']==len(data) and record['sha256']==sha(data),public
   records.append(record);blobs[name]=data
 assert len(records)==4695
 snapshots=[]
 for row in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']:
  original=blobs[row['source']['path']];snapshot=(ROOT/row['snapshot']['path']).read_bytes()
  assert pin(ROOT/row['snapshot']['path'])==row['snapshot']
  assert original.replace(b'\r\n',b'\n')==snapshot.replace(b'\r\n',b'\n')
  if row['source']['path'].endswith('/UdfDataImportMixedModel.vb'):
   assert len(snapshot)==38595 and sha(snapshot)=='5297574cab59e631cf880807903a206f76dfb1509818a1825c1ca6798332071a'
  snapshots.append(dict(path=row['source']['path'],git_bytes=len(original),git_sha256=sha(original),snapshot=row['snapshot'],relation='EXACT' if original==snapshot else 'PROVED_LF_CRLF_ONLY',normalized_sha256=sha(original.replace(b'\r\n',b'\n'))))
 # Authenticate original synthetic INPUT slices against accepted pinned test source.
 source=blobs['BESHStatNG.Test/src/MixedModels/MixedModelOutputAndUdfTests.vb'].decode('utf-8-sig')
 fixture=(ROOT/'evidence/04A38/preedit/native-safety/FixtureInputs38.vb').read_text(encoding='utf-8')
 for name in ['BuildSyntheticMmrmInputs','BuildSyntheticLmmInputs']:
  block=re.search(r'^\s*Private Shared Sub '+name+r'\([\s\S]*?^\s*End Sub',source,re.M)[0]
  assert block.replace('Private Shared Sub '+name,'Public Shared Sub '+name,1).replace('\r\n','\n').strip() in fixture
 save('owner-authorization.json',dict(owner_quote='I approve the proposed two-case authorization.',owner_decision_date='2026-11-09',
      provenance='Explicit current owner execution prompt; adopts exact headered treatment_active=1.0,visit=1.0 two-culture proposal',
      cases_per_configuration=2,cultures=['Invariant','sk-SK'],configurations=['Debug','Release'],public_entry='BESH.REGR.MMRM_LSMESTIMATE',
      source_commit=BASE,reference_class=CLASS,recorded_utc=datetime.now(timezone.utc).isoformat(),
      approvalA_432=pin(ROOT/'evidence/04A38/late-archived-entry-supplement/v1/owner-authorization.json'),approvalB_12GEE=pin(V2/'owner-authorization.json'),
      prior_proposal=pin(V2/'owner-decision-needed.md'),prior04A37='Separate accepted one-batch Route A history; no transferred authority'))
 save('entry-inspection.json',dict(status='LIVE_ENTRY_AND_ORIGINAL_SOURCE_AUTHENTICATED',recorded_utc=datetime.now(timezone.utc).isoformat(),git=state,diff_check_exit=0,
      predecessor_manifest=pin(ROOT/PREVIOUS),predecessor_external_hashes=890,original_git_archive_bytes=len(raw),original_git_archive_sha256=sha(raw),
      original_tree_files=records,udf_snapshots=snapshots,original_fixture_source=pin(ROOT/'evidence/04A38/preedit/native-safety/FixtureInputs38.vb'),
      private_predecessor_control=control['private_raw_receipt'],source_commit=BASE,reference_class=CLASS,new_production_changes=0,new_reference_executions=0,
      TestResults='NOT_TRAVERSED_OR_REHASHED',uploaded_zip='Owner review artifact only; no live authority or fresh archive verification claimed'))
 result=command('predecessor-live-reader',[sys.executable,'-B',str(V2/'verify_closeout.py')]);assert result['exit_code']==0
 save('predecessor-live-reader.json',clean(result))
 print('Authenticated all4695 original Git files,41 UDF snapshots,890 predecessor payloads and original MMRM fixture slices; separate approval recorded')
if __name__=='__main__':main()
