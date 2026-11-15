from publication_helpers import *
import base64,io

def scan(label):
 reports=candidate_scan(label)
 for r in reports:
  m=physical.scanner(r['scanner']);m.ROOT=ProbeRoot(json.dumps(dict(value='benign portable review control')));found,_=m.scan(['negative-control.json']);assert not found
  r['negative_control']=dict(zero_findings=True,no_identity_tokens_written=True)
  for i,token in enumerate(sorted(physical.tokens())):
   payload=base64.b64encode(('A'*90+' '+token+' '+'Z'*90).encode()).decode()
   m=physical.scanner(r['scanner']);m.ROOT=ProbeRoot(json.dumps(dict(payload=payload)));a,ar=m.scan(['encoded-control.json']);m.re=re;b,br=m.scan(['encoded-control.json'])
   assert a==b and ar==br and any(x['category']=='known_private_identifier' and x['origin']=='decoded_payload' for x in a)
   r['identity_controls'].append(dict(identity_ordinal=i,mode='base64_decoded_payload',detected=True,direct_original_and_guard_exact=True))
 return reports

def prepare():
 check_sources();files=[pin(p) for p in sorted(PRIVATE.rglob('*')) if p.is_file()]
 for r in files:(ROOT/r['path']).chmod(stat.S_IREAD)
 save(PRIVATE/'new-raw-inventory-v1.json',dict(files=files,count=len(files),bytes=sum(r['bytes'] for r in files),scope='All43-owned raw files after executed campaigns/before seal. Later privacy/publication receipts are pinned independently. Older raw stores unchanged.'))
 (PRIVATE/'new-raw-inventory-v1.json').chmod(stat.S_IREAD)
 failures=[]
 for p in sorted((PRIVATE/'failed-attempts').glob('*.json')):
  d=load(p);failures.append(dict(recipe=d['recipe'],type=d['type'],raw=pin(p),status='RETAINED_FAILED_ATTEMPT'))
 save('preservation/new-raw-review-v1.json',dict(kind='SANITIZED_INVENTORY_FACTS;NOT_RAW_LOGS',inventory=pin(PRIVATE/'new-raw-inventory-v1.json'),files=len(files),bytes=sum(r['bytes'] for r in files),read_only=True,failed_attempts=failures,old_stores_mutated=False,discovery_failures='Read-only interactive quoting/discovery errors remain in tool history;no fabricated receipts. Own failed capture process stopped only after executable/PID attribution.'))
 commands=[]
 for p in sorted((PRIVATE/'commands').glob('*/command.json')):
  d=load(p);commands.append(dict(label=p.parent.name,exit_code=d['exit_code'],seconds=d['seconds'],raw=pin(p),stdout_sha256=d['stdout_sha256'],stderr_sha256=d['stderr_sha256']))
 save('matrix/actual-commands-review-v1.json',dict(kind='SANITIZED_EXECUTED_COMMAND_FACTS;NOT_RAW_ARGV_TRANSCRIPT',commands=commands,recipe_directory='evidence/04A44/',interpreter='Existing accepted3.14.7 and packages;no installs',full_Debug_campaigns=1,full_Python_campaigns=1,full_Release='NOT_RUN',Node='INHERITED_916_PASS;NOT_RERUN',direct_verify_script='NOT_RUN_FIXED_PROTECTED_TRX_COLLISION;UNCHANGED_ISOLATED_RUNNER_EQUIVALENT',Excel_COM_UIA_R_Office_WASM_manual='NOT_RUN',failed_attempts_retained=True,stage_commit_merge_rebase_push_release=False,commands_to_reproduce='Use existing interpreter -I -B evidence/04A44/run44.py <recipe> [arguments];append-only writers require fresh version/directory names. Recipes and exact safe command labels are authoritative;raw argv remains controlled.',key_recipes=['authenticate-v1.py','entry-build-v1.py Debug/Release','entry-compiled-v1.py','capture-v3.py','capture-races-v1.py','wave-a-v1.py','wave-b-v1.py','close-wave-b-v4.py','final-managed-v1.py Debug/Release','compiled-and-fixtures-v1.py Debug/Release','compatibility-v1.py Debug/Release','python-campaign-v1.py prepare/execute','reconcile-python-v1.py','python-dispositions-v2.py','preservation-v2.py','serial-checks-v1.py','key-edge-readback-v1.py']))
 reports=scan('repository-initial')
 save('privacy/repository-candidate-v1.json',dict(status='ZERO_FINDINGS_FOR_RECORDED_ACTUAL_FULL_CANDIDATE_INPUTS',reports=reports,waivers=0,rules_ignore_and_scope_unchanged=True,owner_tokens_and_unsalted_identifier_hashes_published=False,history='Accepted39 FAIL1941each(1599+342);W3 privacyERROR;old40 raw failures;54+32+21+25 histories remain. Current privacy PASS is not historical waiver.',later_final_metadata='External receipt pins/rechecks full final candidate including later metadata. No self-hash claim.',gates=GATES))
 print('Protected43 inventory sealed;both unchanged actual-candidate scanner passes complete',flush=True)

