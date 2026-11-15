from common44 import *
s=(ROOT/'evidence/04A43/publication-v1.py').read_text();prefix=s[:s.index('\ndef documents():')]
prefix=prefix.replace('04A43','04A44').replace('04A-43','04A-44').replace('run43.py','run44.py')
prefix=prefix.replace("'No original", "'No original")
seal=s[s.index('\ndef seal():'):].replace('04A43','04A44').replace('04A-43','04A-44').replace('phase04A44_authorized','phase04A45_authorized').replace("HERE/'review_reader.py'","HERE/'review_reader_v3.py'").replace('reader43_final','reader44_final').replace('scope/cache-export-matrix-v1.json','scope/member-ownership-v2.json').replace('fixtures/relational-review-v1.json','fixtures/current-main-review-v1.json')
with (HERE/'publication-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(prefix+'\nfrom delivery44 import documents,package_files\n'+seal)
reader=(HERE/'review_reader_v2.py').read_text().replace("'review_reader.py'","'review_reader_v3.py'")
with (HERE/'review_reader_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(reader)
