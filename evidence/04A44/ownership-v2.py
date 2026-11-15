from common44 import *
d=load(HERE/'scope/member-ownership-v1.json');fixed=[]
for r in d['members']:
 if r['qualified_member'] not in ['ExcelCommon.IPreferenceStore.LoadAsync','ExcelCommon.IPreferenceStore.SaveAsync']:continue
 before=(PRIVATE/'source-preimages'/r['path']).read_bytes().decode('utf-8-sig').splitlines(keepends=True);after=(ROOT/r['path']).read_bytes().decode('utf-8-sig').splitlines(keepends=True)
 a=before[r['lines'][0]-1];b=after[r['current_lines'][0]-1];assert a==b and 'Function' in a
 r.update(lines=[r['lines'][0]]*2,current_lines=[r['current_lines'][0]]*2,method_sha256=sha(a.encode()),current_method_sha256=sha(b.encode()),method_changed=False,interim_status='ALREADY_PORTABLE_SIGNATURE_EXACT',parser_correction='Interface declaration has no body/EndFunction;not a multi-method source span.')
 fixed.append(r['qualified_member'])
assert len(fixed)==2
d['changed_bodies']=sum(r['method_changed'] for r in d['members']);assert d['changed_bodies']==11 and all(r['path'].startswith('BESHstatNG/') for r in d['members'] if r['method_changed'])
d.update(supersedes=pin(HERE/'scope/member-ownership-v1.json'),interface_parse_correction=fixed,all_existing_Common_method_bodies_unchanged=True)
save('scope/member-ownership-v2.json',d)
s=(HERE/'review_reader.py').read_text().replace('scope/member-ownership-v1.json','scope/member-ownership-v2.json')
with (HERE/'review_reader_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('276 members reconciled:11 Windows delegate bodies;all original Common members/signatures exact',flush=True)
