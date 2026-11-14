"""Append-only blocked closeout. Never overwrite a sealed or historical file."""
import importlib.util,json,os,re,subprocess,sys
from collections import Counter
from datetime import datetime,timezone
from pathlib import Path
from prepare_original import ROOT,HERE,BASE,PRIVATE,pin,save,git,sha

def module(name,path):
 spec=importlib.util.spec_from_file_location(name,path);obj=importlib.util.module_from_spec(spec);spec.loader.exec_module(obj);return obj

def preservation():
 entry=json.loads((ROOT/'evidence/04A38/entry-source-pins.json').read_bytes())['files']
 transitions=json.loads((ROOT/'evidence/04A38/source-transitions-validation-v2.json').read_bytes())['transitions'];changed={r['path']:r for r in transitions}
 tracked=[p for p in git('ls-files','-z').decode().split('\0') if p];private={'<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>':p for p in tracked}
 records=[];counts=Counter()
 for row in entry:
  path=row['path'];real=private[path] if path.startswith('<PRIVATE_PROTECTED_PATH:') else path
  actual=pin(ROOT/real);actual['path']=path
  assert actual==(changed[path]['current'] if path in changed else row),path
  category='PRIVACY_REDACTED' if path.startswith('<PRIVATE_PROTECTED_PATH:') else 'GITHUB' if path.startswith('.github/') else 'TEST_DEPENDENCY' if path.startswith('BESHStatNG.Test/packages/') else 'OTHER'
  counts[category]+=1;records.append(dict(path=path,current=actual,status='AUTHORIZED_CHECKPOINT_TRANSITION' if path in changed else 'EXACT_ENTRY'))
 assert len(records)==4695 and len(changed)==32
 old=json.loads((ROOT/'evidence/04A38/preservation-validation-v2.json').read_bytes())
 for row in old['lineage_pins']:assert pin(ROOT/row['path'])==row['current']
 for row in old['protected_mixed_model_captures']+old['owner_documents_excluded']:assert pin(ROOT/row['path'])==row
 checkpoint=json.loads((ROOT/'evidence/04A38/continuation/v1/publication-manifest.json').read_bytes())
 for row in checkpoint['entries']:
  if row['sha256'] is not None:assert pin(ROOT/row['path'])==row
 # The continuation's null self-entry is now an ordinary hashed predecessor.
 save('preservation.json',dict(status='PASS',tracked_entry=4695,exact_entry=4663,authorized_checkpoint_transitions=32,new_production_test_project_changes=0,
  records=records,categories=dict(counts),lineage_pins=278,protected_mixed_model_captures=100,owner_documents=16,previous_continuation_external_hashes=687,
  previous_manifests=[pin(ROOT/'evidence/04A38/publication-manifest.json'),pin(ROOT/'evidence/04A38/continuation/v1/publication-manifest.json')],
  omitted_zip_paths='All4695 LIVE paths verified, including389 omitted from uploaded checkpoint; no claim those bytes exist in ZIP',historical_TestResults='NOT_TRAVERSED_OR_REHASHED'))

