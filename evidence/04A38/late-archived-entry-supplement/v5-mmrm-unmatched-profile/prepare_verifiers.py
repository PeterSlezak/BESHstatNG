"""Derive fixture mechanics only; independent v5 assertions remain source-specific."""
from pathlib import Path
HERE=Path(__file__).resolve().parent;OLD=HERE.parent/'v4-mmrm-duplicate-guards'
for name in ['verify_packet.py','test_packet.py','publish_gate.py','validate_prior.py']:
 text=(OLD/name).read_text(encoding='utf-8').replace('mmrm38-v4','mmrm38-v5').replace('Append-only v4','Append-only v5').replace('BOUNDED_V4_DUPLICATE_GUARD_GATE_ONLY','BOUNDED_V5_UNMATCHED_PROFILE_GATE_ONLY')
 if name=='publish_gate.py':text=text.replace("duplicate_guard_evidence='4/4 exact each configuration", "unmatched_profile_evidence='2/2 exact each configuration")
 if name=='validate_prior.py':
  text=text.replace(" lanes=[", " v4=p+'late-archived-entry-supplement/v4-mmrm-duplicate-guards'\n lanes=[('duplicate-reader',[v4+'/verify_supplement.py']),('duplicate-closeout-reader',[v4+'/verify_closeout.py']),")
  text=text.replace("('checkpoint-adverse',p,", "('duplicate-adverse',v4,'test_supplement.py'),('duplicate-manifest-adverse',v4,'test_packet.py'),('old-mask-gap-adverse',v4,'test_mask_gap.py'),('checkpoint-adverse',p,")
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(OLD/'test_supplement.py').read_text(encoding='utf-8')
head=text[:text.index('CASES={')].replace('third reference/input variant','third reference/input variant').replace('MMRM_DUPLICATE_GUARD_EVIDENCE_PASS_NOT_BATCH_READY','MMRM_UNMATCHED_PROFILE_EVIDENCE_PASS_NOT_BATCH_READY')
lines=text[text.index('CASES={')+8:text.index('\n}\ndef make_test')].splitlines()
exclude=['missing_original_branch','missing_second_profile_setting','wrong_header_columns','guard_as_report','missing_true_visit_guard','missing_true_design_guard','first_visit_not_accepted','second_column_resolves_different_index','casefold_literal_changed','missing_literal_Value_header','guard_case_wrong_second_numeric_value']
selected=[]
for line in lines:
 if not line.strip():continue
 name=line.strip().split("'")[1]
 if name in exclude:continue
 line=line.replace("'fifth_case'","'third_case'").replace("[3]['value']","[1]['value']").replace("['values'][3]","['values'][1]")
 line=line.replace("lengths=[2,3]","lengths=[1,2]").replace("bits='3FF0000000000001'","bits='4058C00000000001'")
 line=line.replace("b'System.Object[0:3,0:2]',b'System.Object[1:3,0:2]'","b'System.Object[0:2,0:1]',b'System.Object[1:2,0:1]'")
 selected.append(line)
tail=text[text.index('def make_test'):]
extra='''
 'old_v4_duplicate_rewritten':('evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards/original-results/Debug/plain/results.tsv',lambda b:b+b' '),
 'unmatched_true_arm_missing':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.unmatched=True',b'guard.unmatched=False',1)),
 'mask_one_entry_true':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'B0|B0',b'B1|B0',1)),
 'mask_incorrect_length':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'System.Boolean[0:54]',b'System.Boolean[0:53]',1)),
 'mask_one_based':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'System.Boolean[0:54]',b'System.Boolean[1:54]',1)),
 'matched_count_fabricated':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.matched_count=0',b'guard.matched_count=1',1)),
 'row53_omitted':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'row.index=53',b'row.index=52',1)),
 'row_visit_wrong':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'row.visit=D3FF',b'row.visit=D3FE',1)),
 'profile_not_formed':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'profile.formed=True',b'profile.formed=False',1)),
 'range_not_recognized':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'public.range=True',b'public.range=False',1)),
 'wide_as_headered':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'branch.wide=True',b'branch.headered_name_value=True',1)),
 'byref_description_not_null':(P+'current-results/Debug/results.tsv.branches.json',lambda b:b.replace(b'public.description=N',b'public.description=EMPTY',1)),
 'restored_ui_culture_changed':(P+'current-results/Debug/results.tsv.cultures.json',lambda b:b.replace(b'restored_ui_culture',b'wrong_restored_ui_culture',1)),
 'guard_as_report':(P+'current-results/Debug/results.tsv.branches.json',lambda b:b.replace(b'guard.unmatched=True',b'report.continued=True',1))
'''
with (HERE/'test_supplement.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(head+'CASES={\n'+'\n'.join(selected)+','+extra+'\n}\n'+tail)
