from pathlib import Path
root=Path(__file__).resolve().parents[2];dest=Path(__file__).parent
for name in ['common43.py','run43.py']:
 s=(root/'evidence/04A43'/name).read_text(encoding='utf-8-sig')
 s=s.replace('04A43','04A44').replace('04A-43','04A-44').replace('common43','common44').replace('a43-','a44-').replace('runner43','runner44')
 s=s.replace("MAIN='3955a6e9142256df6bc1fd53a924010c58367b51'","MAIN='243b6dcd654767ffedb8fe3643b310c0c56cd6ac'").replace("PRIOR='1df856c47f2342e26de1115f1621722b7c0ac23e'","PRIOR='3955a6e9142256df6bc1fd53a924010c58367b51'").replace('04A44_NOT_AUTHORIZED','04A45_NOT_AUTHORIZED')
 with (dest/name.replace('43','44')).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(root/'evidence/04A43/authenticate-v1.py').read_text(encoding='utf-8-sig').replace('common43','common44').replace('04A-43','04A-44').replace('isolated43','isolated44')
s=s.replace('04A42','04A43').replace('accepted42','accepted43').replace('sealed42','sealed43').replace('merged42','merged43').replace('ACCEPTED42','ACCEPTED43').replace('Merged42','Merged43')
s=s.replace('a60e331d9b9181b62f60b9a35f6df93a0f552d1636d4529d4418a92ffb460299','d622c27c62132d985ba0ef265b646185bd257434e05bf7c5514f5645c1892050').replace("['41','42']","['41','42','43']").replace("plan_revision='2.19'","plan_revision='2.20'")
with (dest/'authenticate-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
for name in ['entry-build-v1.py']:
 s=(root/'evidence/04A43'/name).read_text().replace('common43','common44').replace('a43-','a44-')
 with (dest/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