def validate():
 if not (HERE/'preservation.json').exists():preservation()
 else:
  for row in json.loads((HERE/'preservation.json').read_bytes())['records']:
   if not row['path'].startswith('<PRIVATE_PROTECTED_PATH:'):assert pin(ROOT/row['path'])==row['current']
 accepted=module('accepted38runner',ROOT/'scripts/test_suite_rationalization.py');runner=accepted.Runner(PRIVATE.parent)
 commands=[]
 lanes=[('supplement-reader',['evidence/04A38/late-archived-entry-supplement/v1/verify_supplement.py']),
  ('supplement-adverse',['-m','unittest','discover','-s','evidence/04A38/late-archived-entry-supplement/v1','-p','test_supplement.py','-v']),
  ('checkpoint-reader',['evidence/04A38/verify_review_packet.py']),
  ('continuation-reader',['evidence/04A38/continuation/v1/verify_continuation.py']),
  ('preedit-adverse-isolated-v2',['evidence/04A38/late-archived-entry-supplement/v1/replay_preedit_adverse.py']),
  ('checkpoint-adverse',['-m','unittest','discover','-s','evidence/04A38','-p','test_review_packet.py','-v']),
  ('continuation-adverse',['-m','unittest','discover','-s','evidence/04A38/continuation/v1','-p','test_branch_audit.py','-v'])]
 for name,args in lanes:
  receipt_path=PRIVATE.parent/'commands'/('late38-closeout-'+name)/'command.json'
  result=json.loads(receipt_path.read_bytes()) if receipt_path.exists() else runner.command('late38-closeout-'+name,[sys.executable,'-B',*args])
  assert result['exit_code']==0,(name,result['exit_code'])
  # Only sanitized commands/digests are public. Raw runner logs remain private.
  def scrub(value):
   if isinstance(value,str):return value.replace(str(ROOT),'<REPO_ROOT>').replace(str(Path(sys.executable)),'<PYTHON>')
   if isinstance(value,list):return [scrub(x) for x in value]
   if isinstance(value,dict):return {k:scrub(v) for k,v in value.items()}
   return value
  commands.append(dict(lane=name,receipt=scrub(result)))
  print(name,'exit0',flush=True)
 report=module('late38independent',HERE/'verify_supplement.py').inspect()
 save('compatibility-comparison.json',report)
 save('validation.json',dict(executed=commands,source_builds='Pristine original Core/Common/Windows Debug/Release offline compile succeeded; separate current UDF harness compiled both configurations',
  original_captures='432/config; plain/repeat/trace exact; current432/config exact after seal',
  not_rerun=['Full managed Debug/Release','Node','Python aggregate','Historical packet suites34-37','Strict/profile/MkDocs','Original28966 capture generation'],
  reason='Evidence-only work; no product/test/project change after checkpoint. New essential GEE profile gap triggers explicit owner-decision stop.',
  previous_receipts_only=dict(managed_each=dict(passed=20556,failed=14,new_passes=49),node_passed=916,python=dict(passed=767,failed=178,errors=11),strict_profile_mkdocs=[1,1,0]),
  nonpasses='Checkpoint nonpasses and test identities unchanged. Old whole-source pin failures and synthetic privacy-fixture flags remain FAIL; historical scientific/native gates unmet.',
  wrappers='NOT_RUN: canonical wrappers imply restore/shared output/packaging; safe offline isolated compile/capture commands recorded instead',
  native='NOT_RUN',r_oracle='NOT_RUN',scientific='NOT_GOLDEN'))

