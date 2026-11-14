"""Expand read-only triage to every captured text/error surface; retain v2."""
from pathlib import Path
HERE=Path(__file__).resolve().parent
text=(HERE/'build_matrix_v2.py').read_text(encoding='utf-8')
names=['original-identity-crosswalk.csv','genuine-preedit-case-index.json','member-preedit-witness-sets.json','authorized-late-index.json','export-comparison.json','member-behavior-evidence-target-matrix.json','branch-statement-inline-matrix.json','unresolved-branch-review-frontier.json','census-summary.json']
for name in names:
 p=Path(name);text=text.replace(name,p.stem+'-v3'+p.suffix)
a=text.index("    if r['outcome']=='RETURN_TEXT':");b=text.index('\n   pins.extend',a)
text=text[:a]+'''    texts=[]
    for surface in ['result','error','caught_error_events']:
     encoded=r[surface]
     for match in re.finditer(r'(?:^|[\\{|])S([A-Za-z0-9+/]*={0,2})(?=[|\\}]|$)',encoded):
      try:texts.append(base64.b64decode(match[1],validate=True).decode('utf-8'))
      except (ValueError,UnicodeDecodeError):pass
     if 'Exception' in encoded:texts.append(encoded)
    for text in list(texts):
     if 'Exception:' in text:
      try:texts.append(base64.b64decode(text.rsplit(':',1)[1],validate=True).decode('utf-8'))
      except (ValueError,UnicodeDecodeError):pass
    for text in set(texts):
     for member in hits[key]:error_witnesses[member][text].add(config+'/'+key)
'''+text[b:]
text=text.replace("re.search(r'(?i)must|cannot|not |requires|missing|invalid|exactly|finite|duplicate',s)", "re.search(r'(?i)\\b(?:must|cannot|not|requires|missing|invalid|exactly|finite|duplicate)\\b',s)")
with (HERE/'build_matrix_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
text=(HERE/'print_frontier.py').read_text(encoding='utf-8').replace('unresolved-branch-review-frontier.json','unresolved-branch-review-frontier-v3.json')
with (HERE/'print_frontier_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
