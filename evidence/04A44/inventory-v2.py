from common44 import *
import re,collections
old=load(HERE/'scope/member-inventory-v1.json');entry=load(HERE/'entry/source-preimages-v1.json');pins={r['path']:r for r in entry['files']};rows=[];texts={p:(PRIVATE/'source-preimages'/p).read_bytes().decode('utf-8-sig') for p in {r['path'] for r in old['files']}}
for path,text in texts.items():
 lines=text.splitlines(keepends=True);owners=[];owner_at={}
 for i,line in enumerate(lines):
  if re.match(r'\s*End\s+(?:Namespace|Class|Module|Structure|Interface)\b',line,re.I) and owners:owners.pop()
  m=re.match(r'\s*(?:(?:Public|Private|Friend|Protected|Partial|NotInheritable|MustInherit)\s+)*(Namespace|Class|Module|Structure|Interface)\s+([\w.]+)',line,re.I)
  if m:owners.append(m.group(2))
  owner_at[i]='.'.join(owners)
 for i,line in enumerate(lines):
  m=re.match(r'\s*(?:(?:Public|Private|Friend|Protected|Shared|ReadOnly|WriteOnly|Overrides|Overloads|Async|Default)\s+)*(Function|Sub|Property)\s+(\w+)',line,re.I)
  if not m:continue
  kind,name=m.groups();end=next((j for j in range(i+1,len(lines)) if re.match(r'\s*End '+kind+r'\b',lines[j],re.I)),i)
  prior=next(r for r in old['members'] if r['path']==path and r['member']==name and r['lines'][0]==i+1)
  rows.append(dict(prior,qualified_member=owner_at[i]+'.'+name,signature=line.strip(),source=pins[path],method_sha256=sha(''.join(lines[i:end+1]).encode()),lines=[i+1,end+1],source_capture='SEALED_CURRENT_MAIN_PRE_EDIT_04A44',baseline='Materialized591 or retained existing tests/source-only where side effects prohibited;not every untouched UI member executed.'))
save('scope/member-inventory-v2.json',dict(old,files=[pins[p] for p in texts],members=rows,supersedes=pin(HERE/'scope/member-inventory-v1.json'),clarification='Version1 slow lexical discovery overlapped WaveA edits when reading some file-level pins. Version2 independently binds every body/line to exact entry preimage. Version1 retained,not authoritative baseline.'))
print('All',len(rows),'qualified members rebound to byte-exact sealed entry sources',flush=True)
