from common import *
book=PRIVATE/'bookkeeping-code-v1';s=book/'verify_c14_composition_v1.py';v=s.read_text().replace("m['original_whole_prefix']","m['original_whole_prefix'].lstrip()" ).replace("m['current_whole_prefix']","m['current_whole_prefix'].lstrip()")
with (book/'verify_c14_composition_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
s2=book/'run_composition_draft_v1.py';v=s2.read_text().replace('verify_c14_composition_v1','verify_c14_composition_v2')
with (book/'run_composition_draft_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
with (book/'composition-span-indentation-correction-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(before=pin(s),after=pin(book/'verify_c14_composition_v2.py'),reason='SourceViews callable body starts at the Function token; the stored exact whole-file dispatch prefix includes its leading indentation. Remove only leading whitespace from both stored prefixes before exact replacement. Complete token/source/body checks remain.',original_queries=0),f,indent=2)
