from pathlib import Path
root=Path(__file__).resolve().parents[2];dest=Path(__file__).parent
for name in ['common42.py','run42.py']:
    s=(root/'evidence/04A42'/name).read_text(encoding='utf-8-sig').replace('04A42','04A43').replace('04A.42','04A.43').replace('04A-42','04A-43').replace('common42','common43')
    s=s.replace("MAIN='1df856c47f2342e26de1115f1621722b7c0ac23e'","MAIN='3955a6e9142256df6bc1fd53a924010c58367b51'").replace("PRIOR='c6c22dbc59d6dfc61ed64f6413d8c81e7102e1dc'","PRIOR='1df856c47f2342e26de1115f1621722b7c0ac23e'").replace('04A43_NOT_AUTHORIZED','04A44_NOT_AUTHORIZED').replace("command('a41-'+name","command('a43-'+name").replace('isolated_runner41','isolated_runner43')
    with (dest/name.replace('42','43')).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
