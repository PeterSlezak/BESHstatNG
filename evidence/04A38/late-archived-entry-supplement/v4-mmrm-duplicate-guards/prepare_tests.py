"""Retain prior adverse suites; derive a separately named v4 suite."""
from common import *
def main():
 text=(V3/'test_supplement.py').read_text(encoding='utf-8')
 text=text.replace('MMRM_HEADERED_PROFILE_EVIDENCE_PASS_NOT_BATCH_READY','MMRM_DUPLICATE_GUARD_EVIDENCE_PASS_NOT_BATCH_READY').replace("'third_case'","'fifth_case'")
 text=text.replace("alter_json('cases_per_configuration',3)","alter_json('cases_per_configuration',5)")
 text=text.replace("b'System.Object[0:5,0:8]',b'System.Object[1:5,0:8]'","b'System.Object[0:3,0:2]',b'System.Object[1:3,0:2]'")
 text=text.replace("b'RETURN_ARRAY',b'RETURN_ERROR_CELL'","b'RETURN_TEXT',b'RETURN_ERROR_CELL'")
 anchor=" 'old_manifest_rewritten':(reader.PREVIOUS,lambda b:b+b' ')"
 assert text.count(anchor)==1
 text=text.replace(anchor,""" 'missing_true_visit_guard':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.duplicate_visit_time=True',b'guard.duplicate_visit_time=False',1)),
 'missing_true_design_guard':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.duplicate_design=True',b'guard.duplicate_design=False',1)),
 'first_visit_not_accepted':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'visit.previously_specified=True',b'visit.previously_specified=False',1)),
 'second_column_resolves_different_index':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'design.existing_index=1',b'design.existing_index=2',1)),
 'casefold_literal_changed':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[1]['operations'][1]['arguments'][3]['value']['values'][4].update(value='treatment_active'))),
 'missing_literal_Value_header':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][3]['value']['values'][1].update(value='val'))),
 'guard_case_wrong_second_numeric_value':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][3]['value']['values'][5].update(bits='3FF0000000000000'))),
 'changed_public_guard_message':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'SQkVT',b'SQkVU',1)),
 'old_v3_positive_rewritten':('evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile/original-results/Debug/plain/results.tsv',lambda b:b+b' '),
"""+anchor)
 with (HERE/'test_supplement.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
 save('adverse-test-lineage.json',dict(original=pin(V3/'test_supplement.py'),v4=pin(HERE/'test_supplement.py'),changes='Four-case matrix and negative guard outcomes, plus specific true-arm/first-acceptance/resolved-index/case-sensitive-input checks; original tests untouched',
      test_execution='Private mutation views only; no additional original/current variant invocation'))
 # The generic publication reader resolves v4 PREFIX/BASE and v3 predecessor.
 for name in ['verify_packet.py','test_packet.py']:
  data=(V3/name).read_bytes()
  with (HERE/name).open('xb') as stream:stream.write(data)
if __name__=='__main__':main()
