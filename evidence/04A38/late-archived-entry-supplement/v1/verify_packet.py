"""Independent stdlib integrity reader for THIS NOT_READY successor packet.

This is deliberately not the still-required full migration acceptance reader.
It cannot certify READY, completed body ownership, science or native rendering.
"""
import hashlib,json,re
from pathlib import Path,PurePosixPath

ROOT=Path(__file__).resolve().parents[4]
PREFIX='evidence/04A38/late-archived-entry-supplement/v1/'
SELF=PREFIX+'publication-manifest.json'
ANCHORS={
 'evidence/04A38/preedit/frozen-v2/receipt.json':'9f9a55337311bf37e010dcc078cb2ff8a8384122c1b105c4593732e90e13d165',
 'evidence/04A38/preedit/addendum-frozen-v1/receipt.json':'35c790471841ce33fd2857ca881ff84103b2856cf61139fe1312bf002eb30646',
 PREFIX+'original-output-seal.json':'031d54ad25ad01f2c85ece31b136aaa2669fb4787f45a346f0a0321132845459'}
DENIED={'P':'NO','N1':'NO GO','N2':'NO','C2':'UNMET','C3':'UNAPPROVED'}
class InvalidPacket(ValueError):pass
def need(condition,message):
 if not condition:raise InvalidPacket(message)
def sha(data):return hashlib.sha256(data).hexdigest()
def inspect(root=ROOT):
 root=Path(root).resolve()
 def load(path):return json.loads((root/path).read_bytes())
 manifest=load(SELF);need(manifest['status']=='NOT_READY_04A38','This reader cannot certify READY')
 entries=manifest['entries'];paths=[r['path'] for r in entries]
 need(paths==sorted(paths),'Manifest reordered')
 need(len(set(p.casefold() for p in paths))==len(paths),'Duplicate/casefold collision')
 need(paths.count(SELF)==1,'Missing/duplicate self-entry')
 reserved=re.compile(r'(?i)^(?:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)')
 for row in entries:
  p=row['path'];parts=PurePosixPath(p)
  need(bool(p) and not parts.is_absolute() and '\\' not in p and ':' not in p,'Unsafe path')
  need(all(x not in {'.','..',''} and not x.endswith((' ','.')) and not reserved.match(x) for x in p.split('/')),'Traversal/reserved path')
  need((root/p).resolve().is_relative_to(root),'Symlink escape')
  need(not p.startswith(('.git/','.test-rationalization-','packages/')),'Foreign/private path')
  if p==SELF:need(row['bytes'] is None and row['sha256'] is None,'Recursive self hash')
  else:need(type(row['bytes']) is int and row['bytes']>=0 and re.fullmatch('[0-9a-f]{64}',row['sha256'] or ''),'Malformed external digest')
 inventory=load(PREFIX+'file-inventory.json');need(inventory['paths']==paths,'Missing/foreign payload inventory')
 for row in entries:
  if row['path']==SELF:continue
  data=(root/row['path']).read_bytes();need(len(data)==row['bytes'] and sha(data)==row['sha256'],'Changed payload: '+row['path'])
 for p,digest in ANCHORS.items():need(sha((root/p).read_bytes())==digest,'Changed chronology anchor')
 predecessor=load('evidence/04A38/continuation/v1/publication-manifest.json')
 for row in predecessor['entries']:
  need(row['path'] in paths,'Lost predecessor payload')
  if row['sha256'] is not None:need(next(r for r in entries if r['path']==row['path'])==row,'Changed historical assertion')
 ready=load(PREFIX+'readiness.json')
 need(ready['status']=='NOT_READY_04A38' and ready['previous_blockers'],'False READY/body-closure claim')
 need(not ready['new_profile_reference_authorized'] and not ready['new_profile_reference_executed'],'Unapproved GEE reference')
 need(ready['gates']==DENIED and ready['native']=='NOT_RUN' and ready['scientific']=='NOT_GOLDEN','Gate promotion')
 need(ready['production_changes_this_supplement']==0,'Unaccounted production change')
 compare=load(PREFIX+'compatibility-comparison.json')
 need(compare['status']=='LATE_SUPPLEMENT_EXACT_AND_COVERAGE_PASS_NOT_BATCH_READY','Wrong comparison qualification')
 for config in ['Debug','Release']:
  row=compare['configurations'][config];need(row['cases']==432 and row['exact'] and row['actual_branches_exact'],'Incomplete432 comparison')
 preserve=load(PREFIX+'preservation.json')
 need((preserve['tracked_entry'],preserve['exact_entry'],preserve['authorized_checkpoint_transitions'])==(4695,4663,32),'Lost tracked preservation')
 need((preserve['lineage_pins'],preserve['protected_mixed_model_captures'],preserve['owner_documents'])==(278,100,16),'Lost protected evidence')
 git=load(PREFIX+'git-final.json')
 need(git['index']=='EMPTY' and git['diff_check']['exit_code']==0 and git['diff_check']['output']=='' and git['no_git_writes'],'Dirty index/whitespace/publication')
 return dict(status='VALID_NOT_READY_LATE_SUPPLEMENT_PACKET',external_hashes=len(entries)-1,late_cases_each_configuration=432,
  original_cases_each_configuration_separate=28966,remaining='GEE coverage decision; incomplete704 ownership/719 current/317 support; remaining migration; full acceptance verifier',
  native='NOT_RUN',scientific='NOT_GOLDEN')

if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except (InvalidPacket,OSError,KeyError,ValueError,TypeError) as error:
  print(json.dumps(dict(status='NOT_READY_04A38',integrity_error=str(error))));raise SystemExit(1)
