from pathlib import Path
HERE=Path(__file__).resolve().parent;OLD=HERE.parent/'v5-mmrm-unmatched-profile'
text=(OLD/'close_gate.py').read_text(encoding='utf-8').replace('mmrm38-v5','mmrm38-v6').replace('(65,0),(16,0)','(54,0),(16,0)')
text=text.replace("'old-mask-gap-adverse']","'old-mask-gap-adverse','unmatched-adverse','unmatched-manifest-adverse','old-group-gap-adverse','unmatched-closeout-adverse']")
text=text.replace('[28,20,12,24,14,52,6,20,9,53,16,10,62,16,10]', '[28,20,12,24,14,52,6,20,9,53,16,10,62,16,10,65,16,12,12]')
text=text.replace('new_test_total=81,old_test_total=352','new_test_total=70,old_test_total=457')
text=text.replace("'MMRM_UNMATCHED_PROFILE_GATE_PASS", "'MMRM_INTERCEPT_GROUP_GATE_PASS").replace('MMRM_UNMATCHED_PROFILE_GATE_PASS.json','MMRM_INTERCEPT_GROUP_GATE_PASS.json')
text=text.replace('original_true_branch_line=1426,mask_false_entries_per_case=54,matched_count_per_case=0','original_true_branch_lines=[1477,1478,1479,1480],observed_index=0,resolved_fitted_name="Intercept",private_ByRef_resolvedName=None,private_ByRef_values=None')
text=text.replace('private ByRef mask54False,descriptionNothing,error recorded through public path','private ByRef resolvedName/valuesNothing,error recorded through scalar public path')
text=text.replace('81 new+352 unchanged older','70 new+457 unchanged older').replace('approvals A-E','approvals A-F')
text=text.replace("proof=inspect();publication=packet();original=source()", "proof=inspect();publication=packet();original=source();assert __import__('json').loads((HERE/'public-copy-reader.json').read_bytes())['command']['exit_code']==0")
with (HERE/'close_gate.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
