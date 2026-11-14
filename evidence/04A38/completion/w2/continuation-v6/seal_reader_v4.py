from common import *
source=(HERE/'verify_checkpoint_v3.py').read_text(encoding='utf-8')
old="if path.is_file():need((path.stat().st_size,sha(path.read_bytes()))==(a['bytes'],a['sha256']),'Actual compiled DLL identity changed')"
new="if path.is_file():\n     blob=raw(path.relative_to(root).as_posix());need((len(blob),sha(blob))==(a['bytes'],a['sha256']),'Actual compiled DLL identity changed')"
assert source.count(old)==1;source=source.replace(old,new,1)
old="comparisons=load(PREFIX+'compatibility-families-v2.json')['results'];totals=Counter()"
new="""comparisons=load(PREFIX+'compatibility-families-v2.json')['results'];totals=Counter()
 old_comparisons={(r['configuration'],r['corpus']):r for r in load(V5+'compatibility-cells-v5.json')['results']}
 need(sha(raw(PREFIX+'compatibility-families-v2.json'))=='"""+sha((HERE/'compatibility-families-v2.json').read_bytes())+"""','Authenticated current replay receipt changed')"""
assert source.count(old)==1;source=source.replace(old,new,1)
old="for r in comparisons:\n  for key in ['original','current','inputs','recorder']:check_pin(r[key])"
new="""for r in comparisons:
  old=old_comparisons[r['configuration'],r['corpus']]
  need(all(r[k]==old[k] for k in ['original','inputs','recorder','cases']),'Original reference/recorder/provenance substitution')
  need(r['current']['path']==PREFIX+'captures/families-v2/'+r['configuration']+'/'+r['corpus']+'/results.tsv','Replay points outside actual current capture')
  for key in ['original','current','inputs','recorder']:check_pin(r[key])"""
assert source.count(old)==1;source=source.replace(old,new,1)
with (HERE/'verify_checkpoint_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
h=json.loads((HERE/'historical-ownership-v1.json').read_bytes())['members']
save('proof-authority-crosswalk-v1.json',dict(status='EXACT_FINAL_AUTHORITY_WITH_IMMUTABLE_PRIOR_REFERENCES',
    rows=[dict(original_id=r['canonical_original_id'],current_ids=r['current_ids'],supported=bool(r['proof']),
         authoritative_reader='verify_checkpoint_v4.py',authoritative_body_dossier='whole-body-contracts-v1.json' if r['proof'] and r['proof']['kind'].startswith('C6_') else 'continuation-v5/verify_ownership_final.py exact entry projection and transitive immutable dossiers' if r['proof'] else None,
         prior_ledger_proof_preserved=r['proof'],compiled_current_context='metadata-full/families-v2/current-{Debug|Release}.json',original_source='preedit/entry-source-snapshot.json') for r in h],
    note='Earlier ledger prose filenames remain historical references; the executable final readers select authenticated final dossiers explicitly. No historical file is edited.',original_queries=0))
save('review-seal-v4.json',dict(reader=pin(HERE/'verify_checkpoint_v4.py'),prior=pin(HERE/'review-seal-final.json'),status='PARTIAL_W2_NOT_READY',original_queries=0))
print('Final checkpoint reader sealed with actual-DLL mutation seam and exact current replay provenance')
