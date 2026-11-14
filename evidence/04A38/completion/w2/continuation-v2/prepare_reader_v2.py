from common import *
s=(HERE/'verify_slice.py').read_text(encoding='utf-8')
s=s.replace("def tokens(s):return [x",'''def tokens(s):
 # Only collapse a single-return If block to its VB single-line form;
 # condition, short-circuit operations and returned expression remain literal.
 s=re.sub(r'(?m)^(\\s*If [^\\n]+ Then)\\n\\s*(Return [^\\n]+)\\n\\s*End If',r'\\1 \\2',s)
 return [x''')
s=s.replace("observations=probe['observations']","observations=probe['results']").replace("+r['rows']","+r['cases']")
with (HERE/'verify_slice_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/independent-reader-v1.json',dict(status='FAILED_SOURCE_SYNTAX_COMPARISON',reader=pin(HERE/'verify_slice.py'),observed='CDF guard is identical single-line If Return versus original multi-line If Return End If; v1 did not canonicalize this syntax. New v2 preserves condition/return tokens. Prospective JSON keys corrected observations→results and rows→cases. No source/oracle/test assertion changed.'))
