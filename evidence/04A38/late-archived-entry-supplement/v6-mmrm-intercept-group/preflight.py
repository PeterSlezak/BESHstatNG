"""Live source, prior-packet and owner authority authentication before capture."""
import io,re,tarfile
from datetime import datetime,timezone
from common import *
QUOTE='I approve the additional two-case-per-configuration MMRM intercept-group archived-entry supplement for Phase 04A.38, limited to `MMRM_LSMEANS(handle, "Intercept", Nothing)`, one case in Invariant and one in sk-SK, in both Debug and Release.\n\nCodex must authenticate the original source and fitted model, seal typed inputs and genuine original outputs before current comparisons, and prove that the original intercept-rejection guard at lines 1477–1479 executes. All previous evidence must remain intact. Any further essential reference gap requires separate owner authorization. For the next prompt, can you create a "larger" prompt to complete the whole 04A.38 in next 2 or 3 batches (if that\'s feasible).'

def main():
 state=dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),
      merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY' if not git('diff','--cached','--name-only') else 'NONEMPTY',
      status=git('status','--porcelain=v1','--untracked-files=all').decode(),worktrees=clean(git('worktree','list').decode()))
 assert state['branch']=='codex/04A-38' and state['head']==state['main']==state['merge_base']==BASE and state['index']=='EMPTY'
 checked=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert checked.returncode==0
 previous=json.loads((ROOT/PREVIOUS).read_bytes());assert len(previous['entries'])==1250
 for row in previous['entries']:
  if row['sha256'] is not None:assert pin(ROOT/row['path'])==row,row['path']
 archive=git('archive','--format=tar',BASE);records=[];blobs={}
 known={r['path']:r for r in json.loads((ROOT/'evidence/04A38/late-archived-entry-supplement/v1/original-source-authenticity.json').read_bytes())['files']}
 with tarfile.open(fileobj=io.BytesIO(archive),mode='r:') as tar:
  for item in tar:
   if item.isdir():continue
   assert item.isfile();data=tar.extractfile(item).read();name=item.name
   assert Path('\\\\?\\'+str((TREE/name).absolute())).read_bytes()==data
   token='<PRIVATE_PROTECTED_PATH:'+sha(name.encode())[:24]+'>';public=token if token in known else name
   row=known[public];assert len(data)==row['bytes'] and sha(data)==row['sha256'],public
   records.append(row);blobs[name]=data
 assert len(records)==4695
 snapshots=[]
 for row in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']:
  data=blobs[row['source']['path']];snapshot=(ROOT/row['snapshot']['path']).read_bytes()
  assert pin(ROOT/row['snapshot']['path'])==row['snapshot'] and data.replace(b'\r\n',b'\n')==snapshot.replace(b'\r\n',b'\n')
  if row['source']['path'].endswith('/UdfDataImportMixedModel.vb'):
   assert len(snapshot)==38595 and sha(snapshot)=='5297574cab59e631cf880807903a206f76dfb1509818a1825c1ca6798332071a'
   lines=snapshot.decode('utf-8-sig').splitlines();assert lines[725].strip()=='If atProfile.VisitSpecified Then' and lines[743].strip()=='If existing.ColumnIndex = idx Then'
  if row['source']['path'].endswith('/MixedModelUDFs.vb'):
   assert len(snapshot)==92704 and sha(snapshot)=='596071f9fdb00fb94585e929a6899494293d2dc7ee0064faadf098b0d95dc13d'
   lines=snapshot.decode('utf-8-sig').splitlines();assert lines[1476].strip().startswith('If String.Equals(h.FixedEffectNames(idx),') and lines[1479].strip()=='Return False'
  snapshots.append(dict(path=row['source']['path'],git_bytes=len(data),git_sha256=sha(data),snapshot=row['snapshot'],relation='EXACT' if data==snapshot else 'PROVED_LF_CRLF_ONLY',normalized_sha256=sha(data.replace(b'\r\n',b'\n'))))
 fixture=(ROOT/'evidence/04A38/preedit/native-safety/FixtureInputs38.vb').read_text(encoding='utf-8')
 original=blobs['BESHStatNG.Test/src/MixedModels/MixedModelOutputAndUdfTests.vb'].decode('utf-8-sig').replace('\r\n','\n')
 for name in ['BuildSyntheticMmrmInputs','BuildSyntheticLmmInputs']:
  block=re.search(r'^\s*Private Shared Sub '+name+r'\([\s\S]*?^\s*End Sub',original,re.M)[0]
  assert block.replace('Private Shared Sub '+name,'Public Shared Sub '+name,1).strip() in fixture
 save('owner-authorization.json',dict(approval='F',owner_quote=QUOTE,owner_decision_date='2026-11-09',recorded_utc=datetime.now(timezone.utc).isoformat(),
      provenance='Explicit current execution prompt adopts precisely the scalar Intercept LS-means grouping guard in two cultures; not owner acceptance',source_commit=BASE,reference_class=CLASS,
      cases_per_configuration=2,cultures=['Invariant','sk-SK'],configurations=['Debug','Release'],case_kinds=['mmrm-lsmeans-scalar-intercept-group'],
      public_entry='BESH.REGR.MMRM_LSMEANS',preceding_proposal=pin(V5/'owner-decision-needed.md'),
      approvalA=pin(ROOT/'evidence/04A38/late-archived-entry-supplement/v1/owner-authorization.json'),approvalB=pin(V2/'owner-authorization.json'),approvalC=pin(V3/'owner-authorization.json'),approvalD=pin(V4/'owner-authorization.json'),approvalE=pin(V5/'owner-authorization.json'),
      original_expected_outputs='UNKNOWN_NOT_CAPTURED',native='NOT_RUN',scientific='NOT_GOLDEN'))
 save('entry-inspection.json',dict(status='LIVE_ENTRY_AND_ORIGINAL_SOURCE_AUTHENTICATED',recorded_utc=datetime.now(timezone.utc).isoformat(),git=state,diff_check_exit=0,
      predecessor_manifest=pin(ROOT/PREVIOUS),predecessor_external_hashes=1249,original_git_archive_bytes=len(archive),original_git_archive_sha256=sha(archive),
      original_tree_files=records,udf_snapshots=snapshots,source_commit=BASE,reference_class=CLASS,new_production_changes=0,new_reference_executions=0,
      original_fixture_source=pin(ROOT/'evidence/04A38/preedit/native-safety/FixtureInputs38.vb'),original_v3_factory=pin(V3/'factory-inputs.json'),
      plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),historical_TestResults='NOT_TRAVERSED_OR_REHASHED',uploaded_zip='Historical owner review receipt only; not a new ZIP/live-state verification'))
 result=command('predecessor-reader',[sys.executable,'-B',str(V5/'verify_closeout.py'),'v2']);assert result['exit_code']==0
 save('predecessor-reader.json',clean(result))
 print('Authenticated live Git,1249 predecessor payloads,4695 original files,41 snapshots and original intercept guard1477-1480; Approval F recorded')
if __name__=='__main__':main()