def seal():
 assert git('branch','--show-current').decode().strip()=='codex/04A-38'
 assert all(git(*args).decode().strip()==BASE for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')])
 assert not git('diff','--cached','--name-only')
 check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True)
 assert check.returncode==0
 status=git('status','--short').decode();names=git('diff','--name-status').decode()
 save('git-final.json',dict(branch='codex/04A-38',head=BASE,main=BASE,merge_base=BASE,index='EMPTY',unstaged_uncommitted=True,
  diff_check=dict(exit_code=check.returncode,output=check.stdout.decode(),stderr=check.stderr.decode()),
  status_short=status,porcelain_v1=git('status','--porcelain=v1').decode(),diff_name_status=names,diff_stat=git('diff','--stat').decode(),
  ignored_evidence_inventory='Explicitly included below; no broad ignored TestResults traversal',no_git_writes=True))
 save('readiness.json',dict(status='NOT_READY_04A38',phase='APPROVED432_COMPLETE_NEW_ESSENTIAL_GEE_PROFILE_GAP',source_commit=BASE,
  approved432=dict(owner_quote='Yes, I approve',date='2026-11-08',cases_per_configuration=432,exact_configurations=['Debug','Release'],
   reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY'),
  original28966='UNCHANGED; distinct genuine before-production freeze; never combined chronologically with late432',
  blocked_member='BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb|BESHStatNG.WorksheetFunctions.GEEUDFs|FunctionBlock|4a1806f9c5baa4a2@99887:2213',
  earliest_missing_case='PROPOSED-GEE-PROFILE/Invariant/name-value-time',new_profile_reference_authorized=False,new_profile_reference_executed=False,
  previous_blockers=['704 all-statement dispositions incomplete:199 consumer-positive/body-unreviewed plus505 remaining review',
   '719 current/317 support/inline identity reconciliation incomplete','Remaining import/profile/report production extraction incomplete','Full independent READY acceptance verifier not implemented'],
  production_changes_this_supplement=0,registration_contracts='268 checkpoint contracts unchanged; not freshly reinspected in compiled full host',
  next_action='Separate owner decision for proposed12 GEE-profile cases; no automatic expansion. Then resume source-backed completeness review, not acceptance of partial work.',
  gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),native='NOT_RUN',r_oracle='NOT_RUN',scientific='NOT_GOLDEN',next_batch='04A39_NOT_AUTHORIZED'))
 old=json.loads((ROOT/'evidence/04A38/continuation/v1/publication-manifest.json').read_bytes());paths={r['path'] for r in old['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 prefix=HERE.relative_to(ROOT).as_posix()+'/'
 self_path=prefix+'publication-manifest.json';inventory=prefix+'file-inventory.json';privacy=prefix+'privacy.json'
 paths.update([self_path,inventory,privacy])
 assert len({p.casefold() for p in paths})==len(paths)
 scan=module('late38privacy',ROOT/'evidence/04A38/privacy_current.py');findings=scan.inspect(sorted(paths-{self_path,inventory,privacy}));assert not findings,findings
 save('privacy.json',dict(status='PASS',artifacts_scanned=len(paths)-3,findings=[],raw_logs_packages_assemblies='PRIVATE_NOT_PROPOSED_FOR_PUBLICATION',owner_documents_excluded=16))
 ignored=[p for p in paths if any(part in {'Debug','Release'} for part in Path(p).parts)]
 save('file-inventory.json',dict(status='LOCAL_NOT_READY_REVIEW_ONLY',paths=sorted(paths),ignored_configuration_payloads_explicitly_included=sorted(ignored),
  previous_packet_external_count=687,new_family='Approved432 late supplement; unexecuted separate GEE-profile proposal; no source changes'))
 rows=[dict(path=p,bytes=None,sha256=None) if p==self_path else pin(ROOT/p) for p in sorted(paths)]
 save('publication-manifest.json',dict(status='NOT_READY_04A38',scope='VERSIONED_LATE432_SUPPLEMENT_AND_BLOCKED_COMPLETION; LOCAL_REVIEW_ONLY',
  predecessor=pin(ROOT/'evidence/04A38/continuation/v1/publication-manifest.json'),entries=rows))
 print('Sealed',len(rows)-1,'external hashes +1 null self-entry; no Git publication')

def verify_final():
 accepted=module('accepted38runner',ROOT/'scripts/test_suite_rationalization.py');runner=accepted.Runner(PRIVATE.parent)
 for name,args in [('packet-reader',['evidence/04A38/late-archived-entry-supplement/v1/verify_packet.py']),
  ('packet-adverse',['-m','unittest','discover','-s','evidence/04A38/late-archived-entry-supplement/v1','-p','test_packet.py','-v'])]:
  result=runner.command('late38-final-'+name,[sys.executable,'-B',*args]);assert result['exit_code']==0
 print('Final NOT_READY integrity reader and14 private-copy manifest tests PASS; raw command receipts remain private')

if __name__=='__main__':
 {'validate':validate,'seal':seal,'verify-final':verify_final}[sys.argv[1]]()
