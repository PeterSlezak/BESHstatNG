from common import *
rows=[]
for old,new,changes in [('publication_adverse_v2.py','publication_adverse_v1.py',{'verify_c11_publication_v2':'verify_c12_publication_v1'}),
    ('prove_publication_privacy_v1.py','prove_publication_privacy_v1.py',{'publication_privacy_c11_v1':'publication_privacy_c12_v1','C11 privacy':'C12 privacy'}),
    ('capture_exit_git_v1.py','capture_exit_git_v1.py',{})]:
    source=PREVIOUS/old;text=source.read_text(encoding='utf-8')
    for a,b in changes.items():text=text.replace(a,b)
    with (HERE/new).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
    rows.append(dict(before=pin(source),after=pin(HERE/new),scope='Unchanged genuine physical/cache/private/Git assertions; C12 reader/paths only.'))
save('archive-control-source-crosswalk-v1.json',dict(rows=rows,original_queries=0))
print('Archive/cache/Git genuine control sources prepared')
