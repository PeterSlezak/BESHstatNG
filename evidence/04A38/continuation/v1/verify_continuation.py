"""Independent integrity/stop-gate reader, NOT a full 04A.38 acceptance verifier."""
import hashlib,json
from pathlib import Path,PurePosixPath

ROOT=Path(__file__).resolve().parents[4]
SELF='evidence/04A38/continuation/v1/publication-manifest.json'
ANCHORS={
 'evidence/04A38/preedit/frozen-v2/receipt.json':'9f9a55337311bf37e010dcc078cb2ff8a8384122c1b105c4593732e90e13d165',
 'evidence/04A38/preedit/addendum-frozen-v1/receipt.json':'35c790471841ce33fd2857ca881ff84103b2856cf61139fe1312bf002eb30646'}
def sha(data):return hashlib.sha256(data).hexdigest()
def read(path):return json.loads((ROOT/path).read_bytes())
def verify():
 manifest=read(SELF);assert manifest['status']=='NOT_READY_04A38'
 entries=manifest['entries'];assert [r['path'] for r in entries]==sorted(r['path'] for r in entries)
 assert len({r['path'].casefold() for r in entries})==len(entries)
 self_rows=[r for r in entries if r['path']==SELF];assert len(self_rows)==1 and self_rows[0]['bytes'] is None and self_rows[0]['sha256'] is None
 for row in entries:
  path=row['path'];parts=PurePosixPath(path);assert not parts.is_absolute() and '..' not in parts.parts and '\\' not in path and ':' not in path
  target=ROOT/path;assert target.resolve().is_relative_to(ROOT.resolve())
  if path==SELF:continue
  data=target.read_bytes();assert len(data)==row['bytes'] and sha(data)==row['sha256'],path
 assert set(read('evidence/04A38/continuation/v1/file-inventory.json')['paths'])=={r['path'] for r in entries}
 checkpoint=read('evidence/04A38/continuation/v1/entry.json')
 for row in checkpoint['checkpoint_payloads']:
  data=(ROOT/row['path']).read_bytes();assert len(data)==row['bytes'] and sha(data)==row['sha256']
 for path,digest in ANCHORS.items():assert sha((ROOT/path).read_bytes())==digest
 ready=read('evidence/04A38/continuation/v1/readiness.json')
 assert ready['status']=='NOT_READY_04A38' and ready['expected_bytes_for_missing_cases']=='UNKNOWN_NOT_CAPTURED'
 assert ready['continuation_production_edits'] is False and ready['new_reference_executions'] is False
 assert ready['owner_decision']['authorization_from_this_continuation'] is False
 assert ready['constraints']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED')
 assert ready['native']=='NOT_RUN' and ready['scientific']=='NOT_GOLDEN'
 audit=read('evidence/04A38/continuation/v1/frozen-branch-coverage-v2.json')
 assert audit['status']=='NOT_READY_04A38' and len(audit['missing'])==5
 for config in ['Debug','Release']:
  for culture in ['Invariant','sk-SK']:
   row=audit['counts'][config+'/'+culture+'/calibration']
   assert row['cases']==29 and row['member_entered']==1 and row['member_not_entered']==28
   assert row['bins_token_N']==row['method_token_N']==row['weights_token_N']==29 and row['nonfinite_CI_rows']==0
 proposal=read('evidence/04A38/continuation/v1/proposed-missing-case-matrix.json')
 assert proposal['status']=='PROPOSED_NOT_AUTHORIZED_NOT_EXECUTED' and len(proposal['cases'])==432
 assert len({r['case_id'] for r in proposal['cases']})==432 and all(r['expected_result']=='UNKNOWN_NOT_CAPTURED' for r in proposal['cases'])
 return dict(status='VALID_BLOCKED_CONTINUATION_PACKET',external_hashes=len(entries)-1,checkpoint_external_bytes_unchanged=611,essential_coverage_gaps=5,
  new_production_edits=0,new_reference_executions=0,acceptance='NOT_READY_04A38; separate owner decision required',native='NOT_RUN',scientific='NOT_GOLDEN')

if __name__=='__main__':
 try:print(json.dumps(verify()))
 except (AssertionError,OSError,KeyError,ValueError) as error:
  print(json.dumps(dict(status='NOT_READY_04A38',integrity_error=str(error))));raise SystemExit(1)