from delivery44 import documents,package_files

def seal():
 check_sources();reports=scan('repository-final-full');files=package_files();rows=[];out=PRIVATE/'publication';out.mkdir();archive=out/'BESHStatNG_04A44_OWNER_REVIEW_v1.zip'
 with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
  for p in files:
   n=p.relative_to(ROOT).as_posix();physical.safe_name(n);b=p.read_bytes();rows.append(dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}'));z.writestr(n,b)
  z.writestr('.review-index.json',json.dumps(dict(kind='04A44_SANITIZED_OWNER_REVIEW',files=rows,self_hash_policy='Index excluded from own map,externally hashed/inventoried/scanned as physical member.'),indent=2)+'\n')
 archive.chmod(stat.S_IREAD);physical_reports=[]
 with zipfile.ZipFile(archive) as z:
  members=physical.inventory(z);assert len(members)==len(rows)+1 and len({unicodedata.normalize('NFKC',r['path']).casefold() for r in members})==len(members)
  for phase in ['13','14']:
   m=physical.scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig');found,reviewed=m.scan(z.namelist());p=out/('physical-scanner-'+phase+'-v1.json');save(p,dict(source=pin(m.__file__),findings=found,reviewed=reviewed,members=len(members)));p.chmod(stat.S_IREAD);assert not found,'UNRESOLVED_PHYSICAL_PRIVACY_FINDING'
   physical_reports.append(dict(scanner=phase,source=pin(m.__file__),scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),physical_members=len(members),index_included=True,findings=0,unresolved=0,owner_context_applied=True,raw=pin(p)))
 reader=module('reader44_final',HERE/'review_reader_v3.py');readback=reader.verify(archive);controls=reader.adverse(archive);assert len(controls)>=16
 # Real privacy-leak adverse control: altered in-memory physical ZIP through both ORIGINAL scanners, never written/published.
 for phase in ['13','14']:
  buf=io.BytesIO()
  with zipfile.ZipFile(buf,'w') as z:z.writestr('.review-index.json',json.dumps(dict(control='A'*90+' '+sorted(physical.tokens())[0])))
  buf.seek(0)
  with zipfile.ZipFile(buf) as z:
   m=physical.scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig');f,_=m.scan(z.namelist());assert any(r['category']=='known_private_identifier' for r in f)
  controls.append(dict(control='physical_privacy_leak_original_scanner_'+phase,detected=True,owner_token_published=False))
 check_sources()
 for report in reports:
  m=physical.scanner(report['scanner']);assert m.paths()==[r['path'] for r in report['members']]
  for r in report['members']:exact(r)
 for r in load(PRIVATE/'new-raw-inventory-v1.json')['files']:exact(r)
 primary=[pin(ROOT/'docs/architecture/batch-04A-44-owner-review.md')]+[pin(HERE/p) for p in ['final/decision-matrix-v1.json','scope/member-ownership-v2.json','fixtures/current-main-review-v1.json','register/issue-delta-v1.json','preservation/readback-v1.json']]
 receipt=dict(status=STATUS,utc=utc(),archive=pin(archive),primary_reports=primary,readiness=pin(HERE/'final/readiness-postseal-v1.json'),full_actual_candidate_scans=reports,physical_scans=physical_reports,physical_members=members,readback=readback,adverse_controls=controls,index_self_excluded_but_external_full_inventory=True,all_final_candidate_bytes_membership_rechecked_after_zip=True,all43_preseal_raw_inventory_rechecked=True,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,HEAD=MAIN,main=MAIN,merge_base=MAIN,branch='codex/04A-44',owner_accepted=False,merged=False,phase04A45_authorized=False,excluded=['Protected raw logs/TRX/diagnostics','Owner identity tokens and unsalted identifier hashes','Model data and fitted dumps','Binaries/giant reverse indices/full historic TestResults'],gates=GATES)
 rp=out/'external-delivery-receipt-v1.json';save(rp,receipt)
 for phase in ['13','14']:
  m=physical.scanner(phase);f,_=m.scan([rp.relative_to(ROOT).as_posix()]);assert not f,'EXTERNAL_RECEIPT_PRIVACY_FINDING'
 rp.chmod(stat.S_IREAD);assert reader.verify(archive,rp)['SHA_size_CRC']=='PASS'
 print(json.dumps(dict(status=STATUS,archive=pin(archive),external_receipt=pin(rp),members=len(members),adverse_controls=len(controls))),flush=True)

if __name__=='__main__':
 try:{'prepare':prepare,'documents':documents,'seal':seal}[sys.argv[1]]()
 except Exception:
  if sys.argv[1]=='seal':save(PRIVATE/('publication-adverse-'+datetime.datetime.now().strftime('%Y%m%d%H%M%S')+'.json'),dict(status='NOT_READY_04A44',reason='Final candidate/physical/readback gate did not complete;conditional readiness ineffective without matching external PASS receipt.',gates=GATES))
  raise
