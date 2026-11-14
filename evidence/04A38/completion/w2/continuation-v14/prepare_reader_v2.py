from common import *
book=PRIVATE/'bookkeeping-code-v1';s=book/'verify_c14_checkpoint_template_v1.py';v=s.read_text().replace("if not before[key]['fully_reconciled'] and s['kind']=='ModuleBlock':","if s['fully_reconciled'] and not before[key]['fully_reconciled'] and s['kind']=='ModuleBlock':")
assert v!=s.read_text()
with (book/'verify_c14_checkpoint_template_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
s2=book/'run_reader_draft_v1.py';v=s2.read_text().replace('verify_c14_checkpoint_template_v1','verify_c14_checkpoint_template_v2').replace('independent-draft-reader-v1.json','independent-draft-reader-v2.json')
with (book/'run_reader_draft_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
with (book/'reader-open-module-guard-correction-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(before=pin(s),after=pin(book/'verify_c14_checkpoint_template_v2.py'),reason='Apply the all-child support check only to newly CLOSED modules. The first draft incorrectly required explicitly OPEN unrelated modules to have all members closed. Keep their exact OPEN status and complement; no eligibility or count rule relaxed.',original_queries=0),f,indent=2)
