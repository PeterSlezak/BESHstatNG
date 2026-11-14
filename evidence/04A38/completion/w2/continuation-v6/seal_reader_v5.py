from common import *
source=(HERE/'verify_checkpoint_v4.py').read_text(encoding='utf-8')
source=source.replace("need(bool(r['proof'])==e['supported'] and r['current_ids']==e['current_ids'],'OPEN or unsupported historical body disposition')", "need(bool(r['proof'])==e['supported'] and r['current_ids']==e['current_ids'],'OPEN or unsupported historical body disposition')\n   prior=histories[r['canonical_original_id']]\n   need(all(r[k]==v for k,v in prior.items() if k not in ['proof','current_ids','disposition','partial_implementation']),'Historical source identity metadata changed')")
source=source.replace("need({n['symbol_id'] for n in c['members'] if not n['review_status'].startswith('OPEN')}==closed,'OPEN or unsupported current body disposition')", "need({n['symbol_id'] for n in c['members'] if not n['review_status'].startswith('OPEN')}==closed,'OPEN or unsupported current body disposition')\n  need(all(all(n[k]==v for k,v in members[n['symbol_id']].items()) for n in c['members']),'Current body/source metadata differs from actual census')")
source=source.replace("need({s['support_id'] for s in c['support'] if s['fully_reconciled']}=={s['id'] for s in support if s['supported']},'Unsupported support declaration closure')", "need({s['support_id'] for s in c['support'] if s['fully_reconciled']}=={s['id'] for s in support if s['supported']},'Unsupported support declaration closure')\n  actual_support={s['support_id']:s for s in current['supports']}\n  need(all(all(s[k]==v for k,v in actual_support[s['support_id']].items()) for s in c['support']),'Support declaration source metadata differs')")
source=source.replace("need(set(actual_frags)=={f['fragment_id'] for f in current['fragments']},'Orphaned/missing fragment')", "need(len(actual_frags)==len(c['fragments']) and set(actual_frags)=={f['fragment_id'] for f in current['fragments']},'Orphaned/missing/duplicate fragment')\n  need(all(all(actual_frags[f['fragment_id']][k]==v for k,v in f.items()) for f in current['fragments']),'Fragment source/branch/inline metadata differs')")
needle="need(summary['historical']==hs and summary['current']==cs and summary['support']==ss and summary['fragments']==fragments,'Forged ownership/fragment totals')"
insert="""original_rows=load(PREFIX+'original-fragment-parent-crosswalk-v1.json')['rows']
  need(Counter(r['fragment_id'] for r in original_rows)==Counter(f['fragment_id'] for f in original['fragments']),'Original fragment crosswalk missing/duplicate')
  source_original={f['fragment_id']:f for f in original['fragments']};canonical={h['git_original_id']:h['canonical_original_id'] for h in old_history}
  for row in original_rows:
   f=source_original[row['fragment_id']];covered=f['member_id'] in proof_orig or f['fragment_id'] in old_headers
   need(row['parent']==f['member_id'] and row['canonical_original_parent']==canonical.get(f['member_id']) and row['is_branch']==f['is_branch'] and row['is_inline']==f['is_inline'],'Original fragment source/parent metadata differs')
   need((row['status']=='COVERED_BY_PROVEN_BODY_OR_DECLARATION')==covered,'Original branch/inline coverage forged')
  """+needle
assert source.count(needle)==1;source=source.replace(needle,insert,1)
needle="need(Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(set(members)-closed),'Remaining current queue incomplete')"
insert=needle+"""
  open_ids={r['id'] for r in expected_history if not r['supported']}
  for row in queue['members']+queue['first_members']:
   need(row['original_id'] in open_ids and row['path']==histories[row['original_id']]['path'] and row['name']==histories[row['original_id']]['name'],'Invented or stale canonical next action')
  need(len({r['original_id'] for r in queue['first_members']})==len(queue['first_members']),'Duplicate priority canonical identity')
  for row in queue['current_OPEN']:need(row['path']==members[row['current_id']]['path'] and row['name']==members[row['current_id']]['name'],'Current queue source identity differs')"""
assert source.count(needle)==1;source=source.replace(needle,insert,1)
with (HERE/'verify_checkpoint_v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
cross=json.loads((HERE/'proof-authority-crosswalk-v1.json').read_bytes())
for row in cross['rows']:row['authoritative_reader']='verify_checkpoint_v5.py'
save('proof-authority-crosswalk-v2.json',cross)
save('review-seal-v5.json',dict(reader=pin(HERE/'verify_checkpoint_v5.py'),prior=pin(HERE/'review-seal-v4.json'),
    hardening='Verify all source metadata and multiplicities in every current/support/fragment row, complete original crosswalk, and exact canonical priority queue. Counts/dispositions unchanged.',status='PARTIAL_W2_NOT_READY',original_queries=0))
