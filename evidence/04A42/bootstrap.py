from pathlib import Path
root=Path(__file__).resolve().parents[2]
dest=Path(__file__).parent
for name in ['common41.py','run41.py']:
    text=(root/'evidence/04A41'/name).read_text(encoding='utf-8-sig')
    text=text.replace('04A41','04A42').replace('04A.41','04A.42').replace('04A-41','04A-42').replace('common41','common42')
    text=text.replace("MAIN='c6c22dbc59d6dfc61ed64f6413d8c81e7102e1dc'","MAIN='1df856c47f2342e26de1115f1621722b7c0ac23e'")
    text=text.replace("PRIOR='bba848305c2c46ff3fc835a1132522ebfec59514'","PRIOR='c6c22dbc59d6dfc61ed64f6413d8c81e7102e1dc'")
    text=text.replace('04A42_NOT_AUTHORIZED','04A43_NOT_AUTHORIZED')
    target=dest/name.replace('41','42')
    with target.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
